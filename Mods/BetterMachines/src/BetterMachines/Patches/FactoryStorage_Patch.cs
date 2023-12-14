using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace FasterMachines
{
    [HarmonyPatch]
    public static class FactoryStorage_Patch
    {
        public delegate void RefAction<T1, T2>(ref T1 arg1, ref T2 arg2);


        [HarmonyPatch(typeof(FactoryStorage), "GameTick")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> ChangeFunction(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .End()
                .MatchBack(false,
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(OpCodes.Add)
                )
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(FactoryStorage), nameof(FactoryStorage.tankPool))))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 4))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldelema, typeof(TankComponent)))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 5))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldelema, typeof(AnimData)))
                .InsertAndAdvance(Transpilers.EmitDelegate<RefAction<TankComponent, AnimData>>((ref TankComponent tank, ref AnimData anim) =>
                {
                    if (tank.fluidCapacity > 11000)
                    {
                        anim.time = InverseFunction(anim.time);
                    }
                }));


            return matcher.InstructionEnumeration();
        }

        private static float InverseFunction(float x)
        {
            // Inverse function to 2 * x^3 + -3.15 * x^2 + 1.76 * x (approx) running inside glass shader.
            float a = 149121 / 32000f - 27 / 2f * x;
            float b = Mathf.Pow(-27 / 4f * x + Mathf.Sqrt(a * a + 132651 / 8192000f) / 2f + 149121 / 64000f, 1.0f / 3.0f);
            float c = -b / 3f + 21 / 40f + 13.5f / 320f / b;

            if (c > 0.9999f)
            {
                return 0.1466f + x;
            }

            return c;
        }
    }
}