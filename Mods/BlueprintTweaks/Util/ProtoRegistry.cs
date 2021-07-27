using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BlueprintTweaks;
using HarmonyLib;
using UnityEngine;
using xiaoye97;
// ReSharper disable InconsistentNaming

namespace CommonAPI
{
    public class ResourceData
    {
        public string modId;
        public string modPath;
        public string keyWord;

        public AssetBundle bundle;
        public string vertaFolder;

        public ResourceData(string modId, string keyWord, string modPath)
        {
            this.modId = modId;
            this.modPath = modPath;
            this.keyWord = keyWord;
        }

        public bool HasVertaFolder()
        {
            return !vertaFolder.Equals("");
        }

        public bool HasAssetBundle()
        {
            return bundle != null;
        }

        public void LoadAssetBundle(string bundleName)
        {
            bundle = AssetBundle.LoadFromFile($"{modPath}/{bundleName}");
            if (bundle == null)
            {
                throw new LoadException($"Failed to load asset bundle at {modPath}/{bundleName}");
            }
        }

        public void ResolveVertaFolder()
        {
            FileInfo folder = new FileInfo($"{modPath}/Verta/");
            FileInfo folder1 = new FileInfo($"{modPath}/plugins/");

            if (Directory.Exists(folder.Directory?.FullName))
            {
                vertaFolder = modPath;
            }
            else if (Directory.Exists(folder1.Directory?.FullName))
            {
                vertaFolder = $"{modPath}/plugins";
            }
            else
            {
                vertaFolder = "";
                throw new LoadException($"Failed to resolve verta folder at {modPath}");
            }
        }
    }

    public class LoadException : Exception
    {
        public LoadException(string message) : base(message) { }
    }

    public static class ProtoRegistry
    {
        //Local proto dictionaries
        internal static Dictionary<int, ItemProto> items = new Dictionary<int, ItemProto>();
        internal static Dictionary<int, int> itemUpgradeList = new Dictionary<int, int>();

        internal static Dictionary<int, RecipeProto> recipes = new Dictionary<int, RecipeProto>();
        internal static Dictionary<int, StringProto> strings = new Dictionary<int, StringProto>();

        internal static Dictionary<int, TechProto> techs = new Dictionary<int, TechProto>();
        internal static Dictionary<int, TechProto> techUpdateList = new Dictionary<int, TechProto>();

        internal static Dictionary<int, ModelProto> models = new Dictionary<int, ModelProto>();
        internal static Dictionary<string, Material[]> modelMats = new Dictionary<string, Material[]>();

        public static Dictionary<string, ResourceData> customResources = new Dictionary<string, ResourceData>();

        public static event Action onLoadingFinished;


        internal static int[] textureNames;
        internal static string[] spriteFileExtensions;

        internal static void Init()
        {
            int mainTex = Shader.PropertyToID("_MainTex");
            int normalTex = Shader.PropertyToID("_NormalTex");
            int msTex = Shader.PropertyToID("_MS_Tex");
            int emissionTex = Shader.PropertyToID("_EmissionTex");

            textureNames = new[] {mainTex, normalTex, msTex, emissionTex};
            spriteFileExtensions = new[] {".jpg", ".png", ".tif"};

            LDBTool.PostAddDataAction += OnPostAdd;
            LDBTool.EditDataAction += EditProto;
        }

        /// <summary>
        /// Initialize Registry with needed data
        /// </summary>
        /// <param name="resource"></param>
        public static void AddResource(ResourceData resource)
        {
            customResources.Add(resource.modId, resource);
        }

        //Post register fixups
        private static void OnPostAdd()
        {
            foreach (var kv in models)
            {
                kv.Value.Preload();
                PrefabDesc pdesc = kv.Value.prefabDesc;

                if (!modelMats.ContainsKey(kv.Value.PrefabPath))
                {
                    LDB.models.modelArray[kv.Value.ID] = kv.Value;
                    continue;
                }

                Material[] mats = modelMats[kv.Value.PrefabPath];

                for (int i = 0; i < pdesc.lodCount; i++)
                {
                    for (int j = 0; j < pdesc.lodMaterials[i].Length; j++)
                    {
                        pdesc.lodMaterials[i][j] = mats[j];
                    }
                }

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

            foreach (var kv in techs)
            {
                kv.Value.Preload();
                kv.Value.Preload2();
            }

            foreach (var kv in techUpdateList)
            {
                TechProto oldTech = LDB.techs.Select(kv.Key);
                oldTech.postTechArray = oldTech.postTechArray.AddToArray(kv.Value);
            }

            onLoadingFinished?.Invoke();

            BlueprintTweaksPlugin.logger.LogInfo("Post loading is complete!");
        }

        private static void EditProto(Proto proto)
        {
            if (proto is ItemProto itemProto)
            {
                if (itemUpgradeList.ContainsKey(itemProto.ID))
                {
                    itemProto.Grade = itemUpgradeList[itemProto.ID];
                    BlueprintTweaksPlugin.logger.LogDebug("Changing grade of " + itemProto.name);
                }

                if (itemProto.Grade == 0 || items.ContainsKey(itemProto.ID)) return;

                foreach (var kv in items)
                {
                    if (kv.Value.Grade == 0 || kv.Value.Upgrades == null) continue;
                    if (itemProto.Grade > kv.Value.Upgrades.Length) continue;

                    if (kv.Value.Upgrades[itemProto.Grade - 1] == itemProto.ID)
                    {
                        itemProto.Upgrades = kv.Value.Upgrades;
                        BlueprintTweaksPlugin.logger.LogDebug("Updating upgrade list of " + itemProto.name);
                    }
                }
            }
        }

        //DO NOT use this function, i think it should be removed!
        private static int FindAvailableID<T>(int startIndex, ProtoSet<T> set, Dictionary<int, T> list)
            where T : Proto
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
                    BlueprintTweaksPlugin.logger.LogError("Failed to find free index!");
                    throw new ArgumentException("No free indices available!");
                }

                id++;
            }

            return id;
        }

        /// <summary>
        /// Creates custom material with given shader name.
        /// _MainTex ("Albedo (RGB) diffuse reflection (A) color mask", 2D)
        /// _NormalTex ("Normal map", 2D)
        /// _MS_Tex ("Metallic (R) transparent paste (G) metal (a) highlight", 2D)
        /// _EmissionTex ("Emission (RGB) self-luminous (A) jitter mask", 2D)
        /// </summary>
        /// <param name="shaderName">Name of shader to use</param>
        /// <param name="materialName">Name of finished material, can be anything</param>
        /// <param name="color">Tint color (In html format, #RRGGBBAA)</param>
        /// <param name="textures">Array of texture names in this order: albedo, normal, metallic, emission</param>
        /// <param name="keywords">Array of keywords to use</param>
        /// <param name="textureIDs">Array of texture property ids (Use Shader.PropertyToID)</param>
        public static Material CreateMaterial(string shaderName, string materialName, string color,
            string[] textures = null, string[] keywords = null, int[] textureIDs = null)
        {
            ColorUtility.TryParseHtmlString(color, out Color newCol);

            Material mainMat = new Material(Shader.Find(shaderName))
            {
                shaderKeywords = keywords ?? new[] {"_ENABLE_VFINST"},
                color = newCol,
                name = materialName
            };

            if (textures == null) return mainMat;
            int[] texIds = textureIDs ?? textureNames;

            for (int i = 0; i < textures.Length; i++)
            {
                if (i >= texIds.Length) continue;

                Texture2D texture = Resources.Load<Texture2D>(textures[i]);
                mainMat.SetTexture(texIds[i], texture);
            }

            return mainMat;
        }

        //All of these register a specified proto in LDBTool

        /// <summary>
        /// Registers a ModelProto
        /// </summary>
        /// <param name="id">UNIQUE id of your model</param>
        /// <param name="prefabPath">Path to the prefab, starting from asset folder in your unity project</param>
        /// <param name="mats">List of materials to use</param>
        public static ModelProto RegisterModel(int id, string prefabPath, Material[] mats = null)
        {
            ModelProto model = new ModelProto
            {
                Name = id.ToString(),
                PrefabPath = prefabPath,
                ID = id
            };

            LDBTool.PreAddProto(ProtoType.Model, model);
            models.Add(model.ID, model);

            if (mats != null)
                modelMats.Add(prefabPath, mats);

            return model;
        }

        /// <summary>
        /// Registers a ModelProto and links an proto item to it
        /// </summary>
        /// <param name="id">UNIQUE id of your model</param>
        /// <param name="proto">ItemProto which will be turned into building</param>
        /// <param name="prefabPath">Path to the prefab, starting from asset folder in your unity project</param>
        /// <param name="mats">List of materials to use</param>
        /// <param name="descFields">int Array of used description fields</param>
        /// <param name="buildIndex">Index in build Toolbar, FSS, F - first submenu, S - second submenu</param>
        /// <param name="grade">Grade of the building, used to add upgrading</param>
        /// <param name="upgradesIDs">List of buildings ids, that are upgradable to this one. You need to include all of them here in order. ID of this building should be zero</param>
        public static ModelProto RegisterModel(int id, ItemProto proto, string prefabPath, Material[] mats,
            int[] descFields, int buildIndex, int grade = 0, int[] upgradesIDs = null)
        {
            ModelProto model = new ModelProto
            {
                Name = id.ToString(),
                PrefabPath = prefabPath,
                ID = id
            };

            AddModelToItemProto(model, proto, descFields, buildIndex, grade, upgradesIDs);

            LDBTool.PreAddProto(ProtoType.Model, model);
            models.Add(model.ID, model);

            if (mats != null)
                modelMats.Add(prefabPath, mats);

            return model;
        }

        /// <summary>
        /// Link ModelProto to an ItemProto
        /// </summary>
        /// <param name="model">ModelProto which will contain building model</param>
        /// <param name="item">ItemProto which will be turned into building</param>
        /// <param name="descFields">int Array of used description fields</param>
        /// <param name="buildIndex">Index in build Toolbar, FSS, F - first submenu, S - second submenu</param>
        /// <param name="grade">Grade of the building, used to add upgrading</param>
        /// <param name="upgradesIDs">List of buildings ids, that are upgradable to this one. You need to include all of them here in order. ID of this building should be zero</param>
        public static void AddModelToItemProto(ModelProto model, ItemProto item, int[] descFields, int buildIndex, int grade = 0, int[] upgradesIDs = null)
        {
            item.Type = EItemType.Production;
            item.ModelIndex = model.ID;
            item.ModelCount = 1;
            item.BuildIndex = buildIndex;
            item.BuildMode = 1;
            item.IsEntity = true;
            item.CanBuild = true;
            item.DescFields = descFields;
            if (grade != 0 && upgradesIDs != null)
            {
                item.Grade = grade;
                for (int i = 0; i < upgradesIDs.Length; i++)
                {
                    int itemID = upgradesIDs[i];
                    if (itemID == 0) continue;

                    itemUpgradeList.Add(itemID, i + 1);
                }

                upgradesIDs[grade - 1] = item.ID;
                item.Upgrades = upgradesIDs;
            }
            else
            {
                item.Upgrades = new int[0];
            }
        }

        /// <summary>
        /// Registers a ItemProto
        /// </summary>
        /// <param name="id">UNIQUE id of your item</param>
        /// <param name="name">LocalizedKey of name of the item</param>
        /// <param name="description">LocalizedKey of description of the item</param>
        /// <param name="iconPath">Path to icon, starting from assets folder of your unity project</param>
        /// <param name="gridIndex">Index in craft menu, format : PYXX, P - page</param>
        /// <param name="stackSize">Stack size of the item</param>
        public static ItemProto RegisterItem(int id, string name, string description, string iconPath,
            int gridIndex, int stackSize = 50)
        {
            //int id = findAvailableID(1001, LDB.items, items);

            ItemProto proto = new ItemProto
            {
                Type = EItemType.Material,
                StackSize = stackSize,
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
            return proto;
        }

        /// <summary>
        /// Registers a RecipeProto
        /// </summary>
        /// <param name="id">UNIQUE id of your recipe</param>
        /// <param name="type">Recipe type</param>
        /// <param name="time">Time in ingame ticks. How long item is being made</param>
        /// <param name="input">Array of input IDs</param>
        /// <param name="inCounts">Array of input COUNTS</param>
        /// <param name="output">Array of output IDs</param>
        /// <param name="outCounts">Array of output COUNTS</param>
        /// <param name="description">LocalizedKey of description of this item</param>
        /// <param name="techID">Tech id, which unlock this recipe</param>
        public static RecipeProto RegisterRecipe(int id, ERecipeType type, int time, int[] input, int[] inCounts,
            int[] output,
            int[] outCounts, string description, int techID = 0)
        {
            if (output.Length > 0)
            {
                ItemProto first = items.ContainsKey(output[0]) ? items[output[0]] : LDB.items.Select(output[0]);

                TechProto tech = null;
                if (techID != 0 && LDB.techs.Exist(techID))
                {
                    tech = LDB.techs.Select(techID);
                }

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

                return proto;
            }

            throw new ArgumentException("Output array must not be empty");
        }


        /// <summary>
        /// Registers a TechProto for a technology.
        /// Total amount of each jello is calculated like this: N = H*C/3600, where H - total hash count, C - items per minute of jello.
        /// </summary>
        /// <param name="id"> UNIQUE ID of the technology. Note that if id > 2000 tech will be on upgrades page.</param>
        /// <param name="name">LocalizedKey of name of the tech</param>
        /// <param name="description">LocalizedKey of description of the tech</param>
        /// <param name="conclusion">LocalizedKey of conclusion of the tech upon completion</param>
        /// <param name="iconPath">Path to icon, starting from assets folder of your unity project</param>
        /// <param name="preTechs">Techs which lead to this tech</param>
        /// <param name="jellos">Items required to research the tech</param>
        /// <param name="jelloRate">Amount of items per minute required to research the tech</param>
        /// <param name="hashNeeded">Number of hashes needed required to research the tech</param>
        /// <param name="unlockRecipes">Once the technology has completed, what recipes are unlocked</param>
        /// <param name="position">Vector2 position of the technology on the technology screen</param>
        public static TechProto RegisterTech(int id, string name, string description, string conclusion,
            string iconPath, int[] preTechs, int[] jellos, int[] jelloRate, long hashNeeded,
            int[] unlockRecipes, Vector2 position)

        {
            bool isLabTech = jellos.Any(itemId => LabComponent.matrixIds.Contains(itemId));


            TechProto proto = new TechProto
            {
                ID = id,
                Name = name,
                Desc = description,
                Published = true,
                Conclusion = conclusion,
                IconPath = iconPath,
                IsLabTech = isLabTech,
                PreTechs = preTechs,
                Items = jellos,
                ItemPoints = jelloRate,
                HashNeeded = hashNeeded,
                UnlockRecipes = unlockRecipes,
                AddItems = new int[] { }, // what items to gift after research is done
                AddItemCounts = new int[] { },
                Position = position,
                PreTechsImplicit = new int[] { }, //Those funky implicit requirements
                UnlockFunctions = new int[] { }, //Upgrades.
                UnlockValues = new double[] { },
            };

            foreach (int tech in preTechs)
            {
                //Do not do LDB.techs.Select here, proto could be not added yet.
                techUpdateList.Add(tech, proto);
            }

            LDBTool.PreAddProto(ProtoType.Tech, proto);
            techs.Add(id, proto);

            return proto;
        }

        //TODO resolve string id conflicts and make its ids auto assign
        /// <summary>
        /// Registers a LocalizedKey
        /// </summary>
        /// <param name="key">UNIQUE key of your localizedKey</param>
        /// <param name="enTrans">English translation for this key</param>
        public static void RegisterString(string key, string enTrans)
        {
            int id = FindAvailableID(2566, LDB.strings, strings);

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


    [HarmonyPatch]
    static class UIBuildMenuPatch
    {
        [HarmonyPatch(typeof(UIBuildMenu), "StaticLoad")]
        [HarmonyPostfix]
        public static void Postfix(ItemProto[,] ___protos)
        {
            foreach (var kv in ProtoRegistry.items)
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


    //Fix item stack size not working
    [HarmonyPatch]
    static class StorageComponentPatch
    {
        private static bool staticLoad;

        [HarmonyPatch(typeof(StorageComponent), "LoadStatic")]
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (!staticLoad)
            {
                foreach (var kv in ProtoRegistry.items)
                {
                    StorageComponent.itemIsFuel[kv.Key] = (kv.Value.HeatValue > 0L);
                    StorageComponent.itemStackCount[kv.Key] = kv.Value.StackSize;
                }

                staticLoad = true;
            }
        }
    }

    //Loading custom resources
    [HarmonyPatch]
    static class ResourcesPatch
    {
        [HarmonyPatch(typeof(Resources), "Load", new Type[] {typeof(string), typeof(Type)})]
        [HarmonyPrefix]
        public static bool Prefix(ref string path, Type systemTypeInstance, ref UnityEngine.Object __result)
        {
            foreach (var kv in ProtoRegistry.customResources)
            {
                if (path.Contains(kv.Value.keyWord) && kv.Value.HasAssetBundle())
                {
                    if (kv.Value.bundle.Contains(path + ".prefab") && systemTypeInstance == typeof(GameObject))
                    {
                        UnityEngine.Object myPrefab = kv.Value.bundle.LoadAsset(path + ".prefab");
                        BlueprintTweaksPlugin.logger.LogDebug($"Loading registered asset {path}: {(myPrefab != null ? "Success" : "Failure")}");
                        
                        if (!ProtoRegistry.modelMats.ContainsKey(path))
                        {
                            __result = myPrefab;
                            return false;
                        }

                        Material[] mats = ProtoRegistry.modelMats[path];
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

                    foreach (string extension in ProtoRegistry.spriteFileExtensions)
                    {
                        if (!kv.Value.bundle.Contains(path + extension)) continue;
                        
                        UnityEngine.Object mySprite = kv.Value.bundle.LoadAsset(path + extension, systemTypeInstance);

                        BlueprintTweaksPlugin.logger.LogDebug($"Loading registered asset {path}: {(mySprite != null ? "Success" : "Failure")}");

                        __result = mySprite;
                        return false;
                    }
                }
            }

            return true;
        }
    }

    [HarmonyPatch]
    static class VertaBufferPatch
    {
        [HarmonyPatch(typeof(VertaBuffer), "LoadFromFile")]
        [HarmonyPrefix]
        public static bool Prefix(ref string filename)
        {
            foreach (var kv in ProtoRegistry.customResources)
            {
                if (!filename.Contains(kv.Value.keyWord) || !kv.Value.HasVertaFolder()) continue;

                string newName = $"{kv.Value.vertaFolder}/{filename}";
                if (!File.Exists(newName)) continue;

                filename = newName;
                BlueprintTweaksPlugin.logger.LogDebug("Loading registered verta file " + filename);
                break;
            }

            return true;
        }
    }
}