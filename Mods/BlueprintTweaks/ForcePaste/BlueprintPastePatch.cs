using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaAPI;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace BlueprintTweaks
{
    [HarmonyPatch]
    public static class BlueprintPastePatch
    {
        public static bool isEnabled;

        public static bool IsGood(this BuildPreview preview)
        {
            return preview.condition == EBuildCondition.Ok || preview.condition == EBuildCondition.NotEnoughItem;
        }

        public static bool IsCollide(this BuildPreview preview)
        {
            return preview.condition == EBuildCondition.Collide || preview.condition == EBuildCondition.PowerTooClose;
        }

        public static Dictionary<long, int> tmpPosBpidx = new Dictionary<long, int>();

        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), "CheckBuildConditions")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Normal)]
        public static void DontStopOnFail(BuildTool_BlueprintPaste __instance, ref bool __result)
        {
            tmpPosBpidx.Clear();
            for (int i = 0; i < __instance.bpCursor; i++)
            {
                BuildPreview preview = __instance.bpPool[i];
                if (!preview.desc.isPowerNode) continue;
                if (preview.condition != EBuildCondition.PowerTooClose && preview.condition != EBuildCondition.BlueprintBPOverlap) continue;

                Vector3 pos = (preview.lpos + preview.lpos2) * 0.5f;
                long key = ((long) Mathf.FloorToInt(pos.x * 100f) << 42) + ((long) Mathf.FloorToInt(pos.y * 100f) << 21) + Mathf.FloorToInt(pos.z * 100f);
                if (tmpPosBpidx.ContainsKey(key))
                {
                    BuildPreview preview2 = __instance.bpPool[tmpPosBpidx[key]];
                    if (!preview2.desc.isPowerNode) continue;
                    if (preview2.condition != EBuildCondition.PowerTooClose && preview2.condition != EBuildCondition.BlueprintBPOverlap) continue;

                    float num = Quaternion.Angle(preview.lrot, preview2.lrot);
                    float num2 = Quaternion.Angle(preview.lrot2, preview2.lrot2);

                    if (preview.desc == preview2.desc && num < 0.5f && num2 < 0.5f)
                    {
                        preview.coverbp = preview2;
                        preview.condition = EBuildCondition.Ok;
                        preview2.bpgpuiModelId = -1;
                        preview2.condition = EBuildCondition.BlueprintBPOverlap;
                        tmpPosBpidx[key] = i;
                    }
                }
                else
                {
                    tmpPosBpidx.Add(key, i);
                }
            }

            if (__result || !isEnabled) return;

            for (int i = 0; i < __instance.bpCursor; i++)
            {
                BuildPreview preview = __instance.bpPool[i];

                if (!preview.IsCollide() || !preview.desc.isBelt) continue;
                
                if (preview.output == null) continue;

                if (!preview.output.IsCollide()) continue;


                int overlapCount = Physics.OverlapSphereNonAlloc(preview.lpos, 0.28f, BuildTool._tmp_cols, 425984, QueryTriggerInteraction.Collide);

                PlanetPhysics physics = __instance.player.planetData.physics;

                for (int m = 0; m < overlapCount; m++)
                {
                    bool found = physics.GetColliderData(BuildTool._tmp_cols[m], out ColliderData collider);
                    int objectId = 0;
                    if (found && collider.isForBuild)
                    {
                        if (collider.objType == EObjectType.Entity)
                        {
                            objectId = collider.objId;
                        }
                    }

                    if (objectId == 0) continue;
                    ItemProto itemProto = __instance.GetItemProto(objectId);
                    if (!itemProto.prefabDesc.isBelt) continue;


                    __instance.factory.ReadObjectConn(objectId, 0, out bool _, out int otherObjId, out int _);

                    //0 next
                    //1 prev

                    __instance.factory.ReadObjectConn(objectId, 1, out bool _, out otherObjId, out int _);
                    
                    if (preview.output.IsCollide())
                    {
                        if (otherObjId == 0)
                        {
                            preview.coverObjId = objectId;
                            preview.willRemoveCover = false;
                            preview.output.bpgpuiModelId = 0;
                            preview.output = null;
                            preview.condition = EBuildCondition.Ok;
                            break;
                        }
                    }
                }
            }

            __result = true;
            __instance.actionBuild.model.cursorState = 0;
        }

        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), "Operating")]
        [HarmonyPostfix]
        public static void AllowToTryAgain(BuildTool_BlueprintPaste __instance)
        {
            if (!__instance.buildCondition && VFInput.blueprintPasteOperate0.onDown)
            {
                __instance.OperatingPrestage();
            }
        }

        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), "CreatePrebuilds")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> RemoveBrokenConnections(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(BuildPreview), nameof(BuildPreview.bpgpuiModelId))),
                    new CodeMatch(OpCodes.Ldc_I4_0)
                ).Advance(1);
            Label label = (Label) matcher.Instruction.operand;

            matcher.Advance(-2)
                .InsertAndAdvance(Transpilers.EmitDelegate<Func<BuildPreview, bool>>(bp =>
                {
                    if (!isEnabled && !NebulaModAPI.NebulaIsInstalled) return true;
                    if (bp.desc.multiLevel)
                    {
                        BuildPreview current = bp;
                        while (current.input != null)
                        {
                            if (!current.input.IsGood()) return false;

                            current = current.input;
                        }
                    }

                    if (bp.desc.isInserter)
                    {
                        if (bp.input != null && !bp.input.IsGood())
                        {
                            return bp.input.desc.isBelt;
                        }

                        if (bp.output != null && !bp.output.IsGood())
                        {
                            return bp.output.desc.isBelt;
                        }
                    }

                    return true;

                }))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse, label))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_3));

            return matcher.InstructionEnumeration();
        }
    }
}