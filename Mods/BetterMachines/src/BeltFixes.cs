using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FasterMachines
{
    //Original author: xiaoye97, modified heavily.
    
    [HarmonyPatch]
    public static class BeltFixes
    {
        public delegate void RefAction<T1>(ref T1 arg1);
        public delegate void RefAction<in T1, T2>(T1 arg1, ref T2 arg2);
        public delegate void RefAction<in T1, T2, T3>(T1 arg1, ref T2 arg2, ref T3 arg3);
        
        
        private static readonly int color4 = Shader.PropertyToID("_Color4");
        private static readonly int emissionColor = Shader.PropertyToID("_EmissionColor");
        private static readonly int uvSpeed = Shader.PropertyToID("_UVSpeed");


        #region Belt Color
        public static void AddMatAndMesh()
        {
            BetterMachinesPlugin.logger.LogDebug("Adding belts materials and meshes.");
            Configs inst = Configs.instance;
            var builtin = inst.m_builtin;

            List<Material> mats = new List<Material>(builtin.beltMat);
            for (int i = 0; i < 4; i++)
            {
                var oriMat = Object.Instantiate(builtin.beltMat[8 + i]);
                oriMat.color = BetterMachinesPlugin.belt4Color;
                oriMat.SetColor(emissionColor, BetterMachinesPlugin.belt4Color);
                oriMat.SetFloat(uvSpeed, 12);
                mats.Add(Object.Instantiate(oriMat));
            }
            builtin.beltMat = mats.ToArray();
            List<Mesh> meshs = new List<Mesh>(builtin.beltMesh);

            for (int i = 0; i < 4; i++)
            {
                var oriMesh = Object.Instantiate(builtin.beltMesh[8 + i]);
                meshs.Add(Object.Instantiate(oriMesh));
            }
            builtin.beltMesh = meshs.ToArray();
        }
        
        
        [HarmonyPatch(typeof(CargoTraffic), "AlterBeltRenderer")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AddColors(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldloc_1),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(BeltComponent), nameof(BeltComponent.speed))),
                    new CodeMatch(OpCodes.Ldc_I4_1)
                ); 
            
            CodeMatcher matcher2 = matcher.Clone();
            matcher2.MatchForward(true,
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Stloc_S));

            object arg = matcher2.Operand;
            matcher2.Advance(1);
            object label = matcher2.Operand;

            matcher.Advance(2)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, arg))
                .SetInstruction(Transpilers.EmitDelegate<Func<int, int, int>>((speed, other) =>
                {
                    if (speed <= 1) return other;

                    if (speed <= 2) other += 4;
                    else if (speed <= 5) other += 8;
                    else if (speed <= 12) other += 12;

                    return other;
                }))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Stloc_S, arg))
                .SetInstruction(new CodeInstruction(OpCodes.Br, label));


                return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(CargoTraffic), "Draw")]
        [HarmonyPatch(typeof(CargoTraffic), "ClearStates")]
        [HarmonyPatch(typeof(CargoTraffic), "CreateRenderingBatches")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> ColorPatch(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(instr => instr.opcode == OpCodes.Ldc_I4_S && (sbyte) instr.operand == 12)
                ).Repeat(matcher2 =>
                {
                    matcher2.SetOperandAndAdvance(16);
                });
            
            return matcher.InstructionEnumeration();
        }
        
        [HarmonyPatch(typeof(ConnGizmoRenderer), "AddBlueprintBeltMajorPoint")]
        [HarmonyPatch(typeof(ConnGizmoRenderer), "AddBlueprintBeltPoint")]
        [HarmonyPatch(typeof(ConnGizmoRenderer), "AddBlueprintBeltConn")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> GizmoColor(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Initobj, typeof(ConnGizmoObj)));

            object arg = matcher.Operand;

                matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_0)
                ).Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, arg))
                .InsertAndAdvance(Transpilers.EmitDelegate<RefAction<ConnGizmoObj>>((ref ConnGizmoObj obj) =>
                {
                    if (obj.color == 12)
                    {
                        obj.color = 6;
                    }
                }));
            
            return matcher.InstructionEnumeration();
        }
        
        [HarmonyPatch(typeof(ConnGizmoRenderer), "Update")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> GizmoColorUpdate(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ConnGizmoRenderer), nameof(ConnGizmoRenderer.factory))),
                    new CodeMatch(OpCodes.Ldloc_3)
                ).Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 6))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, 0))
                .InsertAndAdvance(Transpilers.EmitDelegate<RefAction<int, ConnGizmoObj>>((int speed, ref ConnGizmoObj renderer) =>
                {
                    if (speed == 12 && renderer.color == 3)
                    {
                        renderer.color = 6;
                    }
                }));
            
            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(Configs), "Awake")]
        [HarmonyPostfix]
        public static void ChangeMats()
        {
            Configs.builtin.solidGizmoMats[0] = Object.Instantiate(BetterMachinesPlugin.gizmoMat);
            Configs.builtin.solidGizmoMats[1] = Object.Instantiate(BetterMachinesPlugin.gizmoMat);
            Configs.builtin.solidGizmoMats[2] = Object.Instantiate(BetterMachinesPlugin.gizmoLineMat);
            
            Configs.builtin.solidGizmoMats[5] = Object.Instantiate(BetterMachinesPlugin.gizmoMat);
            Configs.builtin.solidGizmoMats[6] = Object.Instantiate(BetterMachinesPlugin.gizmoLineMat);
            Configs.builtin.solidGizmoMats[7] = Object.Instantiate(BetterMachinesPlugin.gizmoMat);
        }
        
        [HarmonyPatch(typeof(CargoTraffic), "SetBeltSelected")]
        [HarmonyPrefix]
        public static bool SetBeltSelected(CargoTraffic __instance, int beltId)
        {
            int speed = __instance.beltPool[beltId].speed;
            if (speed == 12)
            {
                speed = 5;
            }
            __instance.SetBeltState(beltId, speed);
            return false;
        }    

        
        #endregion
        

        #region Speed bug fix

        [HarmonyPatch(typeof(CargoPath), "Update")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> CargoPathPatch(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(CargoPath), nameof(CargoPath.chunks))),
                    new CodeMatch(OpCodes.Ldloc_S)
                );

            matcher.MatchForward(false, new CodeMatch(OpCodes.Stloc_S));
            object beginArg = matcher.Operand;
            matcher.Advance(1).MatchForward(false, new CodeMatch(OpCodes.Stloc_S));
            object speedArg = matcher.Operand;

            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldc_I4_0),
                new CodeMatch(OpCodes.Stloc_S));

            matcher.Advance(2)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, beginArg))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, speedArg))
                .InsertAndAdvance(Transpilers.EmitDelegate<RefAction<CargoPath, int, int>>((CargoPath path, ref int begin, ref int speed) =>
                {
                    // The change
                    if (speed > 10) // If the speed is greater than 10, the length judgment process is performed to prevent crossing the boundary
                    {
                        for (int i = 10; i <= speed; i++)
                        {
                            if (begin + i + 10 >= path.bufferLength) // About to leave the end of the conveyor belt
                            {
                                speed = i;
                                break;
                            }

                            if (path.buffer[begin + i] != 0) // Not empty within the speed range
                            {
                                speed = i;
                                break;
                            }
                        }

                        if (speed < 10)
                        {
                            speed = 10; // If the speed slows down to within a safe speed, set it to a safe speed
                        }
                    }
                }));

            return matcher.InstructionEnumeration();
        }
        #endregion
    }
}