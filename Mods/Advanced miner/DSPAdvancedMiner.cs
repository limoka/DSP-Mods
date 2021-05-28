
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using kremnev8;
using UnityEngine;
using UnityEngine.UI;


[module: UnverifiableCode]
#pragma warning disable 618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore 618

namespace DSPAdvancedMiner
{
    [BepInPlugin("org.kremnev8.plugin.dspadvancedminer", "DSP Advanced miner", "0.1.0.6")]
    public class DSPAdvancedMiner : BaseUnityPlugin
    {
        public static ManualLogSource logger;

        public static ConfigEntry<float> configMinerMk2Range;


        void Awake()
        {
            logger = Logger;

            Registry.Init("minerbundle", "custommachines", true, true);

            configMinerMk2Range = Config.Bind("General",
                "MinerMk2Range",
                10f,
                "How much range miner mk.2 has(Range of miner mk.1 is 7.75m). Note that this applies only to new miners built, already existing will not have their range changed!");

            Material mainMat = Registry.CreateMaterial("VF Shaders/Forward/PBR Standard", "mining-drill-mk2",
                "#00FFE8FF",
                new[]
                {
                    "assets/custommachines/texture2d/mining-drill-a",
                    "assets/custommachines/texture2d/mining-drill-n",
                    "assets/custommachines/texture2d/mining-drill-s",
                    "assets/custommachines/texture2d/mining-drill-e"
                });

            Material blackMat =
                Registry.CreateMaterial("VF Shaders/Forward/Black Mask", "mining-drill-black", "#FFFFFFFF");

            //Register and create buildings, items, models, etc
            Registry.registerString("advancedMiningDrill", "Mining drill Mk.II");
            Registry.registerString("advancedMiningDrillDesc",
                "Thanks to some hard to pronounce tech this drill has better range!");

            ItemProto miner = Registry.registerItem(2000, "advancedMiningDrill", "advancedMiningDrillDesc",
                "assets/custommachines/texture2d/mining-drill-mk2", 2504);
            
            ModelProto model = Registry.registerModel(178, "assets/custommachines/prefabs/mining-drill-mk2",
                new[] {mainMat, blackMat});
            
            Registry.AddModelToItemProto(model, miner, new[] {18, 19, 11, 12, 1}, 204, 2,
                new[] {2301, 0});

            Registry.registerRecipe(105, ERecipeType.Assemble, 60, new[] {2301, 1106, 1303, 1206}, new[] {1, 4, 2, 2},
               new[] {miner.ID}, new[] {1}, "advancedMiningDrillDesc", 1202);
            

            logger.LogInfo("Advanced Miner mod is initialized!");

            Registry.onLoadingFinished += onPostAdd;
        }

        //Post register fixups
        private static void onPostAdd()
        {
            foreach (var kv in Registry.models)
            {
                PrefabDesc pdesc = kv.Value.prefabDesc;

                if (pdesc.minerType == EMinerType.Vein)
                {
                    pdesc.beltSpeed = 1;
                }
            }
        }

        public static float getMinerRadius(PrefabDesc desc)
        {
            float radius = MinerComponent.kFanRadius;
            if (desc.beltSpeed == 1)
            {
                radius = configMinerMk2Range.Value;
            }

            return radius;
        }
    }

    //GetVeinsInAreaNonAlloc

/*
 * RaycastHit raycastHit;
					if (Physics.Raycast(this.mainCamera.ScreenPointToRay(Input.mousePosition), out raycastHit, 800f, 8720, QueryTriggerInteraction.Collide))
					{
 */

