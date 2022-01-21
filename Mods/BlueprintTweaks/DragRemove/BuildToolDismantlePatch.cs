using CommonAPI;
using HarmonyLib;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable RedundantAssignment

namespace BlueprintTweaks
{
    [RegisterPatch(BlueprintTweaksPlugin.DRAG_REMOVE)]
    public static class BuildToolDismantlePatch
    {

        [HarmonyPatch(typeof(BuildTool_Dismantle), "DetermineActive")]
        [HarmonyPrefix]
        public static bool CheckActive(BuildTool_Dismantle __instance, ref bool __result)
        {
            if (__instance.cursorType != 2) return true;
            
            __result = false;
            return false;
        }
        
        [HarmonyPatch(typeof(BuildTool_Dismantle), "_OnClose")]
        [HarmonyPrefix]
        public static void OnCloseBefore(BuildTool_Dismantle __instance, ref int __state)
        {
            __state = __instance.cursorType;
        }
        
        [HarmonyPatch(typeof(BuildTool_Dismantle), "_OnClose")]
        [HarmonyPostfix]
        public static void OnCloseAfter(BuildTool_Dismantle __instance, int __state)
        {
            if (__state == 2)
            {
                __instance.cursorType = 2;
            }
        }
    }
}