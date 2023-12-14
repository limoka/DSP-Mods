using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace FasterMachines
{
    [HarmonyPatch]
    public static class UIStorageWindow_Patch
    {
        
        [HarmonyPatch(typeof(UIStorageWindow), "OnStorageIdChange")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> OnStorageIdChanged(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldloc_0),
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(ManualBehaviour), nameof(ManualBehaviour._Init)))
                )
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_1))
                .InsertAndAdvance(Transpilers.EmitDelegate<Action<UIStorageWindow, ItemProto>>((window, proto) =>
                {
                    if (proto == null) return;
                    
                    window.storageUI.colCount = proto.prefabDesc.storageCol;
                    window.storageUI.rowCount = proto.prefabDesc.storageRow;
                }));
            
            return matcher.InstructionEnumeration();
        }
        
        [HarmonyPatch(typeof(UIStorageWindow), "_OnUpdate")]
                [HarmonyTranspiler]
                public static IEnumerable<CodeInstruction> OnUpdate(IEnumerable<CodeInstruction> instructions)
                {
                    CodeMatcher matcher = new CodeMatcher(instructions)
                        .MatchForward(false,
                            new CodeMatch(OpCodes.Ldarg_0),
                            new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(UIStorageWindow), nameof(UIStorageWindow.windowTrans))),
                            new CodeMatch(OpCodes.Callvirt)
                        )
                        .Advance(1)
                        .RemoveInstructions(3)
                        .InsertAndAdvance(Transpilers.EmitDelegate<Func<UIStorageWindow, float>>(window =>
                        {
                            return window.storageUI.colCount * 50 + 80;
                        }));


                    return matcher.InstructionEnumeration();
                }
    }
}