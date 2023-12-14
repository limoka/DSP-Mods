using System;
using System.IO;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using NebulaAPI;

namespace BlueprintTweaks.InstallCheck
{
    [BepInPlugin(MODGUID, MODNAME, BlueprintTweaksPlugin.VERSION)]
    public class InstallationChecker : BaseUnityPlugin
    {
        public const string MODNAME = "Blueprint Tweaks Installation Checker";
        
        public const string MODGUID = "org.kremnev8.plugin.BlueprintTweaksInstallCheck";

        public const string LDBTOOLGUID = "me.xiaoye97.plugin.Dyson.LDBTool";

        public static bool preloaderInstalled;
        public static bool ldbToolInstalled;
        public static bool nebulaAPIInstalled;
        public static bool assetBundleFound;

        public static bool installationIsValid;

        private void Awake()
        {
            Type reformData = AccessTools.TypeByName("BlueprintTweaks.ReformData");
            preloaderInstalled = reformData != null;

            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(LDBTOOLGUID))
            {
                ldbToolInstalled = true;
            }
            
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(NebulaModAPI.API_GUID))
            {
                nebulaAPIInstalled = true;
            }
            
            string pluginfolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            assetBundleFound = File.Exists($"{pluginfolder}/blueprinttweaks");

            installationIsValid = preloaderInstalled && ldbToolInstalled && nebulaAPIInstalled && assetBundleFound;

            Harmony.CreateAndPatchAll(typeof(UIMainMenuPatch));
            
            if (installationIsValid)
            {
                Logger.LogInfo("Check successful. Installation is correct!");
            }
            else
            {
                Logger.LogInfo("Check failed. Mod is installed incorrectly!");
            }
        }
    }
    
    public static class UIMainMenuPatch
    {
        [HarmonyPatch(typeof(VFPreload), "InvokeOnLoadWorkEnded")]
        [HarmonyPostfix]
        public static void OnMainMenuOpen()
        {
            if (!InstallationChecker.installationIsValid)
            {
                string message = "Mod installation is incorrect because:\n";

                if (!InstallationChecker.preloaderInstalled)
                {
                    message += "  - Preloader patch has not been detected\n";
                }
                
                if (!InstallationChecker.assetBundleFound)
                {
                    message += "  - Asset Bundle in installation folder is missing!\n";
                }
                
                if (!InstallationChecker.ldbToolInstalled)
                {
                    message += "  - Plugin \"LDB Tool\" is missing\n";
                }
                
                if (!InstallationChecker.nebulaAPIInstalled)
                {
                    message += "  - Plugin \"Nebula Multiplayer Mod API\" is missing\n";
                }

                message +=
                    "Please check your installation, make sure you followed installation instructions correctly.\n" +
                    "If you are using a mod manager make sure that all dependencies have been downloaded!\n" +
                    "If nothing helps you can message me via Discord: Kremnev8#3756";
                
                
                UIMessageBox.Show("BlueprintTweaks mod installation is incorrect!",
                    message, "Ok", UIMessageBox.ERROR);
            }
        }
    }
}