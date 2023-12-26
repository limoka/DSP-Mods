using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using crecheng.DSPModSave.Patches;
using HarmonyLib;
using UnityEngine;

// ReSharper disable SuspiciousTypeConversion.Global

namespace crecheng.DSPModSave
{
    [BepInPlugin(MODGUID, MODNAME, VERSION)]
    public class DSPModSavePlugin : BaseUnityPlugin
    {
        public const string MODGUID = "crecheng.DSPModSave";
        public const string MODNAME = "DSP Mod Save";
        public const string MODID = "DSPModSave";
        public const string VERSION = "1.2.0";

        public static ManualLogSource logger;

        internal static Dictionary<string, ModSaveSettings> allModData = new Dictionary<string, ModSaveSettings>();

        private const string saveExt = ".moddsv";
        private const string autoSaveTmp = "_autosave_tmp";
        private const string autoSave0 = "_autosave_0";
        private const string autoSave1 = "_autosave_1";
        private const string autoSave2 = "_autosave_2";
        private const string autoSave3 = "_autosave_3";
        private const string lastExit = "_lastexit_";

        public const int SAVE_FILE_VERSION = 1;

        private void Start()
        {
            logger = Logger;
            Harmony harmony = new Harmony(MODGUID);
            
            harmony.PatchAll(typeof(GameData_Patch));
            harmony.PatchAll(typeof(GameSave_Patch));

            Init();
            logger.LogInfo("DSP Mod Save is initialized!");
        }

        private void Init()
        {
            foreach (var plugin in Chainloader.PluginInfos.Values)
            {
                RegisterModSaveHandler(plugin.Instance);
            }
        }

        public static void AddModSaveManually(BaseUnityPlugin mod)
        {
            RegisterModSaveHandler(mod);
        }

        public static void RemoveModSaveManually(BaseUnityPlugin mod)
        {
            if (!(mod is IModCanSave)) return;

            string GUID = mod.Info.Metadata.GUID;
            if (allModData.ContainsKey(GUID))
            {
                allModData.Remove(GUID);
            }
        }
        
        private static void RegisterModSaveHandler(BaseUnityPlugin mod)
        {
            if (!(mod is IModCanSave save)) return;

            string GUID = mod.Info.Metadata.GUID;
            if (allModData.ContainsKey(GUID)) return;

            ModSaveSettingsAttribute settingsAttribute = mod.GetType().GetCustomAttribute<ModSaveSettingsAttribute>();
            LoadOrder order = LoadOrder.Postload;

            if (settingsAttribute != null)
            {
                order = settingsAttribute.LoadOrder;
            }

            allModData.Add(GUID, new ModSaveSettings()
            {
                mod = save,
                loadOrder = order
            });
        }

        internal static void OnSave(string saveName)
        {
            if (DSPGame.Game == null)
            {
                logger.LogError("No game to save");
                return;
            }

            if (allModData.Count == 0)
                return;
            saveName = saveName.ValidFileName();
            string path = GameConfig.gameSaveFolder + saveName + saveExt;
            try
            {
                using FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                SaveData(fileStream);
            }
            catch (Exception exception)
            {
                logger.LogError(exception.Message);
            }
        }

        internal static void OnAutoSave()
        {
            string text = GameConfig.gameSaveFolder + GameSave.AutoSaveTmp + saveExt;
            string text2 = GameConfig.gameSaveFolder + autoSave0 + saveExt;
            string text3 = GameConfig.gameSaveFolder + autoSave1 + saveExt;
            string text4 = GameConfig.gameSaveFolder + autoSave2 + saveExt;
            string text5 = GameConfig.gameSaveFolder + autoSave3 + saveExt;
            if (!File.Exists(text)) return;

            if (File.Exists(text5))
            {
                File.Delete(text5);
            }

            if (File.Exists(text4))
            {
                File.Move(text4, text5);
            }

            if (File.Exists(text3))
            {
                File.Move(text3, text4);
            }

            if (File.Exists(text2))
            {
                File.Move(text2, text3);
            }

            File.Move(text, text2);
        }

        private static FileStream currentFileStream;
        private static Dictionary<string, ModSaveEntry> saveEntries = new Dictionary<string, ModSaveEntry>();
        
        internal static void OnPreLoad(string saveName)
        {
            if (currentFileStream != null)
            {
                currentFileStream.Close();
                currentFileStream = null;
            }
            
            if (DSPGame.Game == null)
            {
                logger.LogError("No game to load");
                return;
            }

            string path = GameConfig.gameSaveFolder + saveName + saveExt;
            if (!File.Exists(path))
            {
                logger.LogInfo(saveName + ": Game mod save not exist");
                foreach (var settings in allModData.Values)
                {
                    if (settings.loadOrder != LoadOrder.Preload) continue;
                    
                    settings.mod.IntoOtherSave();
                }

                return;
            }

            try
            {
                logger.LogInfo("Pre Load!");
                currentFileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                LoadData();
                CallImports(LoadOrder.Preload);
            }
            catch (Exception exception)
            {
                logger.LogError(exception.Message);
            }
        }

        internal static void OnPostLoad()
        {
            if (currentFileStream == null)
            {
                foreach (var settings in allModData.Values)
                {
                    if (settings.loadOrder != LoadOrder.Postload) continue;
                    
                    settings.mod.IntoOtherSave();
                }

                return;
            }
            
            logger.LogInfo("Post Load!");
            
            try
            {
                CallImports(LoadOrder.Postload);
            }
            catch (Exception exception)
            {
                logger.LogError(exception.Message);
            }
            
            currentFileStream.Close();
            currentFileStream = null;
            saveEntries.Clear();
        }

        internal static void SaveData(FileStream fileStream)
        {
            using BinaryWriter binaryWriter = new BinaryWriter(fileStream);

            Dictionary<string, long> pos = new Dictionary<string, long>();
            binaryWriter.Write('M');
            binaryWriter.Write('O');
            binaryWriter.Write('D');
            binaryWriter.Write(SAVE_FILE_VERSION);
            binaryWriter.Write(0L);
            binaryWriter.Write(0L);
            binaryWriter.Write(0L);
            binaryWriter.Write(0L);
            binaryWriter.Write(0L);
            binaryWriter.Write(allModData.Count);

            foreach (var name in allModData)
            {
                binaryWriter.Write(name.Key);
                pos.Add(name.Key, fileStream.Position);
                binaryWriter.Write(0L);
                binaryWriter.Write(0L);
            }

            foreach (var data in allModData)
            {
                string name = data.Key;
                if (!pos.ContainsKey(name)) continue;

                long begin = fileStream.Position;
                try
                {
                    using MemoryStream ms = new MemoryStream();
                    using BinaryWriter binary = new BinaryWriter(ms);

                    data.Value.mod.Export(binary);
                    byte[] dataByte = ms.ToArray();
                    fileStream.Write(dataByte, 0, dataByte.Length);
                }
                catch (Exception ex)
                {
                    logger.LogError(data.Key + " : mod data export error!");
                    logger.LogError(ex.Message + "\n" + ex.StackTrace);

                    string message = $"There was an issue saving data of mod {data.Key}." +
                                     "\nYour save game could be corrupted! Please report this to mod author." +
                                     $"\n\nMessage: {ex.Message}, Stacktrace:\n{ex.StackTrace}";

                    UIMessageBox.Show("Mod data export error!", message, "Close", "Copy and Close", 3, () => { },
                        () => { GUIUtility.systemCopyBuffer = message; });
                }

                long end = fileStream.Position;
                fileStream.Seek(pos[name], SeekOrigin.Begin);
                binaryWriter.Write(begin);
                binaryWriter.Write(end);
                fileStream.Seek(0, SeekOrigin.End);
            }
        }
        
        internal static void LoadData()
        {
            using BinaryReader binaryReader = new BinaryReader(currentFileStream, new UTF8Encoding(), true);
            
            saveEntries.Clear();
            bool flag = binaryReader.ReadChar() == 'M';
            flag = flag && binaryReader.ReadChar() == 'O';
            flag = flag && binaryReader.ReadChar() == 'D';
            if (!flag)
            {
                logger.LogError("Error loading save file. Save file is corrupted.");
                return;
            }

            int dataVersion = binaryReader.ReadInt32();
            binaryReader.ReadInt64();
            binaryReader.ReadInt64();
            binaryReader.ReadInt64();
            binaryReader.ReadInt64();
            binaryReader.ReadInt64();
            int count = binaryReader.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                string name = binaryReader.ReadString();
                long begin = binaryReader.ReadInt64();
                long end = binaryReader.ReadInt64();
                saveEntries.Add(name, new ModSaveEntry(name, begin, end));
                logger.LogInfo($"File has mod {name} save data starting from {begin}");
            }
        }

        [SuppressMessage("ReSharper", "MustUseReturnValue")]
        private static void CallImports(LoadOrder currentState)
        {
            foreach (var pair in allModData)
            {
                if (pair.Value.loadOrder != currentState) continue;
                
                if (!saveEntries.ContainsKey(pair.Key))
                {
                    pair.Value.mod.IntoOtherSave();
                    return;
                }

                ModSaveEntry e = saveEntries[pair.Key];
                currentFileStream.Seek(e.begin, SeekOrigin.Begin);
                byte[] b = new byte[e.end - e.begin];
                currentFileStream.Read(b, 0, b.Length);
                
                try
                {
                    using MemoryStream temp = new MemoryStream(b);
                    using BinaryReader binary = new BinaryReader(temp);

                    pair.Value.mod.Import(binary);
                }
                catch (Exception ex)
                {
                    logger.LogError(pair.Key + " :mod data import error!");
                    logger.LogError(ex.Message + "\n" + ex.StackTrace);

                    string message = $"There was an issue loading data of mod {pair.Key}." +
                                     "\nYour save game could be corrupted! Please report this to mod author." +
                                     $"\n\nMessage: {ex.Message}, Stacktrace:\n{ex.StackTrace}";

                    UIMessageBox.Show("Mod data import error!", message, "Close", "Copy and Close", 3, () => { },
                        () => { GUIUtility.systemCopyBuffer = message; });
                }
            }
        }
    }
}