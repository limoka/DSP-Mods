using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CommonAPI;
using CommonAPI.Systems;
using crecheng.DSPModSave;
using HarmonyLib;
using UnityEngine;


[module: UnverifiableCode]
//[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace AdvancedMiner
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency(CommonAPIPlugin.GUID)]
    [CommonAPISubmoduleDependency(nameof(ProtoRegistry), nameof(CustomDescSystem))]
    public class DSPAdvancedMiner : BaseUnityPlugin, IModCanSave
    {
        public const string MODID = "dspadvancedminer";
        public const string GUID = "org.kremnev8.plugin." + MODID;
        public const string NAME = "DSP Advanced miner";
        
        
        public const string VERSION = "1.1.1";

        public static ManualLogSource logger;
        public static ResourceData resources;

        public static ConfigEntry<bool> EnableMinerMk3;

        public static ConfigFile config;
        private static readonly int tintColor = Shader.PropertyToID("_TintColor");
        private static readonly int invFade = Shader.PropertyToID("_InvFade");
        private static readonly int albedoMultiplier = Shader.PropertyToID("_AlbedoMultiplier");
        private static readonly int emissionMultiplier = Shader.PropertyToID("_EmissionMultiplier");
        private static readonly int emissionSwitch = Shader.PropertyToID("_EmissionSwitch");
        private static readonly int emissionUsePower = Shader.PropertyToID("_EmissionUsePower");


        void Awake()
        {
            logger = Logger;
            config = Config;

            string pluginfolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            resources = new ResourceData(GUID, "BetterMachines", pluginfolder);
            resources.LoadAssetBundle("minerbundle");
            resources.ResolveVertaFolder();
            ProtoRegistry.AddResource(resources);

            EnableMinerMk3 = Config.Bind("General",
                "EnableMinerMk3",
                true,
                "Should Miner Mk3 item be added? Do note that loading a save that has Miners mk3 without this enabled will result in loss of these miners");

            Config.Bind("AdvancedMiners",
                "Tier-2_miningRange",
                10f,
                "How much range this tier has(Range of miner mk.1 is 7.75m). Note that this applies only to new miners built, already existing will not have their range changed!");

            Config.MigrateConfig<float>("General", "MinerMk2Range", "AdvancedMiners", "Tier-2_miningRange");

            Harmony harmony = new Harmony(GUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            
            string[] textures = {
                "Assets/BetterMachines/Textures/mining-drill-a",
                "Assets/BetterMachines/Textures/mining-drill-n",
                "Assets/BetterMachines/Textures/mining-drill-s",
                "Assets/BetterMachines/Textures/mining-drill-e"
            };

            Material mk2Mat = ProtoRegistry.CreateMaterial("VF Shaders/Forward/PBR Standard", "mining-drill-mk2",
                "#00FFE8FF", textures);
            
            mk2Mat.SetFloat(albedoMultiplier, 1.5f);
            mk2Mat.SetFloat(emissionMultiplier, 15f);
            mk2Mat.SetFloat(emissionSwitch, 1f);
            mk2Mat.SetFloat(emissionUsePower, 1f);
            mk2Mat.SetFloat(albedoMultiplier, 1.5f);
            
            
            Material mk3Mat = ProtoRegistry.CreateMaterial("VF Shaders/Forward/PBR Standard", "mining-drill-mk3",
                "#5FB9FFFF", textures);
            
            mk3Mat.SetFloat(albedoMultiplier, 1.5f);
            mk3Mat.SetFloat(emissionMultiplier, 15f);
            mk3Mat.SetFloat(emissionSwitch, 1f);
            mk3Mat.SetFloat(emissionUsePower, 1f);
            mk3Mat.SetFloat(albedoMultiplier, 1.5f);

            Material blackMat =
                ProtoRegistry.CreateMaterial("VF Shaders/Forward/Black Mask", "mining-drill-black", "#00000000");
                
            blackMat.renderQueue = 2980;
            blackMat.SetColor(tintColor, Color.black);
            blackMat.SetFloat(invFade, 1.5f);

            //Register and create buildings, items, models, etc
            ProtoRegistry.RegisterString("advancedMiningDrill", "Mining drill Mk.II");
            ProtoRegistry.RegisterString("advancedMiningDrillDesc",
                "Thanks to some hard to pronounce tech this drill has better range!");
            
            ProtoRegistry.RegisterString("miningDrillMk3", "Mining drill Mk.III");
            ProtoRegistry.RegisterString("miningDrillMk3Desc",
                "They did it again. Technology that makes this machine possible is so hard to pronounce that its name contains unknown symbols and math equations! Has better range and mining speed.");

            ItemProto miner = ProtoRegistry.RegisterItem(2000, "advancedMiningDrill", "advancedMiningDrillDesc",
                "Assets/BetterMachines/Icons/mining-drill-mk2", 2504);

            ItemProto minerMk3 = default;
            if (EnableMinerMk3.Value)
            {
                minerMk3 = ProtoRegistry.RegisterItem(2550, "miningDrillMk3", "miningDrillMk3Desc",
                    "Assets/BetterMachines/Icons/mining-drill-mk3", 2505);
            }

            ModelProto model = ProtoRegistry.RegisterModel(178, "Assets/BetterMachines/Machines/mining-drill-mk2",
                new[] {mk2Mat, blackMat});

            int[] upgrades = EnableMinerMk3.Value ? new[] {2301, 0, minerMk3.ID} : new[] {2301, 0};
            
            ProtoRegistry.AddModelToItemProto(model, miner, new[] {18, 19, 11, 12, 1}, 204, 2,
                upgrades);

            ProtoRegistry.RegisterRecipe(105, ERecipeType.Assemble, 60, new[] {2301, 1106, 1303, 1206}, new[] {1, 4, 2, 2},
               new[] {miner.ID}, new[] {1}, "advancedMiningDrillDesc", 1202);

            if (EnableMinerMk3.Value)
            {
                ModelProto modelMk3 = ProtoRegistry.RegisterModel(270, "Assets/BetterMachines/Machines/mining-drill-mk3",
                    new[] {mk3Mat, blackMat});
            
                ProtoRegistry.AddModelToItemProto(modelMk3, minerMk3, new[] {18, 19, 11, 12, 1}, 205, 3,
                    new[] {2301, miner.ID, 0});

                ProtoRegistry.RegisterRecipe(260, ERecipeType.Assemble, 60, new[] {miner.ID, 1107, 1305, 1402}, new[] {1, 4, 2, 8},
                    new[] {minerMk3.ID}, new[] {1}, "miningDrillMk3Desc", 1203);
            }
            
            

            logger.LogInfo("Advanced Miner mod is initialized!");

            //ProtoRegistry.onLoadingFinished += onPostAdd;
        }
        
        public static float getMinerRadius(PrefabDesc desc)
        {
            if (desc.HasProperty(CustomMinerDesc.MINER_RANGE_NAME))
            {
                return desc.GetProperty<float>(CustomMinerDesc.MINER_RANGE_NAME);
            }

            return MinerComponent.kFanRadius;
        }

        // Save and load miner Component's insertTarget2 field
        public void Export(BinaryWriter w)
        {
            w.Write((byte) 0);
            
            for (int i = 0; i < GameMain.data.factoryCount; i++)
            {
                FactorySystem factory = GameMain.data.factories[i].factorySystem;
                for (int j = 1; j < factory.minerCursor; j++)
                {
                    w.Write(factory.minerPool[j].insertTarget2);
                }
            }
        }

        public void Import(BinaryReader r)
        {
            r.ReadByte();
            
            for (int i = 0; i < GameMain.data.factoryCount; i++)
            {
                FactorySystem factory = GameMain.data.factories[i].factorySystem;
                for (int j = 1; j < factory.minerCursor; j++)
                {
                    factory.minerPool[j].insertTarget2 = r.ReadInt32();
                }
            }
        }

        public void IntoOtherSave()
        {
        }
    }
}