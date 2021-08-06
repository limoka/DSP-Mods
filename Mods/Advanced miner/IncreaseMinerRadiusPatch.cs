using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace DSPAdvancedMiner
{
    [HarmonyPatch]
    public static class BuildToolClickPatch
    {
        [HarmonyPatch(typeof(BuildTool_Click), "CheckBuildConditions")]
        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), "CheckBuildConditions")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(BuildPreview), nameof(BuildPreview.lpos))));

            object bpIndex = matcher.Instruction.operand;

            matcher.Start().MatchForward(false,
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldc_R4),
                    new CodeMatch(OpCodes.Ldsfld),
                    new CodeMatch(i =>
                        i.opcode == OpCodes.Callvirt && ((MethodInfo) i.operand).Name == "GetVeinsInAreaNonAlloc"))
                .Advance(1)
                .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldloc, bpIndex))
                .InsertAndAdvance(
                    Transpilers.EmitDelegate<Func<BuildPreview, float>>(preview =>
                        DSPAdvancedMiner.getMinerRadius(preview.desc) + 4))

                .MatchForward(true,
                    new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Vector3), nameof(Vector3.Dot))),
                    new CodeMatch(OpCodes.Stloc_S),
                    new CodeMatch(OpCodes.Ldc_R4))
                .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldloc, bpIndex))
                .InsertAndAdvance(
                    Transpilers.EmitDelegate<Func<BuildPreview, float>>(preview =>
                    {
                        float radius = DSPAdvancedMiner.getMinerRadius(preview.desc);
                        return radius * radius;
                    })
                )
                .MatchForward(true,
                    new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Mathf), nameof(Mathf.Abs), new[]{typeof(float)})),
                    new CodeMatch(OpCodes.Ldc_R4))
                .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Castclass, typeof(BuildTool)))
                .InsertAndAdvance(Transpilers.EmitDelegate<Func<BuildTool, float>>(tool => 400f / tool.planet.realRadius));

            return matcher.InstructionEnumeration();
        }
    }

    [HarmonyPatch]
    public static class BuildingGizmoPatch
    {
        [HarmonyPatch(typeof(BuildingGizmo), "SetGizmoDesc")]
        [HarmonyPostfix]
        public static void Postfix(BuildGizmoDesc _desc, Transform ___minerFan)
        {
            if (_desc.desc.minerType == EMinerType.Vein)
            {
                float size = 2 * DSPAdvancedMiner.getMinerRadius(_desc.desc);
                ___minerFan.localScale = new Vector3(size, size, size);
                ___minerFan.localEulerAngles = new Vector3(0f, 180f, 0f);
            }
        }
    }

    [HarmonyPatch]
    public static class PlanetFactoryPatch
    {
        [HarmonyPatch(typeof(PlanetFactory), "UpgradeEntityWithComponents")]
        [HarmonyPostfix]
        public static void Postfix(int entityId, ItemProto newProto, PlanetFactory __instance)
        {
            if (entityId == 0 || __instance.entityPool[entityId].id == 0) return;
            if (__instance.entityPool[entityId].minerId <= 0) return;
            MinerComponent component = __instance.factorySystem.minerPool[__instance.entityPool[entityId].minerId];

            if (component.type != EMinerType.Vein) return;

            PrefabDesc desc = newProto.prefabDesc;
            float radius = DSPAdvancedMiner.getMinerRadius(desc);
            radius *= radius;
            
            Pose pose;
            pose.position = __instance.entityPool[entityId].pos;
            pose.rotation = __instance.entityPool[entityId].rot;

            int[] tmp_ids = new int[256];
            Vector3 vector2 = pose.position + pose.forward * -1.2f;
            Vector3 rhs = -pose.forward;
            Vector3 vector3 = pose.up;
            int veinsInAreaNonAlloc = __instance.planet.physics.nearColliderLogic.GetVeinsInAreaNonAlloc(vector2, DSPAdvancedMiner.getMinerRadius(desc) + 4, tmp_ids);
            PrebuildData prebuildData = default(PrebuildData);
            prebuildData.InitParametersArray(veinsInAreaNonAlloc);
            VeinData[] veinPool = __instance.planet.factory.veinPool;
            int paramCount = 0;
            for (int j = 0; j < veinsInAreaNonAlloc; j++)
            {
                if (tmp_ids[j] != 0 && veinPool[tmp_ids[j]].id == tmp_ids[j])
                {
                    if (veinPool[tmp_ids[j]].type != EVeinType.Oil)
                    {
                        Vector3 vector4 = veinPool[tmp_ids[j]].pos - vector2;
                        float num2 = Vector3.Dot(vector3, vector4);
                        vector4 -= vector3 * num2;
                        float sqrMagnitude = vector4.sqrMagnitude;
                        float num3 = Vector3.Dot(vector4.normalized, rhs);
                        if (sqrMagnitude <= radius && num3 >= 0.73f && Mathf.Abs(num2) <= 2f)
                        {
                            prebuildData.parameters[paramCount++] = tmp_ids[j];
                        }
                    }
                }
                else
                {
                    Assert.CannotBeReached();
                }
            }
            
            component.InitVeinArray(paramCount);
            if (paramCount > 0)
            {
                Array.Copy(prebuildData.parameters, component.veins, paramCount);
            }
            
            for (int i = 0; i < component.veinCount; i++)
            {
                __instance.RefreshVeinMiningDisplay(component.veins[i], component.entityId, 0);
            }
            
            component.ArrangeVeinArray();
            __instance.factorySystem.minerPool[__instance.entityPool[entityId].minerId] = component;
        }
    }
}