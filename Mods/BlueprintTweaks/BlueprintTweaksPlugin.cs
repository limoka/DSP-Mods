﻿using System;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Logging;
using CommonAPI;
using HarmonyLib;

[module: UnverifiableCode]
#pragma warning disable 618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore 618

namespace BlueprintTweaks
{
    [BepInPlugin(MAINGUID, MAINNAME, VERSION)]
    public class BlueprintTweaksPlugin : BaseUnityPlugin
    {
        public const string MODID = "BlueprintTweaks";
        public const string MAINGUID = "org.kremnev8.plugin." + MODID;
        public const string MAINNAME = "Blueprint Tweaks";
        
        public const string VERSION = "1.0.3";

        public static ManualLogSource logger;
        public static ResourceData resource;

        public static bool cameraToggleEnabled;
        public static bool recipeChangeEnabled;
        public static bool forcePasteEnabled;

        private void Awake()
        {
            logger = Logger;

            cameraToggleEnabled = Config.Bind("General", "cameraToggle", true, "Allow toggling camera between 3rd person and god view\nAll values are applied on restart").Value;
            recipeChangeEnabled = Config.Bind("General", "recipeChange", true, "Add recipe change panel to blueprint inspectors\nAll values are applied on restart").Value;
            forcePasteEnabled = Config.Bind("General", "forcePaste", true, "Allow using key to force blueprint placement\nAll values are applied on restart").Value;
            
            
            string pluginfolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            resource = new ResourceData(MODID, "blueprinttweaks", pluginfolder);
            resource.LoadAssetBundle("blueprinttweaks");
            ProtoRegistry.AddResource(resource);
            
            ProtoRegistry.RegisterString("KEYToggleBPGodModeDesc", "Toggle Blueprint God Mode");
            ProtoRegistry.RegisterString("RecipesLabel", "Recipes");
            ProtoRegistry.RegisterString("ChangeTipText", "Left-click to change recipe");
            
            ProtoRegistry.RegisterString("ChangeTipTitle", "Change Recipe");
            ProtoRegistry.RegisterString("ChangeTipDesc", "Left-click to change recipe. When you click, picker menu will open, where a new recipe can be selected. All machines that used the old recipe will now use the new recipe. This change will take effect after saving.");
            ProtoRegistry.RegisterString("KEYForceBPPlace", "Force Blueprint placement");
            
            KeyBindPatch.Init();
            
            Harmony harmony = new Harmony(MAINGUID);
            
            harmony.PatchAll(typeof(KeyBindPatch));
            
            if (cameraToggleEnabled)
                harmony.PatchAll(typeof(CameraFixPatch));
            if (recipeChangeEnabled)
                harmony.PatchAll(typeof(UIBlueprintInspectorPatch));
            if (forcePasteEnabled)
                harmony.PatchAll(typeof(BlueprintPastePatch));

            logger.LogInfo("Blueprint tweaks mod is initialized!");
        }

        private void Update()
        {
            if (KeyBindPatch.GetKeyBind("ToggleBPGodModeDesc").keyValue)
            {
                CameraFixPatch.mode = !CameraFixPatch.mode;
            }

            BlueprintPastePatch.isEnabled = KeyBindPatch.GetKeyBind("ForceBPPlace").keyValue;
        }
    }
}