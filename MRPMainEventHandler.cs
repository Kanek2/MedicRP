using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Player;
using HarmonyLib;
using Interactables.Interobjects;
using InventorySystem.Items.Usables;
using MEC;
using UnityEngine;
using Player = Exiled.API.Features.Player;

namespace MedicRP
{
    public class MRPMainEventHandler
    {
        private readonly Dictionary<Player, float> healingPotential = new();
        private readonly Dictionary<ushort, int> medkitUses = new();
        private readonly Dictionary<Player, CoroutineHandle> activeHeals = new();

        private float DefaultPotential = Plugin.Instance.Config.DefaultPotential;
        private int MaxMedkitUses = Plugin.Instance.Config.MaxMedkitUses;
        private float HealingReduction = Plugin.Instance.Config.HealingReduction;

        public void OnUsingItem(UsingItemEventArgs ev)
        {
            switch (ev.Item.Type)
            {
                case ItemType.Medkit:
                    ev.IsAllowed = true;
                    StartMedkitUse(ev.Player, ev.Item);
                    break;

                /*case ItemType.Painkillers:
                    ev.IsAllowed = true;
                    StartPainkillerUse(ev.Player, ev.Item);
                    break;

                case ItemType.Adrenaline:
                    ev.IsAllowed = true;
                    StartAdrenalineUse(ev.Player, ev.Item);
                    break;*/

                case ItemType.SCP500:
                    ev.IsAllowed = true;
                    StartScp500Use(ev.Player, ev.Item);
                    break;

                case ItemType.None:
                    break;
            }
        }

        public void OnTogglingNoClip(TogglingNoClipEventArgs ev)
        {
            if (IsMedic(ev.Player))
            {
                Player target = GetHealTarget(ev.Player);
                if (target != null && target != ev.Player)
                    StartMakeshiftHeal(ev.Player, target);
            }
        }

        private void StartMakeshiftHeal(Player medic, Player target)
        {
            if (activeHeals.TryGetValue(medic, out var handle))
                Timing.KillCoroutines(handle);

            activeHeals[medic] = Timing.RunCoroutine(MakeshiftHealProcess(medic, target));
        }

        private IEnumerator<float> MakeshiftHealProcess(Player medic, Player target)
        {
            float startTime = Time.time;
            medic.EnableEffect(EffectType.Slowness, 50);

            while (Time.time - startTime < Plugin.Instance.Config.HealingTime)
            {
                if (!ValidateHeal(medic, null, target))
                    yield break;

                medic.ShowHint("<color=yellow>Makeshift healing... [" + (Time.time - startTime).ToString("N1") + "s]</color>", 1.1f);
                if (target != medic)
                {
                    target.ShowHint("<color=yellow>" + medic.Nickname + " is healing you... [" + (Time.time - startTime).ToString("N1") + "s]</color>", 1.1f);
                }
                yield return Timing.WaitForSeconds(1f);
            }

            target.Health = Mathf.Min(target.MaxHealth, target.Health + Plugin.Instance.Config.HealingAmount);
            medic.DisableEffect(EffectType.Slowness);
            activeHeals.Remove(medic);
        }

        private void StartScp500Use(Player user, Item item)
        {
            if (activeHeals.TryGetValue(user, out var handle))
                Timing.KillCoroutines(handle);

            Player target = GetHealTarget(user) ?? user;
            activeHeals[user] = Timing.RunCoroutine(Scp500HealProcess(user, item, target));
        }

        private IEnumerator<float> Scp500HealProcess(Player user, Item item, Player target)
        {
            yield return Timing.WaitForSeconds(2.5f);
            target.Health = target.MaxHealth;
            healingPotential[target] = DefaultPotential;
            activeHeals.Remove(user);
        }

        private void StartAdrenalineUse(Player user, Item item)
        {
            if (activeHeals.TryGetValue(user, out var handle))
                Timing.KillCoroutines(handle);

            Player target = GetHealTarget(user) ?? user;
            activeHeals[user] = Timing.RunCoroutine(AdrenalineProcess(user, item, target));
        }

        private IEnumerator<float> AdrenalineProcess(Player user, Item item, Player target)
        {
            yield return Timing.WaitForSeconds(1.5f);
            activeHeals.Remove(user);
        }

        private Player GetHealTarget(Player healer)
        {
            Ray ray = new Ray(healer.CameraTransform.position, healer.CameraTransform.forward);
            RaycastHit[] hits = Physics.RaycastAll(ray, Plugin.Instance.Config.HealLookDistance);
            Player target = null;
            float closestDistance = float.MaxValue;

            foreach (var hit in hits)
            {
                var potentialTarget = Player.Get(hit.collider);
                if (potentialTarget != null && potentialTarget != healer && potentialTarget.IsAlive)
                {
                    if (hit.distance < closestDistance)
                    {
                        closestDistance = hit.distance;
                        target = potentialTarget;
                    }
                }
            }
            return target;
        }

        private void StartPainkillerUse(Player user, Item item)
        {
            if (activeHeals.TryGetValue(user, out var handle))
                Timing.KillCoroutines(handle);

            Player target = GetHealTarget(user) ?? user;
            activeHeals[user] = Timing.RunCoroutine(PainkillerProcess(user, item, target));
        }

        private IEnumerator<float> PainkillerProcess(Player user, Item item, Player target)
        {
            float startTime = Time.time;
            var potential = GetPotential(user);

            while (Time.time - startTime < 3f)
            {
                if (!ValidateHeal(user, item, target))
                    yield break;
                user.ShowHint("<color=#FFA500>Using painkillers... [" + (((Time.time - startTime) / 3) * 100).ToString("N0") + "%]</color>", 1.1f);
                if (user != target)
                {
                    target.ShowHint("<color=#FFA500>" + user.Nickname + " is giving you painkillers... [" + (((Time.time - startTime) / 3) * 100).ToString("N0") + "%]</color>", 1.1f);
                }
                yield return Timing.WaitForSeconds(1f);
            }

            target.Health = Mathf.Min(target.MaxHealth, target.Health + 15f);
            ModifyPotential(user, 5f);
            user.RemoveItem(item);
            activeHeals.Remove(user);
        }

        private void StartMedkitUse(Player user, Item item)
        {
            if (activeHeals.TryGetValue(user, out var handle))
                Timing.KillCoroutines(handle);

            if (!medkitUses.ContainsKey(item.Serial))
                medkitUses[item.Serial] = MaxMedkitUses;

            if (medkitUses[item.Serial] <= 0)
            {
                user.ShowHint("<color=red>Medkit empty!</color>", 3f);
                return;
            }

            var target = GetHealTarget(user) ?? user;
            activeHeals[user] = Timing.RunCoroutine(MedkitProcess(user, item, target));
        }

        private IEnumerator<float> MedkitProcess(Player user, Item item, Player target)
        {
            user.EnableEffect(EffectType.Slowness, 50);

            if (Mathf.Approximately(target.Health, target.MaxHealth))
            {
                if (user == target)
                {
                    user.ShowHint("<color=yellow>Your health is full!</color>", 3f);
                    user.DisableEffect(EffectType.Slowness);
                }
                else
                {
                    user.ShowHint("<color=yellow>" + target.Nickname + "'s health is full!</color>", 3f);
                }
                yield break;
            }

            float potential = GetPotential(user);
            int usesLeft = medkitUses[item.Serial];

            for (int i = 0; i < 4; i++)
            {
                if (!ValidateHeal(user, item, target))
                    yield break;
                

                float progress = (i + 1) / 4f * 100f;

                if (user == target)
                {
                    user.ShowHint(
                        "<color=#00ffff>Using medkit... [</color><color=green>" + progress.ToString("N0") +
                        "%</color><color=#00ffff>]\nYour potential: </color><color=green>" + potential.ToString("N0") +
                        "%</color>\n<color=#00ffff>Medkit uses left: </color><color=green>" + usesLeft + "</color>",
                        1.1f
                    );
                }
                else
                {
                    potential = GetPotential(target);
                    target.ShowHint(
                        "<color=#00ffff>" + user.Nickname + " is healing you... [</color><color=green>" + progress.ToString("N0") +
                        "%</color><color=#00ffff>]\nYour potential: </color><color=green>" + potential.ToString("N0") +
                        "%</color>\n<color=#00ffff>Medkit uses left: </color><color=green>" + usesLeft + "</color>",
                        1.1f
                    );
                }

                yield return Timing.WaitForSeconds(1f);
            }

            ApplyMedkitHeal(user, target, item);
            user.DisableEffect(EffectType.Slowness);
            user.ShowHint("<color=green>Healing completed!</color>", 3f);
            activeHeals.Remove(user);
        }

        private void ApplyMedkitHeal(Player user, Player target, Item item)
        {
            float healAmount = 20f * (GetPotential(user) / 100f);
            if (IsMedic(user))
                healAmount += Plugin.Instance.Config.MedicHealingBuff;

            target.Health = Mathf.Min(target.MaxHealth, target.Health + healAmount);
            ModifyPotential(target, HealingReduction);
            medkitUses[item.Serial]--;

            if (medkitUses[item.Serial] <= 0)
                user.RemoveItem(item);
        }

        private bool IsMedic(Player player) =>
            Plugin.Instance.Config.medicBuffRoles.Any(role => player.Role.Name.ToUpper().Contains(role.ToUpper()));

        private float GetPotential(Player player)
        {
            if (!healingPotential.TryGetValue(player, out var potential))
                potential = DefaultPotential;

            return Mathf.Clamp(potential, 0f, 100f);
        }

        private void ModifyPotential(Player player, float amount)
        {
            healingPotential[player] = Mathf.Clamp(GetPotential(player) - amount, 0f, 100f);
        }

        private bool ValidateHeal(Player user, Item item, Player target)
        {
            if (item != null && user.CurrentItem?.Serial != item.Serial)
                return false;

            if (item == null && (target == user || !IsMedic(user)))
                return false;

            if (!target.IsAlive || Vector3.Distance(user.Position, target.Position) > Plugin.Instance.Config.HealMaxMovement)
                return false;

            return true;
        }

        public void OnSelectedItem(ChangingItemEventArgs ev)
        {
            if (ev.Item?.Type == ItemType.Medkit && medkitUses.TryGetValue(ev.Item.Serial, out var uses))
                ev.Player.ShowHint("<color=#00ffff>Medkit uses remaining: </color><color=green>" + uses + "</color>", 3f);
        }

        public void OnPickingUpItem(PickingUpItemEventArgs ev)
        {
            if (ev.Pickup.Type == ItemType.Medkit)
                medkitUses[ev.Pickup.Serial] = MaxMedkitUses;
        }

        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            healingPotential[ev.Player] = DefaultPotential;
            if (activeHeals.TryGetValue(ev.Player, out var handle))
                Timing.KillCoroutines(handle);
        }

        public void OnUsedItem(UsingItemCompletedEventArgs ev)
        {
            if (ev.Item?.Type == ItemType.Medkit)
                ev.IsAllowed = false;
        }

        public void Dispose()
        {
            foreach (var handle in activeHeals.Values) Timing.KillCoroutines(handle);
            healingPotential.Clear();
            medkitUses.Clear();
            activeHeals.Clear();
        }

        [HarmonyPatch]
        private static class HarmonyPatches
        {
            [HarmonyPatch(typeof(Medkit), nameof(Medkit.OnEffectsActivated))]
            [HarmonyPrefix]
            private static bool BlockMedkit() => false;

            [HarmonyPatch(typeof(Painkillers), nameof(Painkillers.OnEffectsActivated))]
            [HarmonyPrefix]
            private static bool BlockPainkillers() => false;
        }
    }
}