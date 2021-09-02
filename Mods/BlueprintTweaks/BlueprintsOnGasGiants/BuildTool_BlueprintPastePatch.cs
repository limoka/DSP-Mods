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
                    new CodeMatch(OpCodes.Ldflda, AccessTools.Field(typeof(BuildTool_BlueprintPaste), nameof(BuildTool_BlueprintPaste.cursorTarget))))
                .Advance(1);

            while (matcher.Opcode != OpCodes.Stloc_S)
            {
                matcher.RemoveInstruction();
            }

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 4))
                .InsertAndAdvance(Transpilers.EmitDelegate<Func<BuildTool_BlueprintPaste, BuildPreview, Vector3>>((tool, preview) => preview.lpos.normalized * Mathf.Min(tool.planet.realRadius * 0.025f, 20f)));

            return matcher.InstructionEnumeration();
        }
    }
}