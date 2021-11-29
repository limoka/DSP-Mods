using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace crecheng.DSPModSave
{
    [BepInPlugin(MODGUID, MODNAME, VERSION)]
    public class DSPModSavePlugin : BaseUnityPlugin
    {
        public const string MODGUID = "crecheng.DSPModSave";
        public const string MODNAME = "DSP Mod Save";
        public const string MODID = "DSPModSave";
        public const string VERSION = "1.1.0";

        public static ManualLogSource logger;

        private void Start()
        {
            logger = Logger;
            Harmony harmony = new Harmony(MODGUID);
            harmony.PatchAll(typeof(Patches));
            
            Init();
            logger.LogInfo("DSP Mod Save is initialized!");
        }

        private void Init()
        {
            foreach (var d in BepInEx.Bootstrap.Chainloader.PluginInfos)
            {
                if (d.Value.Instance is IModCanSave save)
                {
                    Patches.allModData.Add(d.Value.Metadata.GUID, save);
                }
            }
        }
    }
}