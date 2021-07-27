using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using ScenarioRTL;
using UnityEngine;

namespace BlueprintTweaks
{
    [HarmonyPatch]
    public static class UIItemPickerPatch
    {
        public static Func<ItemProto, bool> currentFilter;
        
        [HarmonyPatch(typeof(UIItemPicker), "RefreshIcons")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AddItemFilter(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ItemProto), nameof(ItemProto.GridIndex))),
                    new CodeMatch(OpCodes.Ldc_I4)
                ).Advance(1);
            Label label = (Label) matcher.Instruction.operand;

            matcher.Advance(-2)
                .InsertAndAdvance(Transpilers.EmitDelegate<Func<ItemProto, bool>>(proto => currentFilter == null || currentFilter.Invoke(proto)))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse, label))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_1))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_3))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldelem_Ref));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UIItemPicker), "Popup")]
        [HarmonyPrefix]
        public static void IgnoreFilter(UIItemPicker __instance)
        {
            currentFilter = null;
        }

        public static void Popup(Vector2 pos, Action<ItemProto> _onReturn, Func<ItemProto, bool> filter)
        {
            currentFilter = filter;
            if (UIRoot.instance == null)
            {
                _onReturn?.Invoke(null);
                return;
            }
            UIItemPicker itemPicker = UIRoot.instance.uiGame.itemPicker;
            if (!itemPicker.inited || itemPicker.active)
            {
                _onReturn?.Invoke(null);
                return;
            }
            itemPicker.onReturn = _onReturn;
            itemPicker._Open();
            itemPicker.pickerTrans.anchoredPosition = pos;
        }

    }
}