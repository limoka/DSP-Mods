using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CommonAPI;
using CommonAPI.Systems;
using HarmonyLib;
using RebindBuildBar;
using UnityEngine;
using xiaoye97;

[module: UnverifiableCode]
#pragma warning disable 618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore 618

namespace FasterMachines
{
    [BepInPlugin(MODGUID, MOD_DISP_NAME, VERSION)]
    [BepInDependency(LDBToolPlugin.MODGUID)]
    [BepInDependency(CommonAPIPlugin.GUID)]
    [CommonAPISubmoduleDependency(nameof(ProtoRegistry), nameof(CustomDescSystem), nameof(UtilSystem))]
    public class BetterMachinesPlugin : BaseUnityPlugin
    {
        public const string MODNAME = "BetterMachines";
        
        public const string MODGUID = "org.kremnev8.plugin.BetterMachines";
        
        public const string MOD_DISP_NAME = "Better Machines";
        
        public const string VERSION = "1.1.4";

        public ItemProto smelterMk2item;

        public static List<ItemProto> bmItems = new List<ItemProto>();
         
        public ModelProto smelterMk3;

        public ModelProto liquidTankMk2;
        public ModelProto liquidTankMk3;

        public ItemProto beltMk4Item;
        public ModelProto beltMk4;

        public static ResourceData resources;
        public static ManualLogSource logger;
        public static ConfigFile config;
        
        public static Color belt4Color = new Color(0.5058824F, 0.4039216F, 0.9647059F);

        public static Material gizmoMat;
        public static Material gizmoLineMat;
        
        private static readonly int emissionSwitch = Shader.PropertyToID("_EmissionSwitch");
        private static readonly int emissionUsePower = Shader.PropertyToID("_EmissionUsePower");
        private static readonly int albedoMultiplier = Shader.PropertyToID("_AlbedoMultiplier");
        private static readonly int emissionMultiplier = Shader.PropertyToID("_EmissionMultiplier");
        private static readonly int emissionJitter = Shader.PropertyToID("_EmissionJitter");
        private static readonly int alphaClip = Shader.PropertyToID("_AlphaClip");
        private static readonly int tintColor = Shader.PropertyToID("_TintColor");
        private static readonly int invFade = Shader.PropertyToID("_InvFade");
        private static readonly int bumpScale = Shader.PropertyToID("_BumpScale");
        private static readonly int smoothMultiplier = Shader.PropertyToID("_SmoothMultiplier");
        private static readonly int specularColor = Shader.PropertyToID("_SpecularColor");
        private static readonly int rimColor = Shader.PropertyToID("_RimColor");
        private static readonly int emissionColor = Shader.PropertyToID("_EmissionColor");

        public static ConfigEntry<int> oldChemPlantID;


