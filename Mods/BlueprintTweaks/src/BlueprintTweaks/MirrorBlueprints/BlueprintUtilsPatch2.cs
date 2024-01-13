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
        public delegate void RefAction(ref Vector4 area, float longAxis, float latAxis, float yaw);

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

        public static void MirrorArea(ref Vector4 area, float longAxis, float latAxis, float yaw)
        {
            int yawCount = Mathf.FloorToInt(yaw / 90f);

            if (mirrorLat)
            {
                if (yawCount == 1 || yawCount == 3)
                {
                    latAxis *= -1;
                }
                else
                {
                    longAxis *= -1;
                }
            }

            if (mirrorLong)
            {
                if (yawCount == 1 || yawCount == 3)
                {
                    longAxis *= -1;
                }
                else
                {
                    latAxis *= -1;
                }
            }

            area.x = longAxis < 0f ? area.z : area.x;
            area.y = latAxis < 0f ? area.w : area.y;
        }

        [HarmonyPatch(typeof(BlueprintUtils), "RefreshBuildPreview")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> RefreshPreviews(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            float MirrorBuildingRotation(float yaw, BlueprintBuilding building)
            {
                if (buildingsAxis.ContainsKey(building.modelIndex) && buildingsAxis[building.modelIndex] == MajorAxis.XAXIS)
                {
                    return MirrorRotation(yaw + 90f) - 90f;
                }

                return MirrorRotation(yaw);
            }

            // STEP 1

            // turns
            // Vector4 vector4 = array[l + blueprintBuilding.areaIndex];
            // vector4.x = ((num2 < 0f) ? vector4.z : vector4.x);
            // vector4.y = ((num3 < 0f) ? vector4.w : vector4.y);

            // into

            // Vector4 vector4 = array[l + blueprintBuilding.areaIndex];
            // MirrorArea(ref vector4, num2, num3, _yaw);

            CodeMatcher matcher = new CodeMatcher(instructions, generator)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldelem, typeof(Vector4)),
                    new CodeMatch(OpCodes.Stloc_S),
                    new CodeMatch(OpCodes.Ldloca_S));

            matcher.Advance(1);

            object longAxisVar = matcher.Operand;

            while (matcher.Opcode != OpCodes.Stfld)
            {
                matcher.RemoveInstruction();
            }

            matcher.RemoveInstructions(2);

            object latAxisVar = matcher.Operand;

            while (matcher.Opcode != OpCodes.Stfld)
            {
                matcher.RemoveInstruction();
            }

            matcher.RemoveInstruction()
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, longAxisVar))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, latAxisVar))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_S, 6)) //yaw
                .InsertAndAdvance(Transpilers.EmitDelegate<RefAction>(MirrorArea));

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

            // STEP 3

            // turns
            // Quaternion quaternion = Maths.SphericalRotation(dir, blueprintBuilding.yaw - (float)num * 90f);

            // into
            // Quaternion quaternion = Maths.SphericalRotation(dir, MirrorBuildingRotation(blueprintBuilding.yaw, blueprintBuilding) - (float)num * 90f);

            matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(BlueprintBuilding), nameof(BlueprintBuilding.yaw)))
                ).Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 30)) //TODO this is unsafe
                .InsertAndAdvance(Transpilers.EmitDelegate<Func<float, BlueprintBuilding, float>>(MirrorBuildingRotation)).Advance(2);

            // STEP 4

            // turns
            // Quaternion quaternion2 = Maths.SphericalRotation(dir2, blueprintBuilding.yaw2 - (float)num * 90f);

            // into
            // Quaternion quaternion2 = Maths.SphericalRotation(dir2, MirrorBuildingRotation(blueprintBuilding.yaw2, blueprintBuilding) - (float)num * 90f);

            matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(BlueprintBuilding), nameof(BlueprintBuilding.yaw2)))
                ).Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 30)) //TODO this is unsafe
                .InsertAndAdvance(Transpilers.EmitDelegate<Func<float, BlueprintBuilding, float>>(MirrorBuildingRotation));

            // STEP 5

            // inserts delegate approx before
            // if (buildPreview2.desc.isInserter)

            matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(BuildPreview), nameof(BuildPreview.desc))),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PrefabDesc), nameof(PrefabDesc.isInserter))))
                .Advance(-1);

            object previewVar2 = matcher.Operand;

            matcher.Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, previewVar2))
                .InsertAndAdvance(Transpilers.EmitDelegate<Action<BuildPreview>>(preview =>
                {
                    if (preview.desc.isInserter)
                    {
                        EntityInputsAndOutputs(preview, true);
                    }

                    if (preview.desc.isBelt)
                    {
                        EntityInputsAndOutputs(preview, false);
                    }
                }));


            return matcher.InstructionEnumeration();
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
    }
}