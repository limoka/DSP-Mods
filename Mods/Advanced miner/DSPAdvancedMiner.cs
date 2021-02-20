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
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace DSPAdvancedMiner
{
    [BepInPlugin("org.kremnev8.plugin.dspadvancedminer", "DSP Advanced miner", "0.1.0.4")]
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
            
            Registry.registerModel(178, miner, "assets/custommachines/prefabs/mining-drill-mk2",
                new[] {mainMat, blackMat}, new[] {18, 19, 11, 12, 1}, 204, 2,
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


    [HarmonyPatch(typeof(PlayerAction_Build), "CheckBuildConditions")]
    static class PlayerAction_BuildPatch
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldc_R4),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(i =>
                        i.opcode == OpCodes.Callvirt && ((MethodInfo) i.operand).Name == "GetVeinsInAreaNonAlloc"))
                .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldloc_3))
                .InsertAndAdvance(
                    Transpilers.EmitDelegate<Func<BuildPreview, float>>(preview =>
                        DSPAdvancedMiner.getMinerRadius(preview.desc) + 4)
                ).MatchForward(true,
                    new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Vector3), nameof(Vector3.Dot))),
                    new CodeMatch(OpCodes.Stloc_S),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldc_R4))
                .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldloc_3))
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
        public static void Postfix(int id, ItemProto newProto, PlanetFactory __instance)
        {
            if (id == 0 || __instance.entityPool[id].id == 0) return;
            if (__instance.entityPool[id].minerId <= 0) return;
            MinerComponent component = __instance.factorySystem.minerPool[__instance.entityPool[id].minerId];

            if (component.type != EMinerType.Vein) return;

            PrefabDesc desc = newProto.prefabDesc;

            Pose pose;
            pose.position = __instance.entityPool[id].pos;
            pose.rotation = __instance.entityPool[id].rot;

            int[] tmp_ids = new int[256];
            Vector3 vector3 = pose.position + pose.forward * -1.2f;
            Vector3 rhs = -pose.forward;
            Vector3 up = pose.up;
            int veinsInAreaNonAlloc =
                __instance.planet.physics.nearColliderLogic.GetVeinsInAreaNonAlloc(vector3,
                    DSPAdvancedMiner.getMinerRadius(desc) + 4, tmp_ids);
            int[] refArray = new int[veinsInAreaNonAlloc];

            VeinData[] veinPool = __instance.planet.factory.veinPool;
            int refCount = 0;
            for (int j = 0; j < veinsInAreaNonAlloc; j++)
            {
                if (tmp_ids[j] != 0 && veinPool[tmp_ids[j]].id == tmp_ids[j])
                {
                    if (veinPool[tmp_ids[j]].type != EVeinType.Oil)
                    {
                        Vector3 pos = veinPool[tmp_ids[j]].pos;
                        Vector3 vector4 = pos - vector3;
                        float num8 = Vector3.Dot(up, vector4);
                        vector4 -= up * num8;
                        float sqrMagnitude = vector4.sqrMagnitude;
                        float num9 = Vector3.Dot(vector4.normalized, rhs);
                        float radius = DSPAdvancedMiner.getMinerRadius(desc);
                        if (sqrMagnitude <= radius * radius && num9 >= 0.73f && Mathf.Abs(num8) <= 2f)
                        {
                            refArray[refCount++] = tmp_ids[j];
                        }
                    }
                }
            }

            component.InitVeinArray(refCount);
            if (refCount > 0)
            {
                Array.Copy(refArray, component.veins, refCount);
            }

            for (int i = 0; i < component.veinCount; i++)
            {
                __instance.RefreshVeinMiningDisplay(component.veins[i], component.entityId, 0);
            }

            component.ArrageVeinArray();
            __instance.factorySystem.minerPool[__instance.entityPool[id].minerId] = component;
        }
    }
}