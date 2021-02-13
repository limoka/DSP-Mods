using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Security.Permissions;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using HarmonyLib.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;
using xiaoye97;


[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace DSPAdvancedMiner
{
    [BepInPlugin("org.kremnev8.plugin.dspadvancedminer", "DSP Advanced miner", "0.1.0.4")]
    public class DSPAdvancedMiner : BaseUnityPlugin
    {
        public static ManualLogSource logger;


        //Local proto dictionaries
        public static Dictionary<int, ItemProto> items = new Dictionary<int, ItemProto>();
        public static Dictionary<String, int> itemLookup = new Dictionary<String, int>();

        public static Dictionary<int, RecipeProto> recipes = new Dictionary<int, RecipeProto>();
        public static Dictionary<int, StringProto> strings = new Dictionary<int, StringProto>();

        public static Dictionary<int, ModelProto> models = new Dictionary<int, ModelProto>();
        public static Dictionary<String, Material[]> modelMats = new Dictionary<String, Material[]>();

        public static AssetBundle bundle;

        public static string vertaFolder;


        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int NormalTex = Shader.PropertyToID("_NormalTex");
        private static readonly int MSTex = Shader.PropertyToID("_MS_Tex");
        private static readonly int EmissionTex = Shader.PropertyToID("_EmissionTex");

        public static ConfigEntry<float> configMinerMk2Range;


        void Awake()
        {
            logger = Logger;

            //get location of the plugin
            string pluginfolder = Path.GetDirectoryName(GetType().Assembly.Location);
            
            FileInfo folder = new FileInfo($"{pluginfolder}/Verta/");
            FileInfo folder1 = new FileInfo($"{pluginfolder}/plugins/");
            if (Directory.Exists(folder.Directory?.FullName))
            {
                vertaFolder = pluginfolder;
            }else if (Directory.Exists(folder1.Directory?.FullName))
            {
                vertaFolder = $"{pluginfolder}/plugins";
            }
            else
            {
                logger.LogError("Cannot find folder with verta files. Mod WILL not work!");
                return;
            }
            
            logger.LogInfo(vertaFolder);

            //asset bundle to load
            string assetBundle = "minerbundle";

            //load assetbundle then load the prefab
            bundle = AssetBundle.LoadFromFile($"{pluginfolder}/{assetBundle}");

            configMinerMk2Range = Config.Bind("General",
                "MinerMk2Range",
                10f,
                "How much range miner mk.2 has(Range of miner mk.1 is 7.75m). Note that this applies only to new miners built, already existing will not have their range changed!");

            //Create custom materials
            //orange FF9F3DFF
            //blue 00FFE8FF

            // _MainTex ("Albedo (RGB) diffuse reflection (A) color mask", 2D)
            // _NormalTex ("Normal map", 2D)
            // _MS_Tex ("Metallic (R) transparent paste (G) metal (a) highlight", 2D)
            // _EmissionTex ("Emission (RGB) self-luminous (A) jitter mask", 2D)

            Color newCol;
            ColorUtility.TryParseHtmlString("#00FFE8FF", out newCol);

            Material mainMat = new Material(Shader.Find("VF Shaders/Forward/PBR Standard"));
            mainMat.shaderKeywords = new[] {"_ENABLE_VFINST"};
            mainMat.color = newCol;
            mainMat.name = "mining-drill-mk2";

            Texture2D albedo = bundle.LoadAsset<Texture2D>("assets/custommachines/texture2d/mining-drill-a.png");
            Texture2D normal = bundle.LoadAsset<Texture2D>("assets/custommachines/texture2d/mining-drill-n.png");
            Texture2D metallic = bundle.LoadAsset<Texture2D>("assets/custommachines/texture2d/mining-drill-s.png");
            Texture2D emission = bundle.LoadAsset<Texture2D>("assets/custommachines/texture2d/mining-drill-e.png");
            mainMat.SetTexture(MainTex, albedo);
            mainMat.SetTexture(NormalTex, normal);
            mainMat.SetTexture(MSTex, metallic);
            mainMat.SetTexture(EmissionTex, emission);

            Material blackMat = new Material(Shader.Find("VF Shaders/Forward/Black Mask"));
            blackMat.shaderKeywords = new[] {"_ENABLE_VFINST"};
            blackMat.name = "mining-drill-black";

            //Register and create buildings, items, models, etc
            registerString("advancedMiningDrill", "Mining drill Mk.II");
            registerString("advancedMiningDrillDesc",
                "Thanks to some hard to pronounce tech this drill has better range!");

            ModelProto model = registerModel(178, "assets/custommachines/prefabs/mining-drill-mk2",
                new[] {mainMat, blackMat});

            ItemProto miner = registerBuilding(2000, "advancedMiningDrill", "advancedMiningDrillDesc",
                "assets/custommachines/texture2d/mining-drill-mk2", 2504, model.ID, new[] {18, 19, 11, 12, 1}, 204, 2,
                new[] {2301, 0});

            registerRecipe(105, ERecipeType.Assemble, 60, new[] {2301, 1106, 1303, 1206}, new[] {1, 4, 2, 2},
                new[] {miner.ID}, new[] {1}, "advancedMiningDrillDesc", 1202);


            logger.LogInfo("Advanced Miner mod is initialized!");

            LDBTool.PostAddDataAction += onPostAdd;
            LDBTool.EditDataAction += EditProto;

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        public static void logDebug(String output)
        {
            logger.LogDebug(output);
        }

        //Post register fixups
        private static void onPostAdd()
        {
            foreach (var kv in models)
            {
                kv.Value.Preload();
                PrefabDesc pdesc = kv.Value.prefabDesc;

                if (pdesc.minerType == EMinerType.Vein)
                {
                    pdesc.beltSpeed = 1;
                }

                Material[] mats = pdesc.materials;
                for (int i = 0; i < pdesc.lodMaterials[0].Length; i++)
                {
                    pdesc.lodMaterials[0][i] = mats[i];
                }

                VFPreload.SaveGameObjectResources(pdesc.prefab);
                VFPreload.SaveGameObjectResources(pdesc.colliderPrefab);
                VFPreload.SaveObject(pdesc.mesh);
                VFPreload.SaveMeshes(pdesc.meshes);
                VFPreload.SaveObject(pdesc.meshCollider);
                VFPreload.SaveMaterials(pdesc.materials);
                VFPreload.SaveMeshes(pdesc.lodMeshes);
                VFPreload.SaveMaterials(pdesc.lodMaterials);
                LDB.models.modelArray[kv.Value.ID] = kv.Value;
            }

            foreach (var kv in items)
            {
                kv.Value.Preload(kv.Value.index);
            }

            foreach (var kv in recipes)
            {
                kv.Value.Preload(kv.Value.index);
            }

            logger.LogInfo("Post loading is complete!");
        }

        private static void EditProto(Proto proto)
        {
            if (proto is ItemProto itemProto)
            {
                if (itemProto.ID == 2301)
                {
                    itemProto.Grade = 1;
                    logDebug("Changing grade of " + itemProto.name);
                }

                if (itemProto.Grade == 0 || items.ContainsKey(itemProto.ID)) return;

                foreach (var kv in items)
                {
                    if (kv.Value.Grade == 0 || kv.Value.Upgrades == null) continue;
                    if (itemProto.Grade > kv.Value.Upgrades.Length) continue;

                    if (kv.Value.Upgrades[itemProto.Grade - 1] == itemProto.ID)
                    {
                        itemProto.Upgrades = kv.Value.Upgrades;
                        logDebug("Updating upgrade list of " + itemProto.name);
                    }
                }
            }
        }

        //Finds first available id
        private static int findAvailableID<T>(int startIndex, ProtoSet<T> set, Dictionary<int, T> list) where T : Proto
        {
            int id = startIndex;

            while (true)
            {
                if (!set.dataIndices.ContainsKey(id) && !list.ContainsKey(id))
                {
                    break;
                }

                if (id > 12000)
                {
                    logger.LogError("Failed to find free index!");
                    throw new ArgumentException("No free indices available!");
                }

                id++;
            }

            return id;
        }

        //All of these register a specified proto in LDBTool
        public static ModelProto registerModel(int id, String prefabPath, Material[] mats)
        {
            //int id = findAvailableID(100, LDB.models, models);

            ModelProto proto = new ModelProto
            {
                Name = id.ToString(),
                PrefabPath = prefabPath,
                ID = id
            };

            LDBTool.PreAddProto(ProtoType.Model, proto);
            models.Add(proto.ID, proto);
            modelMats.Add(prefabPath, mats);

            return proto;
        }

        public static ItemProto registerItem(int id, String name, String description, String iconPath, int gridIndex)
        {
            //int id = findAvailableID(1001, LDB.items, items);

            ItemProto proto = new ItemProto
            {
                Type = EItemType.Material,
                StackSize = 50,
                FuelType = 0,
                IconPath = iconPath,
                Name = name,
                Description = description,
                GridIndex = gridIndex,
                DescFields = new[] {1},
                ID = id
            };

            LDBTool.PreAddProto(ProtoType.Item, proto);

            items.Add(proto.ID, proto);
            itemLookup.Add(name, proto.ID);
            return proto;
        }

        public static ItemProto registerBuilding(int id, String name, String description, String iconPath,
            int gridIndex, int modelIndex, int[] descFields, int buildIndex, int grade = 0, int[] upgradesIDs = null)
        {
            ItemProto proto = registerItem(id, name, description, iconPath, gridIndex);

            proto.Type = EItemType.Production;
            proto.ModelIndex = modelIndex;
            proto.ModelCount = 1;
            proto.BuildIndex = buildIndex;
            proto.BuildMode = 1;
            proto.IsEntity = true;
            proto.CanBuild = true;
            proto.DescFields = descFields;
            if (grade != 0 && upgradesIDs != null)
            {
                proto.Grade = grade;
                upgradesIDs[grade - 1] = proto.ID;
                proto.Upgrades = upgradesIDs;
            }

            return proto;
        }

        public static void registerRecipe(int id, ERecipeType type, int time, int[] input, int[] inCounts, int[] output,
            int[] outCounts, String description, int techID = 1)
        {
            if (output.Length > 0)
            {
                ItemProto first = items[output[0]];
                TechProto tech = LDB.techs.Select(techID);

                RecipeProto proto = new RecipeProto
                {
                    Type = type,
                    Handcraft = true,
                    TimeSpend = time,
                    Items = input,
                    ItemCounts = inCounts,
                    Results = output,
                    ResultCounts = outCounts,
                    Description = description,
                    GridIndex = first.GridIndex,
                    IconPath = first.IconPath,
                    Name = first.Name + "Recipe",
                    preTech = tech,
                    ID = id
                };

                LDBTool.PreAddProto(ProtoType.Recipe, proto);
                recipes.Add(id, proto);
            }
        }

        public static void registerString(String key, String enTrans)
        {
            int id = findAvailableID(100, LDB.strings, strings);

            StringProto proto = new StringProto
            {
                Name = key,
                ENUS = enTrans,
                ID = id
            };

            LDBTool.PreAddProto(ProtoType.String, proto);
            strings.Add(id, proto);
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


    [HarmonyPatch(typeof(UIBuildMenu), "StaticLoad")]
    static class UIBuildMenuPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ItemProto[,] ___protos)
        {
            DSPAdvancedMiner.logDebug("Patching UIBuildMenu");

            foreach (var kv in DSPAdvancedMiner.items)
            {
                int buildIndex = kv.Value.BuildIndex;
                if (buildIndex > 0)
                {
                    int num = buildIndex / 100;
                    int num2 = buildIndex % 100;
                    if (num <= 12 && num2 <= 12)
                    {
                        ___protos[num, num2] = kv.Value;
                    }
                }
            }
        }
    }


    [HarmonyPatch(typeof(Resources), "Load", new Type[] {typeof(string), typeof(Type)})]
    static class ResourcesPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref string path, Type systemTypeInstance, ref UnityEngine.Object __result)
        {
            if (path.Contains("custommachines"))
            {
                DSPAdvancedMiner.logDebug("Loading my asset " + path);
                if (DSPAdvancedMiner.bundle.Contains(path + ".prefab") && systemTypeInstance == typeof(GameObject))
                {
                    Material[] mats = DSPAdvancedMiner.modelMats[path];
                    UnityEngine.Object myPrefab = DSPAdvancedMiner.bundle.LoadAsset(path + ".prefab");
                    if (myPrefab != null)
                    {
                        MeshRenderer[] renderers = ((GameObject) myPrefab).GetComponentsInChildren<MeshRenderer>();
                        foreach (MeshRenderer renderer in renderers)
                        {
                            Material[] newMats = new Material[renderer.sharedMaterials.Length];
                            for (int i = 0; i < newMats.Length; i++)
                            {
                                newMats[i] = mats[i];
                            }

                            renderer.sharedMaterials = newMats;
                        }
                    }

                    __result = myPrefab;
                    return false;
                }

                if (DSPAdvancedMiner.bundle.Contains(path + ".png"))
                {
                    UnityEngine.Object mySprite = DSPAdvancedMiner.bundle.LoadAsset(path + ".png", systemTypeInstance);
                    __result = mySprite;
                    return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(VertaBuffer), "LoadFromFile")]
    static class VertaBufferPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref string filename)
        {
            if (filename.Contains("custommachines"))
            {
                filename = $"{DSPAdvancedMiner.vertaFolder}/{filename}";

                DSPAdvancedMiner.logDebug("Loading my verta file " + filename);
            }

            return true;
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
                    Transpilers.EmitDelegate<Func<BuildPreview, float>>(preview => DSPAdvancedMiner.getMinerRadius(preview.desc) + 4)
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
                __instance.planet.physics.nearColliderLogic.GetVeinsInAreaNonAlloc(vector3, DSPAdvancedMiner.getMinerRadius(desc)+4, tmp_ids);
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
                        if (sqrMagnitude <= radius*radius  && num9 >= 0.73f && Mathf.Abs(num8) <= 2f)
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