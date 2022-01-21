using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace xiaoye97
{
    public static class LDBTool
    {
        #region Public Interface
        
        // Add data action
        public static Action PreAddDataAction, PostAddDataAction;

        // Action to modify data
        public static Action<Proto> EditDataAction;
        
        /// <summary>
        /// Set up the construction shortcut bar
        /// </summary>
        /// <param name="category">第几栏</param>
        /// <param name="index">第几个格子</param>
        /// <param name="itemId">物品ID</param>
        public static void SetBuildBar(int category, int index, int itemId)
        {
            if (category < 1 || category > 12)
            {
                LDBToolPlugin.logger.LogWarning("SetBuildBar Fail. category must be between 1 and 12.");
                return;
            }

            if (index < 1 || index > 12)
            {
                LDBToolPlugin.logger.LogWarning("SetBuildBar Fail. index must be between 1 and 12.");
                return;
            }

            if (UIBuildMenu.staticLoaded && Finshed) // 如果已经加载
            {
                var item = LDB.items.Select(itemId);
                if (item != null)
                {
                    UIBuildMenu.protos[category, index] = item;
                    LDBToolPlugin.logger.LogInfo($"Set build bar at {category},{index} ID:{item.ID} name:{item.Name.Translate()}");
                }
                else
                {
                    LDBToolPlugin.logger.LogWarning($"SetBuildBar Fail. ItemProto with ID {itemId} not found.");
                }
            }
            else
            {
                if (!BuildBarDict.ContainsKey(category))
                {
                    BuildBarDict.Add(category, new Dictionary<int, int>());
                }

                BuildBarDict[category][index] = itemId;
            }
        }
        
        /// <summary>
        /// Add data before game data loads
        /// </summary>
        /// <param name="proto">要添加的Proto</param>
        public static void PreAddProto(Proto proto)
        {
            int index = ProtoIndex.GetIndex(proto.GetType());
            
            if (!PreToAdd[index].Contains(proto))
            {
                if (proto is StringProto)
                {
                    int id = FindAvailableStringID();
                    proto.ID = id;
                }

                Bind(proto);
                PreToAdd[index].Add(proto);
                TotalDict[index].Add(proto);
            }
        }

        /// <summary>
        /// Add data before game data loads
        /// </summary>
        [Obsolete("Please use PreAddProto(Proto proto)")]
        public static void PreAddProto(ProtoType protoType, Proto proto)
        {
            PreAddProto(proto);
        }
        
        
        /// <summary>
        /// Add data after the game data is loaded
        /// </summary>
        /// <param name="proto">要添加的Proto</param>
        public static void PostAddProto(Proto proto)
        {
            int index = ProtoIndex.GetIndex(proto.GetType());
            
            if (!PostToAdd[index].Contains(proto))
            {
                if (proto is StringProto)
                {
                    int id = FindAvailableStringID();
                    proto.ID = id;
                }
                
                Bind(proto);
                PostToAdd[index].Add(proto);
                TotalDict[index].Add(proto);
            }
        }
        
        /// <summary>
        /// Add data after the game data is loaded
        /// </summary>
        [Obsolete("Please use PostAddProto(Proto proto)")]
        public static void PostAddProto(ProtoType protoType, Proto proto)
        {
            PostAddProto(proto);
        }

        #endregion

        #region Implementation

        internal static bool Finshed;
        private static int lastStringId = 1000;
        
        internal static List<List<Proto>> PreToAdd = new List<List<Proto>>();
        internal static List<List<Proto>> PostToAdd = new List<List<Proto>>();
        internal static List<List<Proto>> TotalDict = new List<List<Proto>>();
        
        private static List<Dictionary<string, ConfigEntry<int>>> IDDict = new List<Dictionary<string, ConfigEntry<int>>>();
        private static List<Dictionary<string, ConfigEntry<int>>> GridIndexDict = new List<Dictionary<string, ConfigEntry<int>>>();
        
        private static Dictionary<string, ConfigEntry<string>> ZHCNDict = new Dictionary<string, ConfigEntry<string>>();
        private static Dictionary<string, ConfigEntry<string>> ENUSDict = new Dictionary<string, ConfigEntry<string>>();
        private static Dictionary<string, ConfigEntry<string>> FRFRDict = new Dictionary<string, ConfigEntry<string>>();
        private static Dictionary<int, Dictionary<int, int>> BuildBarDict = new Dictionary<int, Dictionary<int, int>>();
        
        private static ConfigFile CustomID = new ConfigFile($"{Paths.ConfigPath}/LDBTool/LDBTool.CustomID.cfg", true);
        private static ConfigFile CustomGridIndex = new ConfigFile($"{Paths.ConfigPath}/LDBTool/LDBTool.CustomGridIndex.cfg", true);
        private static ConfigFile CustomStringZHCN = new ConfigFile($"{Paths.ConfigPath}/LDBTool/LDBTool.CustomLocalization.ZHCN.cfg", true);
        private static ConfigFile CustomStringENUS = new ConfigFile($"{Paths.ConfigPath}/LDBTool/LDBTool.CustomLocalization.ENUS.cfg", true);
        private static ConfigFile CustomStringFRFR = new ConfigFile($"{Paths.ConfigPath}/LDBTool/LDBTool.CustomLocalization.FRFR.cfg", true);
        
        public static PropertyInfo OrphanedEntriesProp;

        internal static void Init()
        {
            Type configFile = AccessTools.TypeByName("BepInEx.Configuration.ConfigFile");
            OrphanedEntriesProp = configFile.GetProperty("OrphanedEntries", AccessTools.all);
            
            for (int i = 0; i <= ProtoIndex.GetProtosCount(); i++)
            {
                PreToAdd.Add(new List<Proto>());
                PostToAdd.Add(new List<Proto>());
                TotalDict.Add(new List<Proto>());
                IDDict.Add(new Dictionary<string, ConfigEntry<int>>());
                GridIndexDict.Add(new Dictionary<string, ConfigEntry<int>>());
            }

            UpdateCustomDataFile(CustomGridIndex);
            UpdateCustomDataFile(CustomStringENUS);
            UpdateCustomDataFile(CustomStringZHCN);
            UpdateCustomDataFile(CustomStringFRFR);
        }

        internal static void UpdateCustomDataFile(ConfigFile file)
        {
            if (!file.ContainsKey(new ConfigDefinition("General", "Version")))
            {
                File.Copy(file.ConfigFilePath, file.ConfigFilePath + ".bak", true);
                file.Clear();
                ((Dictionary<ConfigDefinition, string>)OrphanedEntriesProp.GetValue(file)).Clear();
            }
            
            ConfigEntry<string> version = file.Bind("General", "Version", LDBToolPlugin.VERSION, "Version of LDB Tool which created this file. DO NOT EDIT THIS MANUALLY!");
            version.Value = LDBToolPlugin.VERSION;
            file.Save();
        }

        /// <summary>
        /// Automatically set the construction shortcut bar
        /// </summary>
        internal static void SetBuildBar()
        {
            foreach (var kv in BuildBarDict)
            {
                foreach (var kv2 in kv.Value)
                {
                    var item = LDB.items.Select(kv2.Value);
                    if (item != null)
                    {
                        UIBuildMenu.protos[kv.Key, kv2.Key] = item;
                        LDBToolPlugin.logger.LogInfo($" Set build bar at {kv.Key},{kv2.Key} ID:{item.ID} name:{item.Name.Translate()}");
                    }
                    else
                    {
                        LDBToolPlugin.logger.LogWarning($"SetBuildBar Fail. ItemProto with ID {kv2.Value} not found.");
                    }
                }
            }
        }
        
        /// <summary>
        /// User configuration data binding
        /// </summary>
        private static void Bind(Proto proto)
        {
            IdBind(proto);
            GridIndexBind(proto);
            StringBind(proto);
        }

        /// <summary>
        /// Bind the ID through the configuration file, allowing players to customize the ID in the event of conflict
        /// </summary>
        private static void IdBind(Proto proto)
        {
            if (proto is StringProto) return;
            int index = ProtoIndex.GetIndex(proto);
            
            var entry = CustomID.Bind(ProtoIndex.GetProtoName(proto), proto.Name, proto.ID);
            proto.ID = entry.Value;
            
            if (IDDict[index].ContainsKey(proto.Name))
            {
                LDBToolPlugin.logger.LogError($"[CustomID] ID:{proto.ID} Name:{proto.Name} There is a conflict, please check.");
            }
            else
            {
                IDDict[index].Add(proto.Name, entry);
            }
        }

        /// <summary>
        /// Bind GridIndex through the configuration file, allowing players to customize GridIndex in the event of conflict
        /// Execute after custom ID
        /// </summary>
        private static void GridIndexBind(Proto proto)
        {
            ConfigEntry<int> entry = null;
            
            if (proto is ItemProto item)
            {
                entry = CustomGridIndex.Bind(ProtoIndex.GetProtoName(proto), item.ID.ToString(), 0, $"Default Grid Index = {item.GridIndex}\nItem Name = {item.Name}");
                
                if (entry.Value != 0)
                    item.GridIndex = entry.Value;
            }
            else if (proto is RecipeProto recipe)
            {
                entry = CustomGridIndex.Bind(ProtoIndex.GetProtoName(proto), recipe.ID.ToString(), 0, $"Default Grid Index = {recipe.GridIndex}\nRecipe Name = {recipe.Name}");
               
                if (entry.Value != 0)
                    recipe.GridIndex = entry.Value;
            }

            if (entry == null) return;
            
            int index = ProtoIndex.GetIndex(proto);

            if (GridIndexDict[index].ContainsKey(proto.Name))
            {
                LDBToolPlugin.logger.LogError($"[CustomGridIndex] ID:{proto.ID} Name:{proto.Name} There is a conflict, please check.");
            }
            else
            {
                GridIndexDict[index].Add(proto.Name, entry);
            }
        }

        /// <summary>
        /// Bind translation files through configuration files, allowing players to customize translations when translations are missing or inaccurate
        /// </summary>
        private static void StringBind(Proto proto)
        {
            if (!(proto is StringProto stringProto)) return;
            
            var zhcn = CustomStringZHCN.Bind("String", stringProto.Name, "", "Default String = " + stringProto.ZHCN);
            var enus = CustomStringENUS.Bind("String", stringProto.Name, "", "Default String = " + stringProto.ENUS);
            var frfr = CustomStringFRFR.Bind("String", stringProto.Name, "", "Default String = " + stringProto.FRFR);
                
            if (!string.Equals(zhcn.Value, ""))
                stringProto.ZHCN = zhcn.Value;

            if (!string.Equals(enus.Value, ""))
                stringProto.ENUS = enus.Value;

            if (!string.Equals(frfr.Value, ""))
                stringProto.FRFR = frfr.Value;
                
            if (!string.IsNullOrEmpty(zhcn.Value))
            {
                if (ZHCNDict.ContainsKey(stringProto.Name))
                {
                    LDBToolPlugin.logger.LogError($"[CustomLocalization.ZHCN] Name:{stringProto.Name} There is a conflict, please check.");
                }
                else ZHCNDict.Add(stringProto.Name, zhcn);
            }

            if (ENUSDict != null)
            {
                if (ENUSDict.ContainsKey(stringProto.Name))
                {
                    LDBToolPlugin.logger.LogError($"[CustomLocalization.ENUS] Name:{stringProto.Name} There is a conflict, please check.");
                }
                else ENUSDict.Add(stringProto.Name, enus);
            }

            if (!string.IsNullOrEmpty(frfr.Value))
            {
                if (FRFRDict.ContainsKey(stringProto.Name))
                {
                    LDBToolPlugin.logger.LogError($"[CustomLocalization.FRFR] Name:{stringProto.Name} There is a conflict, please check.");
                }
                else FRFRDict.Add(stringProto.Name, frfr);
            }
        }
        
        /// <summary>
        /// Check if string with id was already registered
        /// </summary>
        /// <param name="id">ID</param>
        private static bool HasStringIdRegisted(int id)
        {
            if (LDB.strings.dataIndices.ContainsKey(id)) return true;
            
            if (PreToAdd[ProtoIndex.GetIndex(typeof(StringProto))].Any(proto => proto.ID == id)) return true;
            if (PostToAdd[ProtoIndex.GetIndex(typeof(StringProto))].Any(proto => proto.ID == id)) return true;
            
            return false;
        }

        /// <summary>
        /// Get last free string ID
        /// </summary>
        /// <returns>Last free ID</returns>
        /// <exception cref="ArgumentException">If there are no free ID's left</exception>
        private static int FindAvailableStringID()
        {
            int id = lastStringId + 1;

            while (true)
            {
                if (!HasStringIdRegisted(id))
                {
                    break;
                }

                if (id > 12000)
                {
                    LDBToolPlugin.logger.LogError("Failed to find free index!");
                    throw new ArgumentException("No free indices available!");
                }

                id++;
            }

            lastStringId = id;

            return id;
        }

        /// <summary>
        /// Add all Protos in datas to corresponding proto sets
        /// </summary>
        /// <param name="datas">List of List of Protos.</param>
        internal static void AddProtos(List<List<Proto>> datas)
        {
            Type[] protoTypes = ProtoIndex.GetAllProtoTypes();
            for (int i = 0; i < protoTypes.Length; i++)
            {
                Type protoType = protoTypes[i];
                PropertyInfo protoProperty = typeof(LDB).GetProperties().First(property =>
                {
                    Type setType = typeof(ProtoSet<>).MakeGenericType(protoType);
                    return setType.IsAssignableFrom(property.PropertyType);
                });

                MethodInfo genericMethod = typeof(LDBTool).GetMethod(nameof(AddProtosToSet), AccessTools.all);
                MethodInfo method = genericMethod.MakeGenericMethod(protoType);

                object protoSet = protoProperty.GetValue(null);
                method.Invoke(null, new[] {protoSet, datas[i]});
            }
        }

        /// <summary>
        /// Add list contains to ProtoSet
        /// </summary>
        private static void AddProtosToSet<T>(ProtoSet<T> protoSet, List<Proto> protos) where T : Proto
        {
            var array = protoSet.dataArray;
            protoSet.Init(array.Length + protos.Count);
            for (int i = 0; i < array.Length; i++)
            {
                protoSet.dataArray[i] = array[i];
            }

            for (int i = 0; i < protos.Count; i++)
            {
                protoSet.dataArray[array.Length + i] = protos[i] as T;

                if (protos[i] is ItemProto item)
                {
                    item.index = array.Length + i;
                }

                if (protos[i] is RecipeProto)
                {
                    RecipeProto proto = protos[i] as RecipeProto;
                    if (proto.preTech != null)
                    {
                        ArrayAddItem(ref proto.preTech.UnlockRecipes, proto.ID);
                        ArrayAddItem(ref proto.preTech.unlockRecipeArray, proto);
                    }
                }

                LDBToolPlugin.logger.LogInfo($"Add {protos[i].ID} {protos[i].Name.Translate()} to {protoSet.GetType().Name}.");
            }

            var dataIndices = new Dictionary<int, int>();
            for (int i = 0; i < protoSet.dataArray.Length; i++)
            {
                protoSet.dataArray[i].sid = protoSet.dataArray[i].SID;
                dataIndices[protoSet.dataArray[i].ID] = i;
            }

            protoSet.dataIndices = dataIndices;
            if (protoSet is StringProtoSet stringProtoSet)
            {
                for (int i = array.Length; i < protoSet.dataArray.Length; i++)
                {
                    stringProtoSet.nameIndices[protoSet.dataArray[i].Name] = i;
                }
            }
        }

        /// <summary>
        /// Append data to the end of array
        /// </summary>
        private static void ArrayAddItem<T>(ref T[] array, T item)
        {
            var list = array.ToList();
            list.Add(item);
            array = list.ToArray();
        }

        #endregion
    }
}