using HarmonyLib;
using InventorySystem.Items.Usables;

namespace MedicRP.Patches
{
    [HarmonyPatch(typeof(Medkit))]
    public static class MedkitPatches
    {

        [HarmonyPatch(nameof(Medkit.OnEffectsActivated))]
        [HarmonyPrefix]
        public static bool OnEffectsActivatedPatch(Medkit __instance)
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(Painkillers), nameof(Painkillers.OnEffectsActivated))]
    public static class PainkillerPatches
    {
        [HarmonyPrefix]
        public static bool OnEffectsActivatedPatch(Painkillers __instance)
        {
            
            return false;
        }
    }
}