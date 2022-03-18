using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using CommonAPI;
using CommonAPI.Systems;
using HarmonyLib;
using UnityEngine;

namespace BlueprintTweaks.FactoryUndo
{
    [RegisterPatch(BlueprintTweaksPlugin.FACTORY_UNDO)]
    public static class UITrashPanel_Patch
    {

        [HarmonyPatch(typeof(UITrashPanel), nameof(UITrashPanel._OnUpdate))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldc_I4_S, (sbyte) 122),
                    new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Input), nameof(Input.GetKeyDown), new []{typeof(KeyCode)})))
                .RemoveInstruction()
                .SetInstruction(Transpilers.EmitDelegate<Func<bool>>(() =>
                {
                    return CustomKeyBindSystem.GetKeyBind("DSPTrashButton").keyValue;
                }));
            
            return matcher.InstructionEnumeration();
        }
    }
}