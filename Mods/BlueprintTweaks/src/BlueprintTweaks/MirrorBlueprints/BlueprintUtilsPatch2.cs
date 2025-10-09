using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace BlueprintTweaks
{
    public enum MajorAxis
    {
        XAXIS,
        ZAXIS
    }

    [HarmonyPatch]
    public static class BlueprintUtilsPatch2
    {
        public delegate float RefAction(ref Vector4 area, ref float latValueOut, bool longAxis, bool latAxis, float yaw);

        public static bool mirrorLat;
        public static bool mirrorLong;

        public static Dictionary<int, MajorAxis> buildingsAxis = new Dictionary<int, MajorAxis>();
        public static Dictionary<int, Vector2> buildingsOffsets = new Dictionary<int, Vector2>();

        public static void Init()
        {
            //Storage mk 2 and storage tank
            buildingsAxis.Add(52, MajorAxis.XAXIS);
            buildingsAxis.Add(121, MajorAxis.XAXIS);

            //Chemical plant
            buildingsAxis.Add(64, MajorAxis.XAXIS);
            buildingsOffsets.Add(64, new Vector2(0, -1));

            //Chemical plant mk2
            buildingsAxis.Add(376, MajorAxis.XAXIS);
            buildingsOffsets.Add(376, new Vector2(0, -1));

            //Particle accelerator
            buildingsAxis.Add(69, MajorAxis.XAXIS);

            // Thermal and fission generators
            buildingsOffsets.Add(54, new Vector2(-1, 0));
            buildingsOffsets.Add(118, new Vector2(-1, 0));
        }

        public static void UpdateBlueprintDisplay()
        {
            if (GameMain.mainPlayer?.controller == null) return;
            if (GameMain.mainPlayer.controller.actionBuild.blueprintMode != EBlueprintMode.Paste) return;

            GameMain.mainPlayer.controller.actionBuild.blueprintPasteTool.ForceDetermineBP();
        }

        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), "_OnClose")]
        [HarmonyPrefix]
        public static void Close()
        {
            if (BlueprintTweaksPlugin.resetFunctionsOnMenuExit.Value)
            {
                mirrorLat = false;
                mirrorLong = false;
            }
        }

        [HarmonyPatch(typeof(BlueprintUtils), "TransitionWidthAndHeight")]
        [HarmonyPrefix]
        public static void MirrorObjects(float _yaw, ref float _width, ref float _height)
        {
            if (mirrorLat)
            {
                _width *= -1;
            }

            if (mirrorLong)
            {
                _height *= -1;
            }
        }

        public static float MirrorRotation(float yaw)
        {
            if (mirrorLat && mirrorLong) return yaw + 180;
            if (mirrorLat) return -yaw;
            if (mirrorLong) return -yaw + 180;

            return yaw;
        }

        public static float MirrorArea(ref Vector4 area, ref float latValueOut, bool longAxis, bool latAxis, float yaw)
        {
            int yawCount = Mathf.FloorToInt(yaw / 90f);

            if (mirrorLat)
            {
                if (yawCount == 1 || yawCount == 3)
                {
                    latAxis = !latAxis;
                }
                else
                {
                    longAxis = !longAxis;
                }
            }

            if (mirrorLong)
            {
                if (yawCount == 1 || yawCount == 3)
                {
                    longAxis = !longAxis;
                }
                else
                {
                    latAxis = !latAxis;
                }
            }

            float longValue = longAxis ? area.z : area.x;
            latValueOut = latAxis ? area.w : area.y;
            
            return longValue;
        }

        [HarmonyPatch(typeof(BlueprintUtils), "RefreshBuildPreview")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> RefreshPreviews(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher matcher = new CodeMatcher(instructions, generator);
            
            RefreshPreviewsPatchStep1(matcher);
            RefreshPreviewsPatchStep2(matcher);
            RefreshPreviewsPatchStep3(matcher);
            RefreshPreviewsPatchStep4(matcher);
            RefreshPreviewsPatchStep3b(matcher);
            RefreshPreviewsPatchStep4b(matcher);
            RefreshPreviewsPatchStep5(matcher);

            return matcher.InstructionEnumeration();
        }

        private static void RefreshPreviewsPatchStep1(CodeMatcher matcher)
        {
            // STEP 1

            // turns
            // ref Vector4 ptr4 = ref array[l + blueprintBuilding.areaIndex];
            // object obj = (flag ? ptr4.z : ptr4.x); (On stack)
            // float num44 = (flag2 ? ptr4.w : ptr4.y);

            // into

            // Vector4 vector4 = array[l + blueprintBuilding.areaIndex];
            // MirrorArea(ref vector4, num2, num3, _yaw);

            //Anchor to prevent false positive
            matcher.MatchForward(
                false,
                new CodeMatch(
                    OpCodes.Ldfld,
                    AccessTools.Field(typeof(BuildPreview), nameof(BuildPreview.bpgpuiModelId))
                )
            );
            
            //Actual target
            matcher.MatchForward(true,
                    new CodeMatch(OpCodes.Ldelema, typeof(Vector4)),
                    new CodeMatch(OpCodes.Stloc_S),
                    new CodeMatch(OpCodes.Ldloc_S));

            matcher.Advance(-1);
            object vectorPtrVar = matcher.Operand;
            matcher.Advance(1);
            
            object longAxisVar = matcher.Operand;

            // Remove two ldfld's (First value stays on the stack)
            for (int i = 0; i < 2; i++)
            {
                while (matcher.Opcode != OpCodes.Ldfld)
                    matcher.RemoveInstruction();
                matcher.RemoveInstruction();
            }

            object latAxisVar = matcher.Operand;

            while (matcher.Opcode != OpCodes.Stloc_S)
                matcher.RemoveInstruction();
            
            object latValueVar = matcher.Operand;

            matcher.RemoveInstruction()
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, vectorPtrVar))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, latValueVar))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, longAxisVar))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, latAxisVar))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_S, 8)) //yaw
                .InsertAndAdvance(Transpilers.EmitDelegate<RefAction>(MirrorArea));
        }
        
        private static void RefreshPreviewsPatchStep2(CodeMatcher matcher)
        {
            // STEP 2

            // turns
            // Vector2 vector5 = BlueprintUtils.TransitionWidthAndHeight(_yaw, blueprintBuilding.localOffset_x, blueprintBuilding.localOffset_y);

            // into
            // Vector2 vector5 = @delegate(_yaw, blueprintBuilding);

            matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(BlueprintUtils), nameof(BlueprintUtils.TransitionWidthAndHeight))))
                .Advance(-3)
                .RemoveInstruction().RemoveInstruction().RemoveInstruction()
                .SetInstruction(Transpilers.EmitDelegate<Func<float, BlueprintBuilding, Vector2>>((yaw, building) =>
                {
                    float x = building.localOffset_x;
                    float y = building.localOffset_y;


                    if ((mirrorLat && !mirrorLong || !mirrorLat && mirrorLong) && buildingsOffsets.ContainsKey(building.modelIndex))
                    {
                        Vector2 offset = buildingsOffsets[building.modelIndex];
                        float rotatedYaw = MirrorBuildingRotation(building.yaw, building);

                        offset = offset.Rotate(rotatedYaw + (mirrorLong ? 180 : 0));

                        x += offset.x;
                        y += offset.y;
                    }

                    return BlueprintUtils.TransitionWidthAndHeight(yaw, x, y);
                }));
        }
        
        private static void RefreshPreviewsPatchStep3(CodeMatcher matcher)
        {
            // STEP 3

            // turns
            // Quaternion quaternion = Maths.SphericalRotation(dir, blueprintBuilding.yaw - (float)num * 90f);

            // into
            // Quaternion quaternion = Maths.SphericalRotation(dir, MirrorBuildingRotation(blueprintBuilding.yaw, blueprintBuilding) - (float)num * 90f);

            matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(BlueprintBuilding), nameof(BlueprintBuilding.yaw)))
                ).Advance(-1);
                
            var buildingVar = matcher.Operand;    
            
            matcher.Advance(2)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, buildingVar))
                .InsertAndAdvance(Transpilers.EmitDelegate<Func<float, BlueprintBuilding, float>>(MirrorBuildingRotation)).Advance(2);
        }
        
        private static void RefreshPreviewsPatchStep4(CodeMatcher matcher)
        {
            // STEP 4

            // turns
            // Quaternion quaternion2 = Maths.SphericalRotation(dir2, blueprintBuilding.yaw2 - (float)num * 90f);

            // into
            // Quaternion quaternion2 = Maths.SphericalRotation(dir2, MirrorBuildingRotation(blueprintBuilding.yaw2, blueprintBuilding) - (float)num * 90f);

            matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(BlueprintBuilding), nameof(BlueprintBuilding.yaw2)))
                ).Advance(-1);
                
            var buildingVar = matcher.Operand;    
            
            matcher.Advance(2)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, buildingVar))
                .InsertAndAdvance(Transpilers.EmitDelegate<Func<float, BlueprintBuilding, float>>(MirrorBuildingRotation));
        }
        
        private static void RefreshPreviewsPatchStep3b(CodeMatcher matcher)
        {
            // STEP 3b - Inserter

            // turns
            // lrot = Maths.SphericalRotation(dir, 0f) * Quaternion.Euler(blueprintBuilding.pitch, blueprintBuilding.yaw - (float)num * 90f, blueprintBuilding.tilt);

            // into
            // lrot = Maths.SphericalRotation(dir, 0f) * Quaternion.Euler(blueprintBuilding.pitch, MirrorBuildingRotation(blueprintBuilding.yaw, blueprintBuilding) - (float)num * 90f, blueprintBuilding.tilt);

            matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(BlueprintBuilding), nameof(BlueprintBuilding.yaw)))
                ).Advance(-1);
                
            var buildingVar = matcher.Operand;    
            
            matcher.Advance(2)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, buildingVar))
                .InsertAndAdvance(Transpilers.EmitDelegate<Func<float, BlueprintBuilding, float>>(MirrorBuildingRotation)).Advance(2);
        }
        
        private static void RefreshPreviewsPatchStep4b(CodeMatcher matcher)
        {
            // STEP 4b - Inserter

            // turns 							
            // lrot2 = Maths.SphericalRotation(dir2, 0f) * Quaternion.Euler(blueprintBuilding.pitch2, blueprintBuilding.yaw2 - (float)num * 90f, blueprintBuilding.tilt2);

            // into
            // lrot2 = Maths.SphericalRotation(dir2, 0f) * Quaternion.Euler(blueprintBuilding.pitch2, MirrorBuildingRotation(blueprintBuilding.yaw2, blueprintBuilding) - (float)num * 90f, blueprintBuilding.tilt2);

            matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(BlueprintBuilding), nameof(BlueprintBuilding.yaw2)))
                ).Advance(-1);
                
            var buildingVar = matcher.Operand;    
            
            matcher.Advance(2)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, buildingVar))
                .InsertAndAdvance(Transpilers.EmitDelegate<Func<float, BlueprintBuilding, float>>(MirrorBuildingRotation));
        }

        private static void RefreshPreviewsPatchStep5(CodeMatcher matcher)
        {
            // STEP 5

            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(BlueprintBuilding), nameof(BlueprintBuilding.index))));

            object buildingVar = matcher.Operand;

            // inserts delegate approx before
            // if (buildPreview2.desc.isInserter)

            matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(BuildPreview), nameof(BuildPreview.desc))),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PrefabDesc), nameof(PrefabDesc.isInserter))))
                .Advance(-1);

            object previewVar2 = matcher.Operand;

            matcher.Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, previewVar2))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, buildingVar))
                .InsertAndAdvance(Transpilers.EmitDelegate<Action<BuildPreview, BlueprintBuilding>>((preview, building) =>
                {
                    if (preview.desc.isInserter)
                    {
                        EntityInputsAndOutputs(preview, true);
                    }

                    if (preview.desc.isBelt)
                    {
                        EntityInputsAndOutputs(preview, false);
                    }

                    if (preview.desc.isStation)
                    {
                        MirrorStationData(preview, building);
                    }
                }));
        }

        public static float MirrorBuildingRotation(float yaw, BlueprintBuilding building)
        {
            if (buildingsAxis.ContainsKey(building.modelIndex) && buildingsAxis[building.modelIndex] == MajorAxis.XAXIS)
            {
                return MirrorRotation(yaw + 90f) - 90f;
            }

            return MirrorRotation(yaw);
        }

        private static Pose[] GetSlotsOrPorts(this PrefabDesc desc, bool useSlots)
        {
            return useSlots ? desc.slotPoses : desc.portPoses;
        }

        private static void EntityInputsAndOutputs(BuildPreview preview, bool useSlots)
        {
            if (preview.input != null &&
                !preview.input.desc.isBelt &&
                !preview.input.desc.isInserter &&
                preview.inputFromSlot < preview.input.desc.GetSlotsOrPorts(useSlots).Length)
            {
                Quaternion invRot = Quaternion.Inverse(preview.input.lrot);
                Vector3 portPosition = preview.lpos - preview.input.lpos;
                portPosition = invRot * portPosition;
                Quaternion portRotation = invRot * preview.lrot;

                Pose[] poses = preview.input.desc.GetSlotsOrPorts(useSlots);

                for (int i = 0; i < poses.Length; i++)
                {
                    Pose pose = poses[i];
                    if (!((pose.position - portPosition).sqrMagnitude < 0.1f)) continue;
                    if (!pose.rotation.Approximately(portRotation)) continue;
                    if (preview.inputFromSlot == i) break;

                    preview.inputFromSlot = i;
                    break;
                }
            }

            if (preview.output != null &&
                !preview.output.desc.isBelt &&
                !preview.output.desc.isInserter &&
                preview.outputToSlot < preview.output.desc.GetSlotsOrPorts(useSlots).Length)
            {
                Quaternion invRot = Quaternion.Inverse(preview.output.lrot);
                Vector3 portPosition = preview.lpos2 - preview.output.lpos;
                portPosition = invRot * portPosition;
                Quaternion portRotation = invRot * (preview.lrot2 * Quaternion.Euler(0f, -180f, 0f));

                Pose[] poses = preview.output.desc.GetSlotsOrPorts(useSlots);

                for (int i = 0; i < poses.Length; i++)
                {
                    Pose pose = poses[i];
                    if (!((pose.position - portPosition).sqrMagnitude < 0.1f)) continue;
                    if (!pose.rotation.Approximately(portRotation)) continue;
                    if (preview.outputToSlot == i) break;

                    preview.outputToSlot = i;
                    break;
                }
            }
        }

        public static Vector2 RotateXZ(Vector3 inVec, float angleRad)
        {
            float x = inVec.x * Mathf.Cos(angleRad) - inVec.z * Mathf.Sin(angleRad);
            float y = inVec.x * Mathf.Sin(angleRad) + inVec.z * Mathf.Cos(angleRad);
            return new Vector2(x, y);
        }
        
        private static void MirrorStationData(BuildPreview preview, BlueprintBuilding building)
        {
            if (preview.desc.isCollectStation) return;
            if (building.parameters == null ||
                building.parameters.Length == 0) return;

            const int startIndex = 192;
            
            if (!mirrorLat && !mirrorLong)
            {
                for (int i = 0; i < preview.desc.portPoses.Length; i++)
                {
                    int currentIndex = startIndex + i * 4;
                    preview.parameters[currentIndex] = building.parameters[currentIndex];
                    preview.parameters[currentIndex + 1] = building.parameters[currentIndex + 1];
                }
                return;
            }

            float finalYaw = MirrorBuildingRotation(building.yaw, building);
            float buildingYawRad = building.yaw * Mathf.Deg2Rad;

            bool xAxisMirror = mirrorLat;
            bool yAxisMirror = mirrorLong;

            float angleDiff = Mathf.Abs(Mathf.DeltaAngle(building.yaw, finalYaw));
            // Hack, because I have no idea why using mirrored angle doesn't work.
            if (angleDiff == 180)
            {
                xAxisMirror = !mirrorLat;
                yAxisMirror = !mirrorLong;
            }

            for (int i = 0; i < preview.desc.portPoses.Length; i++)
            {
                Vector3 originalPos = preview.desc.portPoses[i].position;
                Vector2 transitionedPos = RotateXZ(originalPos, buildingYawRad);
                int currentIndex = startIndex + i * 4;

                Vector2 mirroredPos = new Vector2(
                    xAxisMirror ? -transitionedPos.x : transitionedPos.x,
                    yAxisMirror ? -transitionedPos.y : transitionedPos.y);

                for (int j = 0; j < preview.desc.portPoses.Length; j++)
                {
                    Vector3 testPosition = preview.desc.portPoses[j].position;
                    Vector2 transitionedTestPos = RotateXZ(testPosition, buildingYawRad);

                    if (!((mirroredPos - transitionedTestPos).sqrMagnitude < 0.1f)) continue;

                    int newIndex = startIndex + j * 4;
                    preview.parameters[currentIndex] = building.parameters[newIndex];
                    preview.parameters[currentIndex + 1] = building.parameters[newIndex + 1];
                }
            }
        }
    }
}