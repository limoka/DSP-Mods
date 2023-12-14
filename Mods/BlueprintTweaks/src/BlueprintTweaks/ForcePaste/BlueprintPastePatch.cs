using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using CommonAPI.Systems;
using HarmonyLib;
using NebulaAPI;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace BlueprintTweaks
{
    [HarmonyPatch]
    public static class BlueprintPastePatch
    {
        public static bool isEnabled;

        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), nameof(BuildTool_BlueprintPaste.OperatingPrestage))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> OperatingPrestagePatch(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(BuildTool_BlueprintPaste), nameof(BuildTool_BlueprintPaste.buildCondition)))
                )
                .Advance(1)
                .InsertAndAdvance(Transpilers.EmitDelegate<Func<bool, bool>>(condition =>
                {
                    if (BlueprintTweaksPlugin.forcePasteEnabled.Value)
                    {
                        if (!condition && isEnabled)
                        {
                            return true;
                        }
                    }

                    return condition;
                }))
                .MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_0))
                .SetInstruction(Transpilers.EmitDelegate<Func<bool>>(() => isEnabled));

            return matcher.InstructionEnumeration();
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
    }
}