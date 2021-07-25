using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace BlueprintTweaks
{
    [HarmonyPatch]
    public static class CameraFixPatch
    {
        public static bool mode;
        
        [HarmonyPatch(typeof(GameCamera), "Logic")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> FixCamera(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlayerController), nameof(PlayerController.actionBuild))),
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(PlayerAction_Build), "get_blueprintMode"))
                )
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldc_I4_4)
                )
                .SetInstruction(Transpilers.EmitDelegate<Func<int>>(() => mode ? 4 : 1));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(PlayerMove_Drift), "GameTick")]
        [HarmonyPatch(typeof(PlayerMove_Fly), "GameTick")]
        [HarmonyPatch(typeof(PlayerMove_Walk), "GameTick")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> FixMovement(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlayerController), nameof(PlayerController.actionBuild))),
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(PlayerAction_Build), "get_blueprintMode")),
                    new CodeMatch(OpCodes.Ldc_I4_0),
                    new CodeMatch(OpCodes.Cgt_Un)
                ).Advance(1)
                .InsertAndAdvance(Transpilers.EmitDelegate<Func<bool, bool>>(isBp => isBp && mode));
            
            return matcher.InstructionEnumeration();
        }
    }
}