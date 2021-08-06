using System.Security;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using kremnev8;
using UnityEngine;


[module: UnverifiableCode]
//[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace DSPAdvancedMiner
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class DSPAdvancedMiner : BaseUnityPlugin
    {
        public const string GUID = "org.kremnev8.plugin.dspadvancedminer";
        public const string NAME = "DSP Advanced miner";
        
        public const string VERSION = "1.0.7";

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
}