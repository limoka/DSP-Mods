using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace BlueprintTweaks.Patches
{
    [HarmonyPatch]
    public static class BuildTool_BlueprintPaste_Patch
    {

        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), nameof(BuildTool_BlueprintPaste.DeterminAnchorType))]
        [HarmonyPostfix]
        public static void OnChangedAnchor(BuildTool_BlueprintPaste __instance, bool __result)
        {
            if (__result)
            {
                __instance.blueprint.anchorType = __instance.anchorType;
                __instance.uiInspector.Refresh(true, true, true);
            }
        }

        [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.OpenBlueprintPasteMode))]
        [HarmonyPostfix]
        public static void OnOpenBlueprint(PlayerController __instance)
        {
            BuildTool_BlueprintPaste tool = __instance.actionBuild.blueprintPasteTool;
            if (tool.blueprint != null) 
                tool.anchorType = tool.blueprint.anchorType;
        }
        
        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), nameof(BuildTool_BlueprintPaste._OnOpen))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> DontResetAnchorOnOpen(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo anchorType = AccessTools.Field(typeof(BuildTool_BlueprintPaste), nameof(BuildTool_BlueprintPaste.anchorType));

            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldc_I4_0),
                    new CodeMatch(OpCodes.Stfld, anchorType)
                )
                .RemoveInstructions(3);

            return matcher.InstructionEnumeration();
        }
    }
}