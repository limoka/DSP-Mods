using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace BlueprintTweaks
{
    [HarmonyPatch]
    public static class BuildTool_BlueprintPastePatch
    {
        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), "CheckBuildConditions")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .End()
                .MatchBack(false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldflda, AccessTools.Field(typeof(BuildTool_BlueprintPaste), nameof(BuildTool_BlueprintPaste.cursorTarget))));

            CodeMatcher matcher2 = matcher.Clone().MatchForward(false,
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldc_I4_S)
                , new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(BuildPreview), nameof(BuildPreview.condition))));

            object previewVariable = matcher2.Operand;

            matcher.Advance(1);

            while (!matcher.Instruction.IsStloc())
            {
                matcher.RemoveInstruction();
            }

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, previewVariable))
                .InsertAndAdvance(Transpilers.EmitDelegate<Func<BuildTool_BlueprintPaste, BuildPreview, Vector3>>((tool, preview) =>
                    preview.lpos.normalized * Mathf.Min(tool.planet.realRadius * 0.025f, 20f)));

            return matcher.InstructionEnumeration();
        }
    }
}