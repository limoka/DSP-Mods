using System;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Logging;
using CommonAPI;
using CommonAPI.Systems;
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
    
    [CommonAPISubmoduleDependency(nameof(ProtoRegistry), nameof(CustomKeyBindSystem), nameof(PickerExtensionsSystem))]
    public class RebindBuildBarPlugin : BaseUnityPlugin
    {
        public const string MODNAME = "RebindBuildBar";
        public const string MODGUID = "org.kremnev8.plugin.RebindBuildBar";
        public const string MOD_DISP_NAME = "Rebind Build Bar";

        public const string VERSION = "1.0.0";

        public static ManualLogSource logger;
        public static ResourceData resources;

        private void Awake()
        {
            logger = Logger;

            string pluginfolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            resources = new ResourceData(MODNAME, "RebindBuildBar", pluginfolder);
            resources.LoadAssetBundle("rebindbarbundle");

            ProtoRegistry.RegisterString("ResetBuildMenuTipTitle", "Reset Build Bar item binding");
            ProtoRegistry.RegisterString("ResetBuildMenuTip", 
                "Pressing this button will reset Build Bar items in current category to their defaults. Holding ctrl (Rebindable) resets all items in all categories.");

            ProtoRegistry.RegisterString("ResetBuildMenuQuestionTitle", "Are you sure?");
            ProtoRegistry.RegisterString("ResetBuildMenuQuestion1", "This action will reset all Build Bar items in build bar to their defaults");
            ProtoRegistry.RegisterString("ResetBuildMenuQuestion2", "This action will reset Build Bar items in current category to their defaults");
            
            ProtoRegistry.RegisterString("KEYReassignBuildBar", "Reassign Build Bar item");
            ProtoRegistry.RegisterString("KEYClearBuildBar", "Clear Build Bar item");
            
            ProtoRegistry.RegisterString("LockedTipText", "Locked");

            CustomKeyBindSystem.RegisterKeyBind<HoldKeyBind>(new BuiltinKey
            {
                id = 150,
                key = new CombineKey(0, CombineKey.CTRL_COMB, ECombineKeyAction.OnceClick, false),
                conflictGroup = KeyBindConflict.KEYBOARD_KEYBIND | KeyBindConflict.UI,
                name = "ReassignBuildBar",
                canOverride = true
            });

            CustomKeyBindSystem.RegisterKeyBind<PressKeyBind>(new BuiltinKey
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