        private void Awake()
        {
            logger = Logger;
            config = Config;

            oldChemPlantID = config.Bind("General", "OldChemicalPlantID", 3702, "If you have rebound Chemical Plant Mk.III item ID, you will need to specify it here to allow for correct migration.");
            
            string pluginfolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            using (ProtoRegistry.StartModLoad(MODGUID))
            {
                resources = new ResourceData(MODGUID, "BetterMachines", pluginfolder);
                resources.LoadAssetBundle("bettermachines");
                resources.ResolveVertaFolder();
                ProtoRegistry.AddResource(resources);

                #region Strings

                ProtoRegistry.EditString("电弧熔炉", "Arc Smelter (MK.I)", "电弧熔炉 (MK.I)");
                ProtoRegistry.RegisterString("smelterMk2", "Arc Smelter (MK.II)", "电弧熔炉 (MK.II)");
                ProtoRegistry.EditString("位面熔炉", "Plane Smelter (MK.III)", "位面熔炉 (MK.III)");

                ProtoRegistry.EditString("化工厂", "Chemical Plant MK.I", "化工厂 MK.I");
                ProtoRegistry.RegisterString("chemicalPlantMk2", "Chemical Plant MK.II", "化工厂 MK.II");
                ProtoRegistry.EditString("化工厂 Mk.II", "Quantum Chemical Plant (MK.III)", "量子化工厂 (MK.III)");

                ProtoRegistry.RegisterString("storageMk3", "Storage MK.III", "型储物仓 MK.III");
                ProtoRegistry.RegisterString("storageMk4", "Storage MK.IV", "型储物仓 MK.IV");

                ProtoRegistry.EditString("储液罐", "Liquid Tank MK.I", "储液罐 MK.I", "Réservoir de stockage de liquide MK.I");
                ProtoRegistry.RegisterString("liquidTankMk2", "Liquid Tank MK.II", "储液罐 MK.II", "Réservoir de stockage de liquide MK.II");
                ProtoRegistry.RegisterString("liquidTankMk3", "Liquid Tank MK.III", "储液罐 MK.III", "Réservoir de stockage de liquide MK.III");

                ProtoRegistry.RegisterString("smelterMk2Desc",
                    "High temperature arc smelting can smelt ores and metals, and also be used for purification and crystal smelting. By increasing maximum furnace temperature arc smelter can now smelt faster!");
                ProtoRegistry.EditString("位面冶金结果",
                    "You have unlocked the more advanced smelter ━━ <color=\"#FD965ECC\">Plane Smelter</color>, which is faster as an Arc Smelter (MK.II)!",
                    "你解锁了更高级的熔炉━━<color=\"#FD965ECC\">位面熔炉</color>， 它的速度比电弧炉（MK.II）还要高！",
                    "You have unlocked the more advanced smelter ━━ <color=\"#FD965ECC\">Plane Smelter</color>, which is faster as an Arc Smelter (MK.II)!");

                ProtoRegistry.RegisterString("chemicalPlantMk2Desc",
                    "Chemical facility. Can process the primary products of Crude oil into more advanced chemical products. Production speed has been increased.");
                //ProtoRegistry.RegisterString("chemicalPlantMk3Desc",
                //    "Chemical facility. Can process the primary products of Crude oil into more advanced chemical products. Production speed has been greatly increased.");

                ProtoRegistry.EditString("I小型储物仓", "Storage MK.I is designed to store Solids.");
                ProtoRegistry.EditString("I大型储物仓", "Storage MK.II is designed to store Solids. Thanks to new materials it has more capacity than Storage Mk.I");
                ProtoRegistry.RegisterString("storageMk3Desc",
                    "Storage MK.III is designed to store Solids. Thanks to new materials it has more capacity than Storage Mk.II");
                ProtoRegistry.RegisterString("storageMk4Desc",
                    "Storage MK.IV is designed to store Solids. Thanks to new materials it has more capacity than Storage Mk.III");

                ProtoRegistry.EditString("I储液罐",
                    "Liquid Tank MK.I is designed to store Liquids. When dismantling a non-empty Storage Tank any remaining fluid will be lost.");
                ProtoRegistry.RegisterString("liquidTankMk2Desc",
                    "Liquid Tank MK.II is designed to store Liquids. Thanks to new materials it has more capacity than Liquid Tank Mk.I. When dismantling a non-empty Storage Tank any remaining fluid will be lost.");
                ProtoRegistry.RegisterString("liquidTankMk3Desc",
                    "Liquid Tank MK.III is designed to store Liquids. Thanks to new materials it has more capacity than Liquid Tank Mk.II. When dismantling a non-empty Storage Tank any remaining fluid will be lost.");

                ProtoRegistry.RegisterString("highDensityStorageTech", "High density storage");
                ProtoRegistry.RegisterString("highDensityStorageTechDesc",
                    "By using new stronger materials maximum capacity of Storages and Liquid Tanks can be increased.");
                ProtoRegistry.RegisterString("highDensityStorageTechConc",
                    "You have obtained new Storage MK.IV and Liquid Tank Mk.III, which have increased storage capacity");

                ProtoRegistry.RegisterString("beltMk4", "Belt MK.IV");
                ProtoRegistry.RegisterString("beltMk4Desc",
                    "Allows to transport items around. I have heard that it's so fast, that the game doesn't understand what to do with it.");


                ProtoRegistry.RegisterString("BMModModificationWarn", "  - [Better Machines] Replaced {0} buildings", "  - [Better Machines] 取代 {0} 建筑物");

                #endregion

                #region Materials

                Material smelterMk2Part1 = ProtoRegistry.CreateMaterial("VF Shaders/Forward/PBR Standard", "smelter-mk2-part1", "#DB5860FF",
                    new[]
                    {
                        "Assets/BetterMachines/Textures/smelter-mk1-lod0-a",
                        "Assets/BetterMachines/Textures/smelter-mk1-lod0-n",
                        "Assets/BetterMachines/Textures/smelter-mk1-lod0-s",
                        "Assets/BetterMachines/Textures/smelter-mk1-lod0-e",
                        "Assets/BetterMachines/Textures/smelter-mk1-lod0-j"
                    });
                

                smelterMk2Part1.SetFloat(emissionSwitch, 1);
                smelterMk2Part1.SetFloat(emissionUsePower, 1);

                smelterMk2Part1.SetFloat(albedoMultiplier, 1.3f);
                smelterMk2Part1.SetFloat(emissionMultiplier, 10);
                smelterMk2Part1.SetFloat(emissionJitter, 0.5f);

                Material smelterMk2Part2 = ProtoRegistry.CreateMaterial("VF Shaders/Forward/PBR Standard", "smelter-mk2-part2", "#DB5860FF",
                    new[]
                    {
                        "Assets/BetterMachines/Textures/smelter-mk3-lod0-a",
                        "Assets/BetterMachines/Textures/smelter-mk3-lod0-n",
                        "Assets/BetterMachines/Textures/smelter-mk3-lod0-s",
                        "Assets/BetterMachines/Textures/smelter-mk3-lod0-e"
                    });

                smelterMk2Part2.SetFloat(emissionSwitch, 1);
                smelterMk2Part2.SetFloat(emissionUsePower, 1);

                smelterMk2Part2.SetFloat(albedoMultiplier, 1.3f);
                smelterMk2Part2.SetFloat(emissionMultiplier, 10);
                smelterMk2Part2.SetFloat(emissionJitter, 0.5f);

                Material smelterMk2Lods = ProtoRegistry.CreateMaterial("VF Shaders/Forward/PBR Standard", "smelter-mk2-lods", "#DB5860FF",
                    new[]
                    {
                        "Assets/BetterMachines/Textures/smelter-mk2-lod1-a",
                        "Assets/BetterMachines/Textures/smelter-mk2-lod1-n",
                        "Assets/BetterMachines/Textures/smelter-mk2-lod1-s",
                        "Assets/BetterMachines/Textures/smelter-mk2-lod1-e"
                    });

                smelterMk2Lods.SetFloat(emissionSwitch, 1);
                smelterMk2Lods.SetFloat(emissionUsePower, 1);
                smelterMk2Lods.SetFloat(alphaClip, 0.5f);

                Material storageMat = ProtoRegistry.CreateMaterial("VF Shaders/Forward/PBR Standard", "storage-mat", "#DB5860FF",
                    new[]
                    {
                        "Assets/BetterMachines/Textures/storage-a",
                        "Assets/BetterMachines/Textures/storage-n",
                        "Assets/BetterMachines/Textures/storage-s"
                    });

                Material storageMatLod1 = ProtoRegistry.CreateMaterial("VF Shaders/Forward/PBR Standard", "storage-mat", "#DB5860FF",
                    new[]
                    {
                        "Assets/BetterMachines/Textures/storage-lod-a",
                        "Assets/BetterMachines/Textures/storage-lod-n",
                        "Assets/BetterMachines/Textures/storage-lod-s"
                    });

                storageMatLod1.SetFloat(alphaClip, 0.5f);

                Material blackMat =
                    ProtoRegistry.CreateMaterial("VF Shaders/Forward/Black Mask", "black", "#00000000");

                blackMat.renderQueue = 2980;
                blackMat.SetColor(tintColor, Color.black);
                blackMat.SetFloat(invFade, 1.5f);

                Material blackMatToggle =
                    ProtoRegistry.CreateMaterial("VF Shaders/Forward/Black Mask Vertex Toggle", "liquid-tank-black", "#00000000");

                blackMatToggle.renderQueue = 2980;
                blackMatToggle.SetColor(tintColor, Color.black);
                blackMatToggle.SetFloat(invFade, 1.5f);

                Material liquidTankMat = ProtoRegistry.CreateMaterial("VF Shaders/Forward/PBR Standard Vertex Toggle", "liquid-tank-mat", "#FFFFFFFF",
                    new[]
                    {
                        "Assets/BetterMachines/Textures/storage-tank-a",
                        "Assets/BetterMachines/Textures/storage-tank-n",
                        "Assets/BetterMachines/Textures/storage-tank-s"
                    });

                Material liquidTankGlassMat =
                    ProtoRegistry.CreateMaterial("VF Shaders/Forward/PBR Standard Tank Vertex Toggle", "liquid-tank-glass-mat", "#F2FFFA42", new[]
                    {
                        "", "",
                        "Assets/BetterMachines/Textures/storage-tank-glass-s"
                    });
                liquidTankGlassMat.renderQueue = 2989;
                liquidTankGlassMat.SetFloat(albedoMultiplier, 3);
                liquidTankGlassMat.SetFloat(bumpScale, 1);
                liquidTankGlassMat.SetFloat(emissionMultiplier, 10);
                liquidTankGlassMat.SetFloat(smoothMultiplier, 0.8f);
                liquidTankGlassMat.SetFloat(emissionSwitch, 1);
                liquidTankGlassMat.SetColor(specularColor, new Color(0.1981132f, 0.1981132f, 0.1981132f, 1));
                liquidTankGlassMat.SetColor(rimColor, new Color(0.6039216f, 0.8462228f, 0.8588235f, 1));

                string[] chemicalTextures =
                {
                    "Assets/BetterMachines/Textures/chemical-plant-a",
                    "Assets/BetterMachines/Textures/chemical-plant-n",
                    "Assets/BetterMachines/Textures/chemical-plant-s",
                    "Assets/BetterMachines/Textures/chemical-plant-e"
                };

                Material chemicalPlantMatMk2 =
                    ProtoRegistry.CreateMaterial("VF Shaders/Forward/PBR Standard", "chemical-plant-mk2", "#00FFE8FF", chemicalTextures);

                //Material chemicalPlantMatMk3 =
                //    ProtoRegistry.CreateMaterial("VF Shaders/Forward/PBR Standard", "chemical-plant-mk2", "#5FB9FFFF", chemicalTextures);

                chemicalPlantMatMk2.SetFloat(emissionSwitch, 1);
                chemicalPlantMatMk2.SetFloat(emissionUsePower, 1);
                
               // chemicalPlantMatMk3.SetFloat(emissionSwitch, 1);
                //chemicalPlantMatMk3.SetFloat(emissionUsePower, 1);
                
                Material chemicalPlantMatGlass = ProtoRegistry.CreateMaterial("VF Shaders/Forward/PBR Standard Glass", "chemical-plant-glass", "#D7F7FF29",
                    new[]
                    {
                        "", "",
                        "Assets/BetterMachines/Textures/chemical-plant-glass-s"
                    });

                chemicalPlantMatGlass.renderQueue = 3001;
                chemicalPlantMatGlass.SetColor(emissionColor, Color.black);
                chemicalPlantMatGlass.SetColor(rimColor, new Color(0.6448024f, 0.8279268f, 0.8490566f, 1f));

                Material chemicalPlantMatWater = ProtoRegistry.CreateMaterial("VF Shaders/Forward/PBR Standard Glass", "chemical-plant-glass",
                    new Color(1f, 0.6380855f, 0.1650943f, 0.5176471f));

                chemicalPlantMatWater.renderQueue = 3000;
                chemicalPlantMatWater.SetColor(emissionColor, Color.black);

                string[] chemicallodTextures =
                {
                    "Assets/BetterMachines/Textures/chemical-plant-lod-a",
                    "Assets/BetterMachines/Textures/chemical-plant-lod-n",
                    "Assets/BetterMachines/Textures/chemical-plant-lod-s",
                    "Assets/BetterMachines/Textures/chemical-plant-lod-e"
                };

                Material chemicalPlantMatMk2Lod =
                    ProtoRegistry.CreateMaterial("VF Shaders/Forward/PBR Standard", "chemical-plant-mk2-lod", "#00FFE8FF", chemicallodTextures);
               // Material chemicalPlantMatMk3Lod =
               //     ProtoRegistry.CreateMaterial("VF Shaders/Forward/PBR Standard", "chemical-plant-mk2-lod", "#5FB9FFFF", chemicallodTextures);

                chemicalPlantMatMk2Lod.SetFloat(emissionSwitch, 1);
                chemicalPlantMatMk2Lod.SetFloat(emissionUsePower, 1);
                chemicalPlantMatMk2Lod.SetFloat(alphaClip, 0.5f);
                
                chemicalPlantMatMk2Lod.SetFloat(emissionSwitch, 1);
                chemicalPlantMatMk2Lod.SetFloat(emissionUsePower, 1);
                
                //chemicalPlantMatMk3Lod.SetFloat(emissionSwitch, 1);
                //chemicalPlantMatMk3Lod.SetFloat(emissionUsePower, 1);
                //chemicalPlantMatMk3Lod.SetFloat(alphaClip, 0.5f);

                //chemicalPlantMatMk3Lod.SetFloat(emissionSwitch, 1);
                //chemicalPlantMatMk3Lod.SetFloat(emissionUsePower, 1);
                

                gizmoMat = resources.bundle.LoadAsset<Material>("Assets/BetterMachines/Materials/SolidGizmo.mat");
                gizmoMat.shaderKeywords = new[] {"_ALPHABLEND_ON", "_EMISSION"};

                gizmoLineMat = resources.bundle.LoadAsset<Material>("Assets/BetterMachines/Materials/SolidLine.mat");
                gizmoLineMat.shaderKeywords = new[] {"_ALPHABLEND_ON", "_EMISSION"};

                #endregion

                #region Items

                ItemProto smelterMk3Item = LDB.items.Select(2315);
                smelterMk3Item.SetIcon("Assets/BetterMachines/Icons/smelter-3", false);

                smelterMk2item = ProtoRegistry.RegisterItem(3700, "smelterMk2", "smelterMk2Desc",
                    "Assets/BetterMachines/Icons/smelter-2", ProtoRegistry.GetGridIndex(2, 5, 4), 50, EItemType.Production);
                bmItems.Add(smelterMk2item);
                
                ItemProto chemicalPlantMk2item = ProtoRegistry.RegisterItem(3701, "chemicalPlantMk2", "chemicalPlantMk2Desc",
                    "Assets/BetterMachines/Icons/chemical-plant-mk2", ProtoRegistry.GetGridIndex(2, 5, 5), 50, EItemType.Production);
                bmItems.Add(chemicalPlantMk2item);
                    
                /*ItemProto chemicalPlantMk3item = ProtoRegistry.RegisterItem(3702, "chemicalPlantMk3", "chemicalPlantMk3Desc",
                    "Assets/BetterMachines/Icons/chemical-plant-mk3", ProtoRegistry.GetGridIndex(2, 6, 5), 50, EItemType.Production);
                bmItems.Add(chemicalPlantMk3item);*/

                ItemProto storageMk3Item = ProtoRegistry.RegisterItem(3703, "storageMk3", "storageMk3Desc",
                    "Assets/BetterMachines/Icons/storage-3", ProtoRegistry.GetGridIndex(2, 1, 6), 50, EItemType.Logistics);
                bmItems.Add(storageMk3Item);

                ItemProto storageMk4Item = ProtoRegistry.RegisterItem(3704, "storageMk4", "storageMk4Desc",
                    "Assets/BetterMachines/Icons/storage-4", ProtoRegistry.GetGridIndex(2, 2, 6), 50, EItemType.Logistics);
                bmItems.Add(storageMk4Item);

                ItemProto tankMk2Item = ProtoRegistry.RegisterItem(3705, "liquidTankMk2", "liquidTankMk2Desc",
                    "Assets/BetterMachines/Icons/storage-tank-2", ProtoRegistry.GetGridIndex(2, 1, 5), 50, EItemType.Logistics);
                bmItems.Add(tankMk2Item);

                ItemProto tankMk3Item = ProtoRegistry.RegisterItem(3706, "liquidTankMk3", "liquidTankMk3Desc",
                    "Assets/BetterMachines/Icons/storage-tank-3", ProtoRegistry.GetGridIndex(2, 2, 5), 50, EItemType.Logistics);
                bmItems.Add(tankMk3Item);

                beltMk4Item = ProtoRegistry.RegisterItem(3707, "beltMk4", "beltMk4Desc",
                    "Assets/BetterMachines/Icons/belt-4", ProtoRegistry.GetGridIndex(2, 3, 5), 300, EItemType.Logistics);
                bmItems.Add(beltMk4Item);


                #endregion

                #region Models

                ProtoRegistry.RegisterModel(450, smelterMk2item, "Assets/BetterMachines/Machines/smelter-mk2",
                    new[] {smelterMk2Part1, smelterMk2Part2}, new[] {22, 11, 12, 1}, 502, 2, new[] {2302, 0, 2315});
                ProtoRegistry.AddLodMaterials("Assets/BetterMachines/Machines/smelter-mk2", 1, new[] {smelterMk2Lods});
                ProtoRegistry.AddLodMaterials("Assets/BetterMachines/Machines/smelter-mk2", 2, new[] {smelterMk2Lods});

                ProtoRegistry.RegisterModel(451, chemicalPlantMk2item, "Assets/BetterMachines/Machines/chemical-plant-mk2",
                    new[] {chemicalPlantMatMk2, chemicalPlantMatGlass, blackMat, chemicalPlantMatWater}, new[] {22, 11, 12, 1}, 704, 2,
                    new[] {2309, 0, 2317});
                ProtoRegistry.AddLodMaterials("Assets/BetterMachines/Machines/chemical-plant-mk2", 1, new[] {chemicalPlantMatMk2Lod, chemicalPlantMatGlass});
                ProtoRegistry.AddLodMaterials("Assets/BetterMachines/Machines/chemical-plant-mk2", 2, new[] {chemicalPlantMatMk2Lod, chemicalPlantMatGlass});

                /*ProtoRegistry.RegisterModel(452, chemicalPlantMk3item, "Assets/BetterMachines/Machines/chemical-plant-mk3",
                    new[] {chemicalPlantMatMk3, chemicalPlantMatGlass, blackMat, chemicalPlantMatWater}, new[] {22, 11, 12, 1}, 705, 3,
                    new[] {2309, chemicalPlantMk2item.ID, 0});
                ProtoRegistry.AddLodMaterials("Assets/BetterMachines/Machines/chemical-plant-mk3", 1, new[] {chemicalPlantMatMk3Lod, chemicalPlantMatGlass});
                ProtoRegistry.AddLodMaterials("Assets/BetterMachines/Machines/chemical-plant-mk3", 2, new[] {chemicalPlantMatMk3Lod, chemicalPlantMatGlass});*/

                ProtoRegistry.RegisterModel(453, storageMk3Item, "Assets/BetterMachines/Machines/storage-3",
                    new[] {storageMat}, new[] {17, 1}, 403);
                ProtoRegistry.AddLodMaterials("Assets/BetterMachines/Machines/storage-3", 1, new[] {storageMatLod1});


                ProtoRegistry.RegisterModel(454, storageMk4Item, "Assets/BetterMachines/Machines/storage-4",
                    new[] {storageMat}, new[] {17, 1}, 404);
                ProtoRegistry.AddLodMaterials("Assets/BetterMachines/Machines/storage-4", 1, new[] {storageMatLod1});

                liquidTankMk2 = ProtoRegistry.RegisterModel(455, tankMk2Item, "Assets/BetterMachines/Machines/liquid-tank-mk2",
                    new[] {liquidTankMat, blackMatToggle, liquidTankGlassMat}, new[] {30, 1}, 406, 0, Array.Empty<int>(), 3);

                liquidTankMk3 = ProtoRegistry.RegisterModel(456, tankMk3Item, "Assets/BetterMachines/Machines/liquid-tank-mk3",
                    new[] {liquidTankMat, blackMatToggle, liquidTankGlassMat}, new[] {30, 1}, 407, 0, Array.Empty<int>(), 3);

                beltMk4 = ProtoRegistry.RegisterModel(457, beltMk4Item, "Assets/BetterMachines/Machines/belt-4",
                    null, new[] {15, 1}, 304, 4, new[] {2001, 2002, 2003, 0});

                #endregion

                #region Recipes

                // Smelter mk2 <= Smelter mk1,  Titanium alloy * 4, Processor * 4, Magnetic coil * 8
                ProtoRegistry.RegisterRecipe(250, ERecipeType.Assemble, 240, new[] {2302, 1107, 1303, 1202}, new[] {1, 4, 4, 8},
                    new[] {smelterMk2item.ID}, new[] {1}, "smelterMk2Desc", 1202);

                // Smelter mk3 <= Smelter mk2,  Particle container * 8, Plane filter * 4, Particle broadband * 8
                ProtoRegistry.EditRecipe(116, ERecipeType.Assemble, 300, new[] {smelterMk2item.ID, 1206, 1304, 1402}, new[] {1, 8, 4, 8},
                    new[] {2315}, new[] {1}, "smelterMk3Desc", 1417, ProtoRegistry.GetGridIndex(2, 6, 4));

                // Chem plant mk2 <= Chem plant mk1, Titanium alloy * 4, Titanium glass * 4, Processor * 4
                ProtoRegistry.RegisterRecipe(251, ERecipeType.Assemble, 360, new[] {2309, 1107, 1119, 1303}, new[] {1, 4, 4, 4},
                    new[] {chemicalPlantMk2item.ID}, new[] {1}, "chemicalPlantMk2Desc", 1202);

                // Chem plant mk3 <= Chem plant mk2, Particle broadband * 4,  Plane filter * 2, Quantum chip * 2
                /*ProtoRegistry.RegisterRecipe(252, ERecipeType.Assemble, 420, new[] {chemicalPlantMk2item.ID, 1402, 1304, 1305}, new[] {1, 4, 2, 2},
                    new[] {chemicalPlantMk3item.ID}, new[] {1}, "chemicalPlantMk2Desc", 1203);*/

                ProtoRegistry.RegisterRecipe(253, ERecipeType.Assemble, 300, new[] {1106, 1108}, new[] {12, 12},
                    new[] {storageMk3Item.ID}, new[] {1}, "storageMk3Desc", 1604);

                RecipeProto storageMk4Recipe = ProtoRegistry.RegisterRecipe(254, ERecipeType.Assemble, 360, new[] {1107, 1108}, new[] {16, 16},
                    new[] {storageMk4Item.ID}, new[] {1}, "storageMk4Desc", 1851);

                ProtoRegistry.RegisterRecipe(255, ERecipeType.Assemble, 300, new[] {1103, 1108, 1110}, new[] {12, 12, 8},
                    new[] {tankMk2Item.ID}, new[] {1}, "liquidTankMk2Desc", 1603);

                RecipeProto liquidTankMk3Recipe = ProtoRegistry.RegisterRecipe(256, ERecipeType.Assemble, 360, new[] {1107, 1108, 1119}, new[] {16, 16, 12},
                    new[] {tankMk3Item.ID}, new[] {1}, "liquidTankMk3Desc", 1851);

                ProtoRegistry.RegisterTech(1851, "highDensityStorageTech", "highDensityStorageTechDesc", "highDensityStorageTechConc",
                    "Assets/BetterMachines/Icons/high-density-storage",
                    new[] {1414}, new[] {6001, 6002, 6003}, new[] {20, 20, 2}, 216000, new[] {storageMk4Recipe.ID, liquidTankMk3Recipe.ID},
                    new Vector2(37, 13));

                ProtoRegistry.RegisterRecipe(257, ERecipeType.Assemble, 360, new[] {2003, 1304, 1124}, new[] {3, 1, 1},
                    new[] {beltMk4Item.ID}, new[] {3}, "beltMk4Desc", 1605);

                #endregion

                LDBTool.SetBuildBar(3, 4, beltMk4Item.ID); // Insert belt mk4
                LDBTool.SetBuildBar(3, 5, 2011);
                LDBTool.SetBuildBar(3, 6, 2012);
                LDBTool.SetBuildBar(3, 7, 2013);
                LDBTool.SetBuildBar(3, 8, 2040);
                LDBTool.SetBuildBar(3, 9, 2030);
                LDBTool.SetBuildBar(3, 10, 2020);

                LDBTool.SetBuildBar(4, 5, 2106); //Move default position for Liquid Tank mk1

                LDBTool.SetBuildBar(5, 2, smelterMk2item.ID);
                LDBTool.SetBuildBar(5, 3, 2315); //Move default position for Plane Smelter (MK.III)
                LDBTool.SetBuildBar(5, 4, 2303);
                LDBTool.SetBuildBar(5, 5, 2304);
                LDBTool.SetBuildBar(5, 6, 2305);
                LDBTool.SetBuildBar(5, 7, 2308);
                LDBTool.SetBuildBar(5, 8, 2309);
                LDBTool.SetBuildBar(5, 9, 2314);
                LDBTool.SetBuildBar(5, 10, 2310);

            }

            Harmony harmony = new Harmony(MODGUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            
            UtilSystem.AddLoadMessageHandler(EntityData_Patch.GetFixMessage);

            ProtoRegistry.onLoadingFinished += ModifySpeed;
        }
       

        void ModifySpeed()
        {
            ItemProto assemblerMk2 = LDB.items.Select(2304);
            CustomAssemblerDesc assDesc2 = new CustomAssemblerDesc()
            {
                tier = 2,
                assembleSpeed = 1f
            };
            assDesc2.ApplyProperties(new PrefabDesc());

            assemblerMk2.prefabDesc.assemblerSpeed = (int) (assDesc2.assembleSpeed * 10000);
            
            ItemProto assemblerMk3 = LDB.items.Select(2305);
            CustomAssemblerDesc assDesc3 = new CustomAssemblerDesc()
            {
                tier = 3,
                assembleSpeed = 1.5f
            };
            assDesc3.ApplyProperties(new PrefabDesc());

            assemblerMk3.prefabDesc.assemblerSpeed = (int) (assDesc3.assembleSpeed * 10000);

            // Spray coater grid index
            LDB.recipes.Select(10).GridIndex = ProtoRegistry.GetGridIndex(2, 9, 4);
            LDB.items.Select(2901).GridIndex = ProtoRegistry.GetGridIndex(2, 9, 4);
            
            // Quantum chemical plant
            LDB.recipes.Select(124).GridIndex = ProtoRegistry.GetGridIndex(2, 6, 5);
            
            // Arc smelter grid index
            LDB.recipes.Select(56).GridIndex = ProtoRegistry.GetGridIndex(2, 4, 4);
            
            ItemProto smelterMk1Item = LDB.items.Select(2302);
            ItemProto smelterMk3Item = LDB.items.Select(2315);
            ItemProto chemicalPlantMk1Item = LDB.items.Select(2309);
            ItemProto chemicalPlantMk3Item = LDB.items.Select(2317);
            ItemProto beltMk3 = LDB.items.Select(2003);
            beltMk3.recipes = null;
            beltMk3.FindRecipes();
            smelterMk1Item.FindRecipes();

            // Plane smelter grid index
            smelterMk3Item.GridIndex = ProtoRegistry.GetGridIndex(2, 6, 4);
            smelterMk1Item.recipes = null; //Reload smelter recipes
            smelterMk2item.recipes = null;
            smelterMk3Item.recipes = null;
            smelterMk1Item.FindRecipes();
            smelterMk2item.FindRecipes();
            smelterMk3Item.FindRecipes();
            

            // Chemical plant mk1 grid index
            chemicalPlantMk1Item.GridIndex = ProtoRegistry.GetGridIndex(2, 4, 5);
            chemicalPlantMk1Item.recipes = null;
            chemicalPlantMk1Item.FindRecipes();
            chemicalPlantMk3Item.GridIndex = ProtoRegistry.GetGridIndex(2, 6, 5);
            chemicalPlantMk3Item.recipes = null;
            chemicalPlantMk3Item.FindRecipes();

            ChemicalPlantDesc chemDesc = new ChemicalPlantDesc()
            {
                tier = 3,
                processingSpeed = 2f
            };
            chemDesc.ApplyProperties(chemicalPlantMk3Item.prefabDesc);

            chemicalPlantMk3Item.prefabDesc.workEnergyPerTick = 48000;
            chemicalPlantMk3Item.prefabDesc.idleEnergyPerTick = 1600;
            
            LDB.recipes.Select(22).GridIndex = ProtoRegistry.GetGridIndex(2, 4, 5);

            
            ColorUtility.TryParseHtmlString("#F9D352FF", out Color yellow);
            
            smelterMk3 = LDB.models.Select(194);
            smelterMk3.prefabDesc.lodMaterials[0][0].color = yellow;
            smelterMk3.prefabDesc.lodMaterials[1][0].color = yellow;
            smelterMk3.prefabDesc.lodMaterials[2][0].color = yellow;
            
            SmelterDesc desc = new SmelterDesc()
            {
                tier = 3,
                smeltSpeed = 2f
            };
            desc.ApplyProperties(new PrefabDesc());

            smelterMk3.prefabDesc.assemblerSpeed = (int) (desc.smeltSpeed * 10000);

            smelterMk3.prefabDesc.workEnergyPerTick = 24000;
            smelterMk3.prefabDesc.idleEnergyPerTick = 800;

            beltMk4Item.BuildMode = 2; //Belt Build mode
            beltMk4.prefabDesc.beltPrototype = beltMk4Item.ID;
            BeltLogic_Patches.AddMatAndMesh();
        }
    }
}