using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
[assembly: SecurityPermission( SecurityAction.RequestMinimum, SkipVerification = true )]
namespace DSP_CustomBuildings
{
    [BepInPlugin("org.kremnev8.plugin.dspcustombuildings", "DSP Custom Buildings", "0.1.0.0")]
    public class DSPCustomBuildings : BaseUnityPlugin
    {
        public static ManualLogSource logger;

        
        //Local proto dictionaries
        public static Dictionary<int, ItemProto> items = new Dictionary<int, ItemProto>();
        public static Dictionary<String, int> itemLookup = new Dictionary<String, int>();
        public static Dictionary<int, RecipeProto> recipes = new Dictionary<int, RecipeProto>();
        public static Dictionary<int, StringProto> strings = new Dictionary<int, StringProto>();
        public static Dictionary<int, ModelProto> models = new Dictionary<int, ModelProto>();
        
        public static AssetBundle bundle;


        void Awake()
        {
            logger = Logger;
            
            //get location of the plugin
            string pluginfolder = System.IO.Path.GetDirectoryName(GetType().Assembly.Location);

            //asset bundle to load
            string assetBundle = "custombundle";
            
            //load assetbundle then load the prefab
            bundle = AssetBundle.LoadFromFile($"{pluginfolder}/{assetBundle}");

            
            //Register and create buildings, items, models, etc
            
            registerStrings("myAssembler", "My Assembler");
            registerStrings("myAssemblerDesc", "This is not an assembler. Why? IDK");
            //Icons/ItemRecipe/assembler-1
            //int nugget = registerItem("ironNugget", "ironNuggetDesc", "Icons/ItemRecipe/iron-plate", 1512);

            int modelID = registerModel("assets/custommachines/prefabs/assembler-mk-1");
            
            
            
            int assemb = registerBuilding("myAssembler", "myAssemblerDesc", "Icons/ItemRecipe/assembler-1", 2501, modelID);
            
            registerRecipe(ERecipeType.Assemble, 60, new []{1101}, new []{1}, new []{assemb}, new []{1}, "myAssemblerDesc", 1);

            
            logger.LogInfo("DSP Custom Buildings is initialized!");
            
            
            
            
            
            LDBTool.PostAddDataAction += onPostAdd;
            
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
                
                Material assMat = LDB.models.modelArray[65].prefabDesc.materials[0];
                pdesc.lodMaterials[0][0] = assMat;
                
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
        public static int registerModel(String prefabPath)
        {
            int id = findAvailableID(100, LDB.models, models);

            ModelProto proto = new ModelProto
            {
                Name = id.ToString(),
                PrefabPath = prefabPath,
                ID = id
            };

            LDBTool.PreAddProto(ProtoType.Model, proto);
            models.Add(proto.ID, proto);
            return proto.ID;
        }

        public static int registerItem(String name, String description, String iconPath, int gridIndex)
        {
            int id = findAvailableID(1001, LDB.items, items);

            ItemProto proto = new ItemProto
            {
                Type = EItemType.Material,
                StackSize = 64,
                FuelType = 0,
                IconPath = iconPath,
                Name = name,
                Description = description,
                GridIndex = gridIndex,
                DescFields = new []{1},
                ID = id
            };

            LDBTool.PreAddProto(ProtoType.Item, proto);
            
            items.Add(proto.ID, proto);
            itemLookup.Add(name, proto.ID);
            return proto.ID;
        }
        
        public static int registerBuilding(String name, String description, String iconPath, int gridIndex, int modelIndex)
        {
            int id = findAvailableID(2000, LDB.items, items);

            ItemProto proto = new ItemProto
            {
                Type = EItemType.Production,
                StackSize = 50,
                IconPath = iconPath,
                Name = name,
                Description = description,
                GridIndex = gridIndex,
                DescFields = new []{22,11,12,1},
                ID = id,
                ModelIndex = modelIndex,
                ModelCount = 1,
                BuildIndex = 703,
                BuildMode = 1,
                IsEntity = true,
                CanBuild = true
                
            };

            LDBTool.PreAddProto(ProtoType.Item, proto);
            
            items.Add(proto.ID, proto);
            itemLookup.Add(name, proto.ID);
            return proto.ID;
        }
        
        public static void registerRecipe(ERecipeType type, int time, int[] input, int[] inCounts, int[] output, int[] outCounts, String description, int techID)
        {
            if (output.Length > 0)
            {
                int id = findAvailableID(100, LDB.recipes, recipes);

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
                    Name = first.Name,
                    preTech = tech,
                    ID = id
                };

                LDBTool.PreAddProto(ProtoType.Recipe, proto);
                recipes.Add(id, proto);
            }
        }
        
        public static void registerStrings(String key, String enTrans)
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
        
    }


    [HarmonyPatch(typeof(UIBuildMenu), "StaticLoad")]
    static class UIBuildMenuPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ItemProto[,] ___protos)
        {
            DSPCustomBuildings.logger.LogInfo("Patching UIBuildMenu");

            foreach (var kv in DSPCustomBuildings.items)
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

    
    [HarmonyPatch(typeof(Resources), "Load", new Type[] { typeof(string), typeof(Type) })]
    static class ResourcesPatch
    {
        [HarmonyPrefix]
        public static bool LoadHook(ref string path, ref UnityEngine.Object __result) {

            if (path.Contains("custommachines"))
            {
                DSPCustomBuildings.logger.LogInfo("Loading my asset " + path);
                if (DSPCustomBuildings.bundle.Contains(path+".prefab"))
                {
                    Material assMat = LDB.models.modelArray[65].prefabDesc.materials[0];
                    UnityEngine.Object myPrefab = DSPCustomBuildings.bundle.LoadAsset(path + ".prefab");
                    if (myPrefab != null)
                    {
                       MeshRenderer[] renderers = ((GameObject) myPrefab).GetComponentsInChildren<MeshRenderer>();
                       foreach (MeshRenderer renderer in renderers)
                       {
                           renderer.sharedMaterial = assMat;
                       }
                    }
                    __result = myPrefab;
                    return false;
                }
            }
            return true;
        }
    }
}