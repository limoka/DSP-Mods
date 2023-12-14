using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using CommonAPI;
using HarmonyLib;
using NebulaAPI;

namespace BlueprintTweaks.PasteLocked
{
    [RegisterPatch(BlueprintTweaksPlugin.PASTE_LOCKED)]
    public static class BuildingParameters_Patch
    {
        [HarmonyPatch(typeof(BuildingParameters), "CanPasteToFactoryObject")]
        [HarmonyPatch(typeof(BuildingParameters), "PasteToFactoryObject")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AllowPastingLockedRecipes(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(GameHistoryData), nameof(GameHistoryData.RecipeUnlocked)))
                )
                .Repeat(codeMatcher =>
                {
                    codeMatcher.Advance(-1).SetOpcodeAndAdvance(OpCodes.Nop)
                        .SetOpcodeAndAdvance(OpCodes.Nop)
                        .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
                        .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_1));
                });

            matcher.Start().MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(GameHistoryData), nameof(GameHistoryData.ItemUnlocked)))
                )
                .Repeat(codeMatcher =>
                {
                    codeMatcher.Advance(-1).SetOpcodeAndAdvance(OpCodes.Nop)
                        .SetOpcodeAndAdvance(OpCodes.Nop)
                        .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
                        .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_1));
                });

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(BuildingParameters), "ApplyPrebuildParametersToEntity")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AllowBuildingWithLockedRecipes(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_1),
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(GameHistoryData), nameof(GameHistoryData.RecipeUnlocked)))
                )
                .Repeat(codeMatcher =>
                {
                    codeMatcher.Advance(-1).SetOpcodeAndAdvance(OpCodes.Nop)
                        .SetOpcodeAndAdvance(OpCodes.Nop)
                        .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_1));
                });

            matcher.Start().MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_3),
                    new CodeMatch(OpCodes.Ldc_I4_0),
                    new CodeMatch(OpCodes.Ldelem_I4),
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(GameHistoryData), nameof(GameHistoryData.ItemUnlocked)))
                )
                .Advance(-1).SetOpcodeAndAdvance(OpCodes.Nop)
                .SetOpcodeAndAdvance(OpCodes.Nop)
                .SetOpcodeAndAdvance(OpCodes.Nop)
                .SetOpcodeAndAdvance(OpCodes.Nop)
                .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_1));

            return matcher.InstructionEnumeration();
        }
    }
}