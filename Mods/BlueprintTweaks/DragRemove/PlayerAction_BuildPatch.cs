using HarmonyLib;

namespace BlueprintTweaks
{
    [RegisterPatch(BlueprintTweaksPlugin.DRAG_REMOVE)]
    public static class PlayerAction_BuildPatch
    {
        
        [HarmonyPatch(typeof(PlayerAction_Build), "Init")]
        [HarmonyPostfix]
        public static void InitTool(PlayerAction_Build __instance)
        {
            BuildTool[] buildTools = __instance.tools;
            BuildTool[] ourTools = new BuildTool[buildTools.Length + 1];
            buildTools.CopyTo(ourTools, 0);
            BlueprintTweaksPlugin.tool = new DragRemoveBuildTool();
            ourTools[ourTools.Length - 1] = BlueprintTweaksPlugin.tool;
            __instance.tools = ourTools;
        }
    }
}