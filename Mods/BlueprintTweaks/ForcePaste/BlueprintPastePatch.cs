using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace BlueprintTweaks
{
    [HarmonyPatch]
    public static class BlueprintPastePatch
    {
        public static bool isEnabled;

        public static bool IsGood(this BuildPreview preview)
        {
            return preview.condition == EBuildCondition.Ok || preview.condition == EBuildCondition.NotEnoughItem;
        }

        public static bool IsCollide(this BuildPreview preview)
        {
            if (preview == null) return false;
            return preview.condition == EBuildCondition.Collide;
        }

        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), "CheckBuildConditions")]
        [HarmonyPostfix]
        public static void DontStopOnFail(BuildTool_BlueprintPaste __instance, ref bool __result)
        {
            if (__result || !isEnabled) return;

            __result = true;
            __instance.actionBuild.model.cursorState = 0;
        }

        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), "Operating")]
        [HarmonyPostfix]
        public static void AllowToTryAgain(BuildTool_BlueprintPaste __instance)
        {
            if (!__instance.buildCondition && VFInput.blueprintPasteOperate0.onDown)
            {
                __instance.OperatingPrestage();
            }
        }

        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), "CreatePrebuilds")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> RemoveBrokenConnections(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(BuildPreview), nameof(BuildPreview.bpgpuiModelId))),
                    new CodeMatch(OpCodes.Ldc_I4_0)
                ).Advance(1);
            Label label = (Label) matcher.Instruction.operand;

            matcher.Advance(-2)
                .InsertAndAdvance(Transpilers.EmitDelegate<Func<BuildPreview, bool>>(bp =>
                {
                    if (!isEnabled && !BlueprintTweaksPlugin.nebulaInstalled) return true;

                    if (bp.desc.multiLevel)
                    {
                        BuildPreview current = bp;
                        while (current.input != null)
                        {
                            if (!current.input.IsGood()) return false;

                            current = current.input;
                        }
                    }

                    if (bp.desc.isInserter)
                    {
                        if (bp.input != null && !bp.input.IsGood())
                        {
                            return false;
                        }

                        if (bp.output != null && !bp.output.IsGood())
                        {
                            return false;
                        }
                    }

                    return true;
                }))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse, label))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_3));

            return matcher.InstructionEnumeration();
        }
    }
}