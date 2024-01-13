using System;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Logging;
using CommonAPI;
using CommonAPI.Systems;
using CommonAPI.Systems.ModLocalization;
using HarmonyLib;
using UnityEngine;

[module: UnverifiableCode]
#pragma warning disable 618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore 618


namespace RebindBuildBar
{
    [BepInPlugin(MODGUID, MOD_DISP_NAME, VERSION)]
    [BepInDependency(CommonAPIPlugin.GUID)]
    
    [CommonAPISubmoduleDependency(nameof(ProtoRegistry), nameof(CustomKeyBindSystem), nameof(PickerExtensionsSystem), nameof(LocalizationModule))]
    public class RebindBuildBarPlugin : BaseUnityPlugin
    {
        public const string MODNAME = "RebindBuildBar";
        public const string MODGUID = "org.kremnev8.plugin.RebindBuildBar";
        public const string MOD_DISP_NAME = "Rebind Build Bar";

        public const string VERSION = "1.0.4";

        public static ManualLogSource logger;
        public static ResourceData resources;

        private void Awake()
        {
            logger = Logger;

            string pluginfolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            resources = new ResourceData(MODNAME, "RebindBuildBar", pluginfolder);
            resources.LoadAssetBundle("rebindbarbundle");

            LocalizationModule.RegisterTranslation("ResetBuildMenuTipTitle", "Reset Build Bar item binding");
            LocalizationModule.RegisterTranslation("ResetBuildMenuTip", 
                "Pressing this button will reset Build Bar items in current category to their defaults. Holding ctrl (Rebindable) resets all items in all categories.");

            LocalizationModule.RegisterTranslation("ResetBuildMenuQuestionTitle", "Are you sure?");
            LocalizationModule.RegisterTranslation("ResetBuildMenuQuestion1", "This action will reset all Build Bar items in build bar to their defaults");
            LocalizationModule.RegisterTranslation("ResetBuildMenuQuestion2", "This action will reset Build Bar items in current category to their defaults");
            
            LocalizationModule.RegisterTranslation("KEYReassignBuildBar", "Reassign Build Bar item");
            LocalizationModule.RegisterTranslation("KEYClearBuildBar", "Clear Build Bar item");
            
            LocalizationModule.RegisterTranslation("LockedTipText", "Locked");

            CustomKeyBindSystem.RegisterKeyBind<HoldKeyBind>(new BuiltinKey
            {
                id = 150,
                key = new CombineKey(0, CombineKey.CTRL_COMB, ECombineKeyAction.OnceClick, false),
                conflictGroup = KeyBindConflict.KEYBOARD_KEYBIND | KeyBindConflict.UI,
                name = "ReassignBuildBar",
                canOverride = true
            });

            CustomKeyBindSystem.RegisterKeyBind<ReleaseKeyBind>(new BuiltinKey
            {
                id = 151,
                key = new CombineKey((int) KeyCode.Mouse2, 0, ECombineKeyAction.OnceClick, false),
                conflictGroup = KeyBindConflict.MOUSE_KEYBIND | KeyBindConflict.UI,
                name = "ClearBuildBar",
                canOverride = true
            });

            Harmony harmony = new Harmony(MODGUID);
            harmony.PatchAll(typeof(Patches));

            logger.LogInfo("Rebind Build Bar mod is loaded successfully!");
        }

        private void Update()
        {
            Patches.Update();
        }
    }
}