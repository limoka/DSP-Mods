using System.IO;
using CommonAPI;
using HarmonyLib;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable RedundantAssignment

namespace BlueprintTweaks
{
    [RegisterPatch(BlueprintTweaksPlugin.BLUEPRINT_FOUNDATIONS)]
    public static class BlueprintDataPatch
    {
        [HarmonyPatch(typeof(BlueprintData), "IsNullOrEmpty")]
        [HarmonyPrefix]
        public static bool CheckNull(BlueprintData blueprint, ref bool __result)
        {
            __result = blueprint == null || !blueprint.isValid || blueprint.buildings.Length == 0 && blueprint.reforms.Length == 0;
            return false;
        }

        [HarmonyPatch(typeof(BlueprintData), "isValid", MethodType.Getter)]
        [HarmonyPrefix]
        public static bool CheckValid(BlueprintData __instance, ref bool __result)
        {
            if (__instance.reforms != null) return true;
            __result = false;
            return false;
        }

        [HarmonyPatch(typeof(BlueprintData), "isEmpty", MethodType.Getter)]
        [HarmonyPrefix]
        public static bool CheckEmpty(BlueprintData __instance, ref bool __result)
        {
            __result = (__instance.buildings == null || __instance.buildings.Length == 0) && (__instance.reforms == null || __instance.reforms.Length == 0);
            return false;
        }

        [HarmonyPatch(typeof(BlueprintData), "DataRepair")]
        [HarmonyPostfix]
        public static void Repair(BlueprintData __instance)
        {
            if (__instance.reforms == null)
            {
                __instance.reforms = new ReformData[0];
            }
        }

        [HarmonyPatch(typeof(BlueprintData), "ResetAsEmpty")]
        [HarmonyPatch(typeof(BlueprintData), "ResetContentAsEmpty")]
        [HarmonyPostfix]
        public static void Reset(BlueprintData __instance)
        {
            __instance.reforms = new ReformData[0];
        }

        [HarmonyPatch(typeof(BlueprintData), "Reset")]
        [HarmonyPostfix]
        public static void SetNull(BlueprintData __instance)
        {
            __instance.reforms = null;
        }
    }
}