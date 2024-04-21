using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using InventorySystem.Items.Usables;
using MedicRP;
using PlayerRoles;
using UnityEngine;
using MEC;


namespace DonitoUtilities.Loader.Features
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class CombinedHealCommand : ICommand
    {
        private readonly Dictionary<ItemType, (int, float)> TreatmentTimings = new Dictionary<ItemType, (int, float)>
        {
            { ItemType.Adrenaline, (3, 1f) },
            { ItemType.Medkit, (5, 1f) },
            { ItemType.Painkillers, (4, 1f) },
            { ItemType.None, (Plugin.Instance.Config.HealingTime, 1f) }
        };


       
        internal static int Progress;
        static bool isMedic = false;

        int maxProgress = Plugin.Instance.Config.HealingTime;
        float MedicBuff = Plugin.Instance.Config.MedicHealingBuff;

        public string Command { get; } = "heal";
        public string[] Aliases { get; } = { };
        public string Description { get; } = "Heals or gives items to the player you are looking at.";

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            try
            {
                var healer = Player.Get(sender);

                if (Plugin.Instance.Config.medicBuffRoles.Any(role => healer.CustomName.Contains(role)))
                {
                    isMedic = true;
                }

                Vector3 position = healer.Position;

                if (healer.IsScp)
                {
                    response = "Even as SCP-049, you won't heal anyone this way!";
                    return false;
                }
                if (!healer.IsHuman)
                {
                    response = "You can't heal while being dead!";
                    return false;
                }

                else if (healer.CurrentItem == null && !isMedic)
                {
                    response = "Healing with bare hands is not an option.";
                    return false;
                }
                else if (!Plugin.Instance.Config.Medkithealcommand)
                {
                    response = "The command has been disabled.";
                    return false;
                }

                if (!CanExecuteHealCommand(healer, out response))
                    return false;

                var target = FindTargetPlayer(healer);
                if (target == null)
                {
                    response = "You must be looking at a player to heal them.";
                    return false;
                }
                else if (target.Role.Team == Team.SCPs)
                {
                    response = "Healing an SCP? That's got to be a joke!";
                    return false;
                }
                if (healer == target)
                {
                    response = "You must be looking at a player to heal them.";
                    return false;
                }

                ItemType itemType = healer.CurrentItem?.Type ?? ItemType.None;

                (int maxProgress, float progressInterval) timing;
                if (TreatmentTimings.ContainsKey(itemType))
                {
                    timing = TreatmentTimings[itemType];
                }
                else if (isMedic)
                {
                    timing = (3, 1f); 
                }
                else
                {
                    response = "Your current item cannot be used for healing.";
                    return false;
                }

                var (maxProgress, progressInterval) = timing;
                if (maxProgress == 0 || progressInterval == 0)
                {
                    response = $"No defined healing time for {itemType}.";
                    return false;
                }

                var treatmentCoroutine = Timing.RunCoroutine(TreatmentProcess(healer, target, itemType, maxProgress, progressInterval));

                switch (itemType)
                {
                    case ItemType.Adrenaline:
                        response = "Adrenaline injection process started.";
                        break;
                    case ItemType.Medkit:
                        response = "Medkit healing process started.";
                        break;
                    case ItemType.Painkillers:
                        response = "Painkillers administering process started.";
                        break;
                    default:
                        response = "Healing process started.";
                        break;
                }

                return true;
            }
            catch (Exception e)
            {
                Log.Error($"{nameof(CombinedHealCommand)} error: {e}");
                response = "An error occurred while executing the command.";
                return false;
            }
        }

        private bool CanExecuteHealCommand(Player healer, out string response)
        {
            var camera = healer.CameraTransform;
            if (!Physics.Raycast(camera.position, camera.forward, out var hit, Plugin.Instance.Config.HealLookDistance))
            {
                response = "You must be looking at a player to heal them.";
                return false;
            }

            response = "";
            return true;
        }

        private Player FindTargetPlayer(Player healer)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(healer.CameraTransform.position, healer.CameraTransform.forward, out hitInfo, Plugin.Instance.Config.HealLookDistance))
            {
                var hitObject = hitInfo.collider.gameObject;
                return Player.Get(hitObject.GetComponentInParent<ReferenceHub>());
            }
            else
            {
                return null;
            }
        }

        private IEnumerator<float> TreatmentProcess(Player healer, Player target, ItemType itemType, int maxProgress, float progressInterval)
        {
            Vector3 initialHealerPosition = healer.Position;
            Progress = 0;
            float healingAmount = Plugin.Instance.Config.HealingAmount;

            string interruptionMessage = " ";
            string TargetProgressMessage = " ";
            string HealerProgressMessage = " ";
            string TargetCompletionMessage = " ";
            string HealerCompletionMessage = " ";

            switch (itemType)
            {
                case ItemType.Adrenaline:
                    interruptionMessage = "Injection interrupted -";
                    TargetCompletionMessage = $"{healer.CustomName} injected you with adrenaline";
                    HealerCompletionMessage = $"You injected adrenaline to {target.CustomName}";
                    TargetProgressMessage = $"{healer.CustomName} is injecting you with adrenaline: ";
                    HealerProgressMessage = $"You are injecting adrenaline to {target.CustomName}: ";
                    healingAmount = Adrenaline.AhpAddition;
                   
                    break;
                case ItemType.Medkit:
                    interruptionMessage = "Healing interrupted -";
                    TargetCompletionMessage = $"Your wounds were treated by {healer.CustomName}";
                    HealerCompletionMessage = $"You successfully treated {target.CustomName}'s wounds";
                    TargetProgressMessage = $"Your wounds are being treated by {healer.CustomName}: ";
                    HealerProgressMessage = $"You are treating {target.CustomName}'s wounds: ";
                    if (isMedic == false)
                    {
                        healingAmount = Medkit.HpToHeal;
                    }
                    else
                    {
                        healingAmount = (Medkit.HpToHeal + MedicBuff);
                    }
                    break;
                case ItemType.Painkillers:
                    interruptionMessage = "Painkillers administration interrupted -";
                    TargetCompletionMessage = $"{healer.CustomName} gave you painkillers";
                    HealerCompletionMessage = $"You administered painkillers to {target.CustomName}";
                    TargetProgressMessage = $"{healer.CustomName} is giving you painkillers: ";
                    HealerProgressMessage = $"You are administering painkillers to {target.CustomName}: ";
                    healingAmount = Painkillers.TotalHpToRegenerate;
                    break;
                default:
                    interruptionMessage = "Makeshift treatment interrupted -";
                    TargetCompletionMessage = $"Your wounds were temporarily treated by {healer.CustomName}";
                    HealerCompletionMessage = $"You temporarily treated {target.CustomName}'s wounds";
                    TargetProgressMessage = $"Your wounds are being temporarily treated by {healer.CustomName}: ";
                    HealerProgressMessage = $"You are temporarily treating {target.CustomName}'s wounds: ";
                    healingAmount = 20;
                    break;
            }

            while (Progress < maxProgress)
            {
                float movementThresholdSquared = Plugin.Instance.Config.HealMaxMovement;

                if (Vector3.SqrMagnitude(healer.Position - initialHealerPosition) > movementThresholdSquared)
                {
                    target.ShowHint($"<b><color=\"red\">{interruptionMessage} The healer moved.</color>", 2f);
                    healer.ShowHint($"<b><color=\"red\">{interruptionMessage} The healer moved.</color>", 2f);
                    yield break;
                }

                RaycastHit hitInfo;
                Vector3 raycastDirection = healer.CameraTransform.forward;
                float deviationAngle = Plugin.Instance.Config.HealMaxLookingRange;
                Quaternion randomRotation = Quaternion.Euler(UnityEngine.Random.Range(-deviationAngle, deviationAngle), UnityEngine.Random.Range(-deviationAngle, deviationAngle), 0f);
                raycastDirection = randomRotation * raycastDirection;

                if (!Physics.Raycast(healer.CameraTransform.position, raycastDirection, out hitInfo, Plugin.Instance.Config.HealLookDistance))
                {
                    target.ShowHint($"<b><color=\"red\">{interruptionMessage} Target lost.</color>", 2f);
                    healer.ShowHint($"<b><color=\"red\">{interruptionMessage} Target lost.</color>", 2f);
                    yield break;
                }

                var hitObject = hitInfo.collider.gameObject;
                if (Player.Get(hitObject.GetComponentInParent<ReferenceHub>()) != target)
                {
                    target.ShowHint($"<b><color=\"red\">{interruptionMessage} Target lost.</color>", 2f);
                    healer.ShowHint($"<b><color=\"red\">{interruptionMessage} Target lost.</color>", 2f);
                    yield break;
                }

                int timeLeft = maxProgress - Progress;
                Progress++;
                target.ShowHint($"<b>{TargetProgressMessage}</b> <color=red> {timeLeft} seconds </color>", progressInterval);
                healer.ShowHint($"<b>{HealerProgressMessage}</b> <color=red> {timeLeft} seconds </color>", progressInterval);
                yield return Timing.WaitForSeconds(progressInterval);
            }

            var healingItem = healer.Items.FirstOrDefault(i => i.Type == itemType);

            if (healer.CurrentItem != null)
            {
                switch (healer.CurrentItem.Type)
                {
                    case ItemType.Medkit:
                        target.Heal(healingAmount);
                        break;
                    case ItemType.Painkillers:
                        target.UseItem(ItemType.Painkillers);
                        break;
                    case ItemType.Adrenaline:
                        target.UseItem(ItemType.Adrenaline);
                        break;
                    default:
                        target.Heal(healingAmount); 
                        break;
                }
            }
            else
            {
                target.Heal(healingAmount); 
            }

            string healingitem = $"{itemType}";
            if (isMedic == false)
            {
                target.ShowHint($"<b><i><color=green>{TargetCompletionMessage}</b>", 2);
                healer.ShowHint($"<b><i><color=green>{HealerCompletionMessage}</b>", 2);
                healer.RemoveItem(healingItem);
            }
            if (isMedic == true && healer.CurrentItem == null)
            {
                target.ShowHint($"<b><i><color=green>You have been temporarily treated by {healer.CustomName}</b>", 2);
                healer.ShowHint($"<b><i><color=green>You have successfully temporarily treated {target.CustomName}</b>", 2);
                healer.RemoveItem(healingItem);
            }
            else
            {
                target.ShowHint($"<b><i><color=green>{TargetCompletionMessage}</b>", 2);
                healer.ShowHint($"<b><i><color=green>{HealerCompletionMessage}</b>", 2);
                healer.RemoveItem(healingItem);
            }
        }
    }
}
