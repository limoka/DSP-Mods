using System;
using System.Reflection;
using BepInEx;
using HarmonyLib;

namespace GigaStations.InstallCheck
{
    [BepInPlugin(MODGUID, MODNAME, GigaStationsPlugin.VERSION)]
    public class InstallationChecker : BaseUnityPlugin
    {
        public const string MODNAME = "Giga Stations Installation Checker";
        
        public const string MODGUID = "org.limoka.plugin.GigaStationsInstallCheck";

        public const string LDBTOOLGUID = "me.xiaoye97.plugin.Dyson.LDBTool";

        public static bool preloaderInstalled;
        public static bool ldbToolInstalled;

        public static bool installationIsValid;

        private void Awake()
        {
            Type stationEntry = AccessTools.TypeByName("UIControlPanelStationEntry");
            FieldInfo extraStorageItemsField = stationEntry.GetField("extraStorageItems", BindingFlags.Instance |  BindingFlags.NonPublic);
            
            preloaderInstalled = extraStorageItemsField != null;

            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(LDBTOOLGUID))
            {
                ldbToolInstalled = true;
            }
            
            installationIsValid = preloaderInstalled && ldbToolInstalled;

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
                
                if (!InstallationChecker.ldbToolInstalled)
                {
                    message += "  - Plugin \"LDB Tool\" is missing\n";
                }

                message +=
                    "Please check your installation, make sure you followed installation instructions correctly.\n" +
                    "If you are using a mod manager make sure that all dependencies have been downloaded!\n" +
                    "If nothing helps you can message me via Discord: limoka8";
                
                
                UIMessageBox.Show("Giga Stations mod installation is incorrect!",
                    message, "Ok", UIMessageBox.ERROR);
            }
        }
    }
}