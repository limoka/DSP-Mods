using HarmonyLib;

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
    }
}