   /* [HarmonyPatch(typeof(PlayerController), "UpdateCommandState")]
    static class PlayerControllerPatch
    {
        public static bool objectAdded = false;
        public static Text textObj;
        [HarmonyPostfix]
        public static void Postfix(PlayerController __instance)
        {
            if (Physics.Raycast(__instance.mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit raycastHit,
                800f, 8720, QueryTriggerInteraction.Collide))
            {
                if (!objectAdded)
                {
                    GameObject obj = GameObject.Find("In Game");
                    if (obj != null)
                    {
                        var nobj = new GameObject("Special text");
                        nobj.transform.parent = obj.transform;
                        nobj.AddComponent<RectTransform>();
                        Text txt = nobj.AddComponent<Text>();
                        textObj = txt;
                        objectAdded = true;
                    }
                }

                int hash = PlanetPhysics.HashPhysBlock(raycastHit.point);
                textObj.text = $"Pos hash: {hash}";
            }
        }
    }

    [HarmonyPatch(typeof(NearColliderLogic), "GetVeinsInAreaNonAlloc")]
    static class NearColliderLogicPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(NearColliderLogic __instance, Vector3 center, float areaRadius, int[] veinIds,ref int __result)
        {
            if (veinIds == null)
            {
                __result = 0;
                return false;
            }
            int num = 0;
            Array.Clear(veinIds, 0, veinIds.Length);
            Vector3 up = center.normalized;
            Vector3 right = Vector3.Cross(up, Vector3.up).normalized;
            Vector3 forward;
            if (right.sqrMagnitude < 0.25f)
            {
                right = Vector3.right;
                forward = Vector3.forward;
            }
            else
            {
                forward = Vector3.Cross(right, up).normalized;
            }

            float dist = areaRadius + 3f;
            right *= dist;
            forward *= dist;
            
            __instance.activeColHashCount = 0;
            __instance.MarkActivePos(center);
            __instance.MarkActivePos(center + right);
            __instance.MarkActivePos(center - right);
            __instance.MarkActivePos(center + forward);
            __instance.MarkActivePos(center - forward);
            __instance.MarkActivePos(center + right + forward);
            __instance.MarkActivePos(center - right + forward);
            __instance.MarkActivePos(center + right - forward);
            __instance.MarkActivePos(center - right - forward);

            if (__instance.activeColHashCount > 0)
            {
                for (int i = 0; i < __instance.activeColHashCount; i++)
                {
                    int num2 = __instance.activeColHashes[i];
                    ColliderData[] colliderPool = __instance.colChunks[num2].colliderPool;
                    for (int j = 1; j < __instance.colChunks[num2].cursor; j++)
                    {
                        if (colliderPool[j].idType != 0)
                        {
                            if (colliderPool[j].usage != EColliderUsage.Build)
                            {
                                if (colliderPool[j].objType == EObjectType.Vein)
                                {
                                    if ((colliderPool[j].pos - center).sqrMagnitude <= areaRadius * areaRadius + colliderPool[j].ext.sqrMagnitude)
                                    {
                                        bool flag = false;
                                        for (int k = 0; k < num; k++)
                                        {
                                            if (veinIds[k] == colliderPool[j].objId)
                                            {
                                                flag = true;
                                                break;
                                            }
                                        }
                                        if (!flag)
                                        {
                                            veinIds[num++] = colliderPool[j].objId;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            __result = num;
            return false;
        }
    }*/

    [HarmonyPatch(typeof(BuildTool_Click), "CheckBuildConditions")]
    static class BuildTool_ClickPatch
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldc_R4),
                    new CodeMatch(OpCodes.Ldsfld),
                    new CodeMatch(i =>
                        i.opcode == OpCodes.Callvirt && ((MethodInfo) i.operand).Name == "GetVeinsInAreaNonAlloc"))
                .Advance(1)
                .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldloc, 6))
                .InsertAndAdvance(
                    Transpilers.EmitDelegate<Func<BuildPreview, float>>(preview =>
                        DSPAdvancedMiner.getMinerRadius(preview.desc) + 4)
                ).MatchForward(true,
                    new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Vector3), nameof(Vector3.Dot))),
                    new CodeMatch(OpCodes.Stloc_S),
                    new CodeMatch(OpCodes.Ldc_R4))
                .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldloc, 6))
                .InsertAndAdvance(
                    Transpilers.EmitDelegate<Func<BuildPreview, float>>(preview =>
                    {
                        float radius = DSPAdvancedMiner.getMinerRadius(preview.desc);
                        return radius * radius;
                    })
                );

            return matcher.InstructionEnumeration();
        }
    }

    [HarmonyPatch(typeof(BuildingGizmo), "SetGizmoDesc")]
    static class BuildingGizmoPatch
    {
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

    [HarmonyPatch(typeof(PlanetFactory), "UpgradeEntityWithComponents")]
    static class PlanetFactoryPatch
    {
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