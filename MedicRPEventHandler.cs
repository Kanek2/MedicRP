
using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Player;
using HarmonyLib;
using InventorySystem.Items.Usables;
using MEC;
using MedicRP;
using MedicRP.Localization;
using UnityEngine;

namespace MedicRP
{
    public class MedicRPEventHandler
    {
        public bool IsDebug = false;

        private readonly Tranlationmanager _loc;
        private readonly Config _cfg;

        private readonly Dictionary<Player, float> _potential = new();
        private readonly Dictionary<ushort, int> _medkitUses = new();
        private readonly Dictionary<Player, CoroutineHandle> _running = new();
        private readonly Dictionary<Player, byte> _savedSlowness = new();

        private float MedkitDuration = MedicRP.Instance.Config.HealingTime;
        private float PainkillerDuration = MedicRP.Instance.Config.PainkillerDuration;
        private int MaxMedkitUses = MedicRP.Instance.Config.MaxMedkitUses;
        private float MedkitTotalHeal = MedicRP.Instance.Config.HealingAmount;
        private float PainkillerTotalHeal = MedicRP.Instance.Config.PainkillerTotalHeal;
        private float PotentialLossPerMedkitUse = MedicRP.Instance.Config.PotentialLossPerMedkitUse;
        private float PotentialLossPainkiller = MedicRP.Instance.Config.PotentialLossPainkiller;
        private float HealDistance = MedicRP.Instance.Config.HealDistance;

        public MedicRPEventHandler(Config cfg, Tranlationmanager loc)
        {
            _cfg = cfg;
            _loc = loc;
        }

        #region Registration

        public void Register()
        {
            Exiled.Events.Handlers.Player.UsingItem += OnUsingItem;
            Exiled.Events.Handlers.Player.UsingItemCompleted += OnUsingItemC;
            Exiled.Events.Handlers.Player.CancelledItemUse += OnCUsingItem;
            Exiled.Events.Handlers.Player.ChangingItem += OnChangingItem;
            Exiled.Events.Handlers.Player.PickingUpItem += OnPickingUpItem;
            Exiled.Events.Handlers.Player.ChangingRole += OnChangingRole;
            Exiled.Events.Handlers.Player.Died += OnDeath;
        }

        public void Unregister()
        {
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingItem;
            Exiled.Events.Handlers.Player.UsingItemCompleted -= OnUsingItemC;
            Exiled.Events.Handlers.Player.CancelledItemUse -= OnCUsingItem;
            Exiled.Events.Handlers.Player.ChangingItem -= OnChangingItem;
            Exiled.Events.Handlers.Player.PickingUpItem -= OnPickingUpItem;
            Exiled.Events.Handlers.Player.ChangingRole -= OnChangingRole;
            Exiled.Events.Handlers.Player.Died -= OnDeath;
        }

        #endregion

        #region Potential helpers

        private float GetPotential(Player p)
        {
            if (!_potential.TryGetValue(p, out var v))
                v = _cfg.DefaultPotential;
            v = Mathf.Clamp(v, 0f, 100f);
            _potential[p] = v;
            return v;
        }

        private void SpendPotential(Player p, float amt) =>
            _potential[p] = Mathf.Clamp(GetPotential(p) - amt, 0f, 100f);

        private void ResetPotential(Player p) => _potential[p] = _cfg.DefaultPotential;

        #endregion

        #region Core helpers

        private static bool NeedsHeal(Player t) => t.Health < t.MaxHealth;

        private Player GetLookTarget(Player healer)
        {
            var origin = healer.CameraTransform.position + healer.CameraTransform.forward * 0.5f;
            var hits = Physics.RaycastAll(origin, healer.CameraTransform.forward, HealDistance)
                .OrderBy(h => h.distance);
            foreach (var h in hits)
            {
                var tgt = Player.Get(h.collider);
                if (tgt is { IsAlive: true } && tgt != healer)
                    return tgt;
            }

            return null;
        }

        private void Interrupt(Player p)
        {
            if (_running.TryGetValue(p, out var h)) Timing.KillCoroutines(h);
            p.DisableEffect(EffectType.Slowness);
            
            if (_savedSlowness.TryGetValue(p, out var slowness))
            {
                if (slowness > 0)
                    p.EnableEffect(EffectType.Slowness, slowness);
                _savedSlowness.Remove(p);
            }
            
            _running.Remove(p);
        }
        
    

        #endregion

        /* ==================================================================== */
        /* ITEM ENTRY POINT                                                     */
        /* ==================================================================== */
        private void OnUsingItem(UsingItemEventArgs ev)
        {
            
         
            switch (ev.Item.Type)
            {
                case ItemType.Medkit:
                    StartMedkit(ev.Player, ev.Item);
                    ev.Usable.MaxCancellableTime = 0.5f;
                    break;

                case ItemType.Painkillers:
                    StartPainkiller(ev.Player, ev.Item);
                    break;

                case ItemType.SCP500:
                    ResetPotential(ev.Player);
                    ev.Player.ShowHint(_loc.T(ev.Player, "SCP500_Reset", GetPotential(ev.Player).ToString("N0")), 4f);
                    break;
            }
        }
        
        public void OnUsingItemC(UsingItemCompletedEventArgs ev)
        {
            if (ev.Item.Type == ItemType.Medkit)
            {
                ev.IsAllowed = false; 
               
            }
        }

        /* ==================================================================== */
        /* PAINKILLERS                                                          */
        /* ==================================================================== */
        private void StartPainkiller(Player user, Item item)
        {
            Interrupt(user);
            _running[user] = Timing.RunCoroutine(PainkillerRoutine(user, item));
        }

        private IEnumerator<float> PainkillerRoutine(Player user, Item item)
        {
            var start = Time.time;
            var pot = GetPotential(user);

            while (Time.time - start < PainkillerDuration)
            {
                if (!user.IsAlive) yield break;
                float prg = (Time.time - start) / PainkillerDuration * 100f;
                user.ShowHint(_loc.T(user, "PainkillerProgress", prg.ToString("N0"), pot.ToString("N0")), 1.1f);
                yield return 0.1f;
            }

         
            
            SpendPotential(user, PotentialLossPainkiller);
            user.AddAhp(10f, 10f, 2f, 1f,0f, false);
           
            float heal = PainkillerTotalHeal * pot / 100f;
            float maxHealRate = 1f;
            float healDuration = heal; // Czas trwania leczenia w sekundach


            AnimationCurve animationCurve = AnimationCurve.Constant(0f, healDuration, 1f);
            RegenerationProcess reg = new RegenerationProcess(animationCurve, 1f, 1f);
            UsableItemsController.GetHandler(user.ReferenceHub).ActiveRegenerations.Add(reg);

            Timing.CallDelayed(healDuration + 0.5f, () => {
                if (UsableItemsController.GetHandler(user.ReferenceHub).ActiveRegenerations.Contains(reg))
                    UsableItemsController.GetHandler(user.ReferenceHub).ActiveRegenerations.Remove(reg);
            });
            user.RemoveItem(item);
            user.ShowHint(_loc.T(user, "PainkillerDone", heal.ToString("N1"), GetPotential(user).ToString("N0")), 5f);
            _running.Remove(user);
        }

        /* ==================================================================== */
        /* MEDKIT                                                               */
        /* ==================================================================== */
        private void StartMedkit(Player healer, Item medkit)
        {
            Interrupt(healer);
            ushort id = medkit.Serial;
            if (!_medkitUses.ContainsKey(id)) _medkitUses[id] = MaxMedkitUses;
            if (_medkitUses[id] <= 0)
            {
                healer.ShowHint(_loc.T(healer, "MedkitEmpty"), 3f);
                return;
            }

            var target = GetLookTarget(healer) ?? healer;
            if (!NeedsHeal(target))
            {
                healer.ShowHint(
                    _loc.T(healer, target == healer ? "SelfFullHealth" : "TargetFullHealth", target.Nickname), 3f);
                return;
            }

            _running[healer] = Timing.RunCoroutine(MedkitRoutine(healer, target, medkit));
        }

        private IEnumerator<float> MedkitRoutine(Player healer, Player target, Item medkit)
        {
            ushort id = medkit.Serial;
            float pot = GetPotential(target);
            float time = 0f;
            float duration = MedkitDuration;
            
            _savedSlowness[healer] = healer.GetEffect(EffectType.Slowness)?.Intensity ?? 0;
            
            healer.EnableEffect(EffectType.Slowness, 50);
          

            try
            {
                while (time < duration)
                {
                    if (!Validate(healer, medkit, target)) yield break;

                    float pct = time / duration * 100f;
                    healer.ShowHint(
                        _loc.T(healer, "MedkitProgress", target.Nickname, pct.ToString("N0"), pot.ToString("N0"),
                            _medkitUses[id] - 1), 1.1f); 
                  
                    yield return Timing.WaitForSeconds(1f);
                    time += 1f;
                }
            }
            finally
            {
                healer.DisableEffect(EffectType.Slowness);
                if (_savedSlowness.TryGetValue(healer, out var slowness))
                {
                    if (slowness > 0)
                        healer.EnableEffect(EffectType.Slowness, slowness);
                    _savedSlowness.Remove(healer);
                }
            }

            float rawHeal = MedkitTotalHeal * pot / 100f;
            if (_cfg.MedicBuffRoles.Any(r => healer.CustomName.IndexOf(r, StringComparison.OrdinalIgnoreCase) >= 0))
                rawHeal += MedicRP.Instance.Config.MedicHealingBuff;
            float actual = Mathf.Min(rawHeal, target.MaxHealth - target.Health);
            if (actual <= 0f) yield break;

            target.Health += actual;
            SpendPotential(target, PotentialLossPerMedkitUse);
            _medkitUses[id]--;

            healer.ShowHint(
                _loc.T(healer, "MedkitDoneSelf", target.Nickname, actual.ToString("N1"),
                    GetPotential(healer).ToString("N0")), 5f);
            if (target != healer)
                target.ShowHint(_loc.T(target, "MedkitDoneTarget", healer.Nickname, actual.ToString("N1")), 5f);

            if (_medkitUses[id] <= 0)
            {
                Timing.CallDelayed(0.1f, () => healer.RemoveItem(medkit));
                _medkitUses.Remove(id);
            }

            _running.Remove(healer);
        }

        /* ==================================================================== */
        /* VALIDATION                                                           */
        /* ==================================================================== */
        private bool Validate(Player healer, Item itm, Player target)
        {
            if (!healer.IsAlive) return false;
            if (healer.CurrentItem == null || healer.CurrentItem.Serial != itm.Serial) return false;
            if (target != healer &&
                (!target.IsAlive || Vector3.Distance(healer.Position, target.Position) > HealDistance)) return false;
            return true;
        }

        /* ==================================================================== */
        /* OTHER EXILED EVENTS                                                  */
        /* ==================================================================== */
        private void OnChangingItem(ChangingItemEventArgs ev)
        {
            if (ev.Item?.Type == ItemType.Medkit)
            {
                ushort id = ev.Item.Serial;
                if (!_medkitUses.ContainsKey(id)) _medkitUses[id] = MaxMedkitUses;
                ev.Player.ShowHint(
                    _loc.T(ev.Player, "MedkitStatus", GetPotential(ev.Player).ToString("N0"), _medkitUses[id]), 5f);
            }
        }

        private void OnPickingUpItem(PickingUpItemEventArgs ev)
        {
            if (ev.Pickup.Type != ItemType.Medkit) return;
            ushort id = ev.Pickup.Serial;
            if (!_medkitUses.ContainsKey(id)) _medkitUses[id] = MaxMedkitUses;
            ev.Player.ShowHint(_loc.T(ev.Player, "PickedUpMedkit", _medkitUses[id]), 4f);
        }
        
        private void OnCUsingItem(CancelledItemUseEventArgs ev)
        {
            if (ev.Item.Type == ItemType.Medkit || ev.Item.Type == ItemType.Painkillers)
            {
                Interrupt(ev.Player);
                
                ev.Player.ShowHint(_loc.T(ev.Player, "HealingInterrupted"), 3f);
            }
        }

        private void OnChangingRole(ChangingRoleEventArgs ev)
        {
            ResetPotential(ev.Player);
            Interrupt(ev.Player);
        }

        private void OnDeath(DiedEventArgs ev)
        {
            Interrupt(ev.Player);
            _potential.Remove(ev.Player);
        }

        public void Dispose()
        {
            foreach (var h in _running.Values) Timing.KillCoroutines(h);
            _running.Clear();
            _potential.Clear();
            _medkitUses.Clear();
        }

      
    }
}