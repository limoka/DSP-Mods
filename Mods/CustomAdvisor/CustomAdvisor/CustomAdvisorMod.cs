using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Microsoft.Win32;
using UnityEngine;
using UnityEngine.Networking;
using xiaoye97;

namespace CustomAdvisor
{
    
    public enum AdvisorCharacter
    {
        DEFAULT,
        UKFEMALE,
        //UKMALE,
        CUSTOM
    }
    
    [BepInPlugin(GUID, NAME, VERSION)]
    public class CustomAdvisorMod : BaseUnityPlugin
    {
        public const string GUID = "org.kremnev8.plugin.custom_advisor";
        public const string NAME = "Custom advisor";
        public const string VERSION = "1.0.0";

        public static ManualLogSource logger;
        public static string pluginfolder;

        public static AssetBundle bundle;
        
        public static ConfigEntry<AdvisorCharacter> character;
        public static ConfigEntry<string> customName;


        private void Awake()
        {
            logger = Logger;
            pluginfolder = Path.GetDirectoryName(Assembly.GetAssembly(typeof(CustomAdvisorMod)).Location);
            
            character = Config.Bind("General", "AdvisorCharacter", AdvisorCharacter.UKFEMALE, "Which advisor voice to use. DEFAULT mode means mod is disabled.");
            
            customName = Config.Bind("General", "CustomVoice", "", "Asset bundle name for custom voice pack. Use with CUSTOM mode.");

            if (character.Value.Equals(AdvisorCharacter.DEFAULT))
            {
                logger.LogInfo("Custom advisor mod is disabled by config setting.");
                return;
            }

            string bundleName = character.Value == AdvisorCharacter.CUSTOM ? customName.Value : "customadvisor";
            
            logger.LogDebug($"Loading bundle {bundleName}");
            bundle = AssetBundle.LoadFromFile($"{pluginfolder}/{bundleName}");

            LDBTool.EditDataAction += editLDB;

            logger.LogInfo("Loaded custom advisor mod");
        }

        private void editLDB(Proto proto)
        {
            if (proto is AdvisorTipProto advisor)
            {
                string path = $"assets/customadvisor/{character.Value.ToString()}/{advisor.Voice}.mp3";
                if (!bundle.Contains(path)) return;
                
                AudioClip clip = (AudioClip) bundle.LoadAsset(path);
                logger.LogDebug($"Loading advisor audio clip {advisor.Voice}: {(clip != null ? "Success" : "Failure")}");
                Traverse.Create(advisor).Property("auclip").SetValue(clip);
            }
        }
    }
}