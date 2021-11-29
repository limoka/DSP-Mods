using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;

namespace crecheng.DSPModSave
{
    [HarmonyPatch]
    public static class Patches
    {
        internal static Dictionary<string, IModCanSave> allModData = new Dictionary<string, IModCanSave>();
        
        private const string saveExt = ".moddsv";
        private const string autoSaveTmp = "_autosave_tmp";
        private const string autoSave0 = "_autosave_0";
        private const string autoSave1 = "_autosave_1";
        private const string autoSave2 = "_autosave_2";
        private const string autoSave3 = "_autosave_3";
        private const string lastExit = "_lastexit_";
        
        public const int SAVE_FILE_VERSION = 1;
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameSave), "SaveCurrentGame")]
        static void SaveCurrentGame(bool __result, string saveName)
        {
            if (!__result) return;

            if (DSPGame.Game == null)
            {
                DSPModSavePlugin.logger.LogError("No game to save");
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
                DSPModSavePlugin.logger.LogError(exception.Message);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameSave), "AutoSave")]
        static void AutoSave(bool __result)
        {
            if (!__result) return;

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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameData), "NewGame")]
        static void EnterGame()
        {
            DSPModSavePlugin.logger.LogInfo("Enter New Game");
            foreach (var d in allModData)
            {
                d.Value.IntoOtherSave();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameSave), "LoadCurrentGame")]
        static void LoadCurrentGame(bool __result, string saveName)
        {
            if (!__result) return;

            if (DSPGame.Game == null)
            {
                DSPModSavePlugin.logger.LogError("No game to load");
                return;
            }

            string path = GameConfig.gameSaveFolder + saveName + saveExt;
            if (!File.Exists(path))
            {
                DSPModSavePlugin.logger.LogInfo(saveName + ": Game mod save not exist");
                foreach (var d in allModData)
                {
                    d.Value.IntoOtherSave();
                }

                return;
            }

            try
            {
                using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);

                LoadData(fileStream);
            }
            catch (Exception exception)
            {
                DSPModSavePlugin.logger.LogError(exception.Message);
            }
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

                    data.Value.Export(binary);
                    byte[] dataByte = ms.ToArray();
                    fileStream.Write(dataByte, 0, dataByte.Length);
                }
                catch (Exception ex)
                {
                    DSPModSavePlugin.logger.LogError(data.Key + " : mod data export error!");
                    DSPModSavePlugin.logger.LogError(ex.Message + "\n" + ex.StackTrace);
                }

                long end = fileStream.Position;
                fileStream.Seek(pos[name], SeekOrigin.Begin);
                binaryWriter.Write(begin);
                binaryWriter.Write(end);
                fileStream.Seek(0, SeekOrigin.End);
            }
        }

        internal static void LoadData(FileStream fileStream)
        {
            using BinaryReader binaryReader = new BinaryReader(fileStream);

            Dictionary<string, ModSaveData> data = new Dictionary<string, ModSaveData>();
            bool flag = binaryReader.ReadChar() == 'M';
            flag = flag && binaryReader.ReadChar() == 'O';
            flag = flag && binaryReader.ReadChar() == 'D';
            if (!flag)
            {
                DSPModSavePlugin.logger.LogError("Error loading save file. Save file is corrupted.");
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
                data.Add(name, new ModSaveData(name, begin, end));
            }

            foreach (var d in allModData)
            {
                if (data.ContainsKey(d.Key))
                {
                    ModSaveData e = data[d.Key];
                    fileStream.Seek(e.begin, SeekOrigin.Begin);
                    byte[] b = new byte[e.end - e.begin];
                    fileStream.Read(b, 0, b.Length);
                    try
                    {
                        using MemoryStream temp = new MemoryStream(b);
                        using BinaryReader binary = new BinaryReader(temp);

                        d.Value.Import(binary);
                    }
                    catch (Exception ex)
                    {
                        DSPModSavePlugin.logger.LogError(d.Key + " :mod data import error!");
                        DSPModSavePlugin.logger.LogError(ex.Message + "\n" + ex.StackTrace);
                    }
                }
                else
                {
                    d.Value.IntoOtherSave();
                }
            }
        }
    }
}