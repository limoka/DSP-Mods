using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace GigaStations
{
    [HarmonyPatch]
    public static class UIControlPanelStationInspector_Patch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(UIControlPanelStationInspector), "_OnCreate")]
        public static IEnumerable<CodeInstruction> EditUIControlPanel(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_6));

            var needCount = Mathf.Max(GigaStationsPlugin.plsMaxSlots, GigaStationsPlugin.ilsMaxSlots);

            matcher.Repeat(codeMatcher => { codeMatcher.SetInstruction(new CodeInstruction(OpCodes.Ldc_I4_S, needCount)); });

            return matcher.InstructionEnumeration();
        }

        private static readonly Dictionary<OpCode, int> storeToOperand = new Dictionary<OpCode, int>
        {
            { OpCodes.Stloc_0, 0 },
            { OpCodes.Stloc_1, 1 },
            { OpCodes.Stloc_2, 2 },
            { OpCodes.Stloc_3, 3 }
        };

        private static int ToLocalNum(this CodeInstruction codeInstruction)
        {
            if (storeToOperand.TryGetValue(codeInstruction.opcode, out var localNum))
            {
                return localNum;
            }

            return (int)codeInstruction.operand;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(UIControlPanelStationInspector), "_OnUpdate")]
        public static IEnumerable<CodeInstruction> HandleUIResize(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldc_I4_0),
                    new CodeMatch(i => i.IsStloc())
                );
            
            object extraSizeLocalOperand = matcher.Instruction.ToLocalNum();

            matcher
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(Transpilers.EmitDelegate<Action<UIControlPanelStationInspector>>(inspector =>
                {
                    inspector.stationGroupContentRect.anchorMin = new Vector2(0, 1);
                }));
            
            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld,
                    AccessTools.Field(typeof(UIControlPanelStationInspector), nameof(UIControlPanelStationInspector.veinCollectorPanel))),
                new CodeMatch(OpCodes.Callvirt));

            matcher.Advance(1);

            matcher.InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldloca_S, extraSizeLocalOperand))
                .InsertAndAdvance(
                    Transpilers.EmitDelegate<StationEditPatch.RefAction<UIControlPanelStationInspector, int>>(
                        (UIControlPanelStationInspector inspector, ref int extraSize) =>
                        {
                            RectTransform rectTransform = inspector.storageUIPrefab.transform as RectTransform;
                            var elementHeight = rectTransform!.sizeDelta.y + 6f;

                            var itemCount = inspector.station.storage.Length;
                            var extraCount = itemCount - 6;
                            if (extraCount <= 0) return;

                            inspector.stationGroupContentRect.anchorMin = new Vector2(0, 0);
                            extraSize = Mathf.RoundToInt(extraCount * elementHeight) + 30;
                        }
                    ));

            return matcher.InstructionEnumeration();
        }
    }
}