using System;
using System.IO;
using System.Reflection;
using BepInEx;
using CommonAPI;
using FasterMachines;
using HarmonyLib;
using RebindBuildBar;

namespace InstallCheck
{
    [BepInPlugin(MODGUID, MODNAME, BetterMachinesPlugin.VERSION)]
    public class InstallationChecker : BaseUnityPlugin
    {
        public const string MODNAME = "Better Machines Installation Checker";
        
        public const string MODGUID = "org.kremnev8.plugin.BetterMachinesInstallCheck";

        public const string LDBTOOL_GUID = "me.xiaoye97.plugin.Dyson.LDBTool";

        public static bool ldbToolInstalled;
        public static bool commonAPIInstalled;
        public static bool rebindBuildBarInstalled;
        public static bool assetBundleFound;

        public static bool installationIsValid;

        private void Awake()
        {
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(LDBTOOL_GUID))
            {
                ldbToolInstalled = true;
            }
            
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(RebindBuildBarPlugin.MODGUID))
            {
                rebindBuildBarInstalled = true;
            }
            
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(CommonAPIPlugin.GUID))
            {
                commonAPIInstalled = true;
            }


            string pluginfolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            assetBundleFound = File.Exists($"{pluginfolder}/bettermachines");

            installationIsValid =  ldbToolInstalled && commonAPIInstalled && assetBundleFound;

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

                if (!InstallationChecker.assetBundleFound)
                {
                    message += "  - Asset Bundle in installation folder is missing!\n";
                }
                
                if (!InstallationChecker.ldbToolInstalled)
                {
                    message += "  - Plugin \"LDB Tool\" is missing\n";
                }
                
                if (!InstallationChecker.commonAPIInstalled)
                {
                    message += "  - Plugin \"Common API\" is missing\n";
                }

                if (!InstallationChecker.rebindBuildBarInstalled)
                {
                    message += "  - Recommended plugin \"Rebind Build Bar\" is missing\n";
                }

                message +=
                    "Please check your installation, make sure you followed installation instructions correctly.\n" +
                    "If you are using a mod manager make sure that all dependencies have been downloaded!\n" +
                    "If nothing helps you can message me via Discord: Kremnev8#3756";
                
                
                UIMessageBox.Show("Better Machines mod installation is incorrect!",
                    message, "Ok", UIMessageBox.ERROR);
            }
        }
    }
}