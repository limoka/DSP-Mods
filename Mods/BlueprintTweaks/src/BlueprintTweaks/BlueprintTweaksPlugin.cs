using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BlueprintTweaks.BlueprintBrowserUIChanges;
using BlueprintTweaks.BrowserRememberFolder;
using BlueprintTweaks.FactoryUndo;
using BlueprintTweaks.GlobalPatch;
using BlueprintTweaks.Patches;
using CommonAPI;
using CommonAPI.Systems;
using CommonAPI.Systems.ModLocalization;
using HarmonyLib;
using NebulaAPI;
using UnityEngine;

[module: UnverifiableCode]
#pragma warning disable 618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore 618


namespace BlueprintTweaks
{
    [BepInPlugin(MODGUID, MOD_DISP_NAME, VERSION)]
    [BepInDependency(NebulaModAPI.API_GUID)]
    [BepInDependency(CommonAPIPlugin.GUID)]
    [CommonAPISubmoduleDependency(
        nameof(ProtoRegistry), 
        nameof(CustomKeyBindSystem), 
        nameof(PickerExtensionsSystem),
        nameof(LocalizationModule)
    )]
    public class BlueprintTweaksPlugin : BaseUnityPlugin, IMultiplayerMod
    {
        public const string MODNAME = "BlueprintTweaks";
        
        public const string MODGUID = "org.kremnev8.plugin.BlueprintTweaks";
        
        public const string MOD_DISP_NAME = "Blueprint Tweaks";
        
        public const string VERSION = "1.5.11";

        public const string GENESIS_BOOK_MODGUID = "org.LoShin.GenesisBook";
        public const string FREE_FOUNDATIONS_GUID = "de.Hotte.DSP.FreeFoundations";
        public const string FREE_FOUNDATIONS_GUID_2 = "com.aekoch.mods.dsp.UnlimitedFoundations";
        
        
        // Features keys
        
        public const string DRAG_REMOVE = "DragRemove";
        public const string BLUEPRINT_FOUNDATIONS = "BlueprintFoundations";
        public const string PASTE_LOCKED = "PasteLocked";
        public const string FACTORY_UNDO = "FactoryUndo";

        public static ManualLogSource logger;
        public static ResourceData resource;

        public static DragRemoveBuildTool tool;

        public static bool gotPluginInfo;
        public static bool freeFoundationsIsInstalled;
        public static bool genesisBookIsInstalled;

        public static ConfigEntry<bool> cameraToggleEnabled;
        public static ConfigEntry<bool> addPasteButtonEnabled;
        
        public static ConfigEntry<bool> recipeChangeEnabled;
        public static ConfigEntry<bool> changeTierEnabled;
        public static ConfigEntry<bool> logisticCargoChangeEnabled;
        public static ConfigEntry<bool> beltHintsChangeEnable;
        public static ConfigEntry<bool> keepBlueprintDesc;
        public static ConfigEntry<bool> keepBrowserPath;

        public static ConfigEntry<bool> forcePasteEnabled;
        public static ConfigEntry<bool> axisLockEnabled;
        public static ConfigEntry<bool> gridControlFeature;
        public static ConfigEntry<bool> blueprintMirroring;
        public static ConfigEntry<bool> dragRemove;
        public static ConfigEntry<bool> pasteLocked;
        public static ConfigEntry<bool> moveWithDragNDrop;
        public static ConfigEntry<bool> factoryUndo;
        
        public static ConfigEntry<bool> blueprintFoundations;
        
        public static ConfigEntry<bool> resetFunctionsOnMenuExit;
        public static ConfigEntry<bool> canBlueprintOnGasGiants;
        
        public static ConfigEntry<bool> excludeStations;
        public static ConfigEntry<bool> undoExcludeStations;
        public static ConfigEntry<bool> useFastDismantle;
        public static ConfigEntry<int>  undoMaxHistory;
        public static ConfigEntry<bool> showUndoClearedMessage;

        private void Awake()
        {
            logger = Logger;

            LoadConfigs();
            Config.Save();

            string pluginfolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            resource = new ResourceData(MODNAME, "blueprinttweaks", pluginfolder);
            resource.LoadAssetBundle("blueprinttweaks");
            ProtoRegistry.AddResource(resource);

            string localePath = Path.Combine(pluginfolder, "Locale");
            LocalizationModule.LoadTranslationsFromFolder(localePath);
            
            Harmony harmony = new Harmony(MODGUID);

            if (factoryUndo.Value)
            {
                UndoManager.Init();
            }
            UIBlueprintInspectorPatch.Init();
            BlueprintUtilsPatch2.Init();
            RegisterKeyBinds();

            NebulaModAPI.RegisterPackets(Assembly.GetExecutingAssembly());

            #region Patches

            if (factoryUndo.Value)
            {
                harmony.PatchAll(FACTORY_UNDO);
            }

            if (keepBrowserPath.Value)
            {
                harmony.PatchAll(typeof(UIBlueprintBrowser_Patch));
            }

            if (moveWithDragNDrop.Value)
            {
                harmony.PatchAll(typeof(BuildTool_Copy_Patch));
                harmony.PatchAll(typeof(DragToMoveBlueprints.UIBlueprintBrowser_Patch));
            }

            if (keepBlueprintDesc.Value)
            {
                harmony.PatchAll(typeof(KeepBPName.UIBlueprintInspectorPatch));
            }

            if (pasteLocked.Value)
            {
                harmony.PatchAll(PASTE_LOCKED);
            }

            if (addPasteButtonEnabled.Value)
            {
                harmony.PatchAll(typeof(UIBlueprintBrowserPatch));
            }

            if (blueprintMirroring.Value)
            {
                harmony.PatchAll(typeof(BlueprintUtilsPatch2));
            }

            if (dragRemove.Value)
            {
                harmony.PatchAll(DRAG_REMOVE);
            }

            if (blueprintFoundations.Value)
            {
                harmony.PatchAll(BLUEPRINT_FOUNDATIONS);
            }

            if (changeTierEnabled.Value)
                harmony.PatchAll(typeof(UIBlueprintComponentItemPatch));
            
            if (canBlueprintOnGasGiants.Value)
            {
                harmony.PatchAll(typeof(PlayerControllerPatch));
                harmony.PatchAll(typeof(BuildTool_BlueprintPastePatch));
            }

            if (axisLockEnabled.Value || gridControlFeature.Value)
            {
                harmony.PatchAll(typeof(GridSnappingPatches));
                harmony.PatchAll(typeof(BuildTool_BlueprintPaste_Patch));
            }

            if (cameraToggleEnabled.Value)
                harmony.PatchAll(typeof(CameraFixPatch));
            if (recipeChangeEnabled.Value || gridControlFeature.Value)
                harmony.PatchAll(typeof(UIBlueprintInspectorPatch));
            if (forcePasteEnabled.Value)
            {
                harmony.PatchAll(typeof(BlueprintPastePatch));
            }
            
            harmony.PatchAll(typeof(BuildTool_BlueprintPaste_Patch_3));
            
            #endregion
            
            logger.LogInfo("Blueprint tweaks mod is initialized!");
        }

        private void LoadConfigs()
        {
            cameraToggleEnabled = Config.Bind("Interface", "cameraToggle", true,
                "Allows toggling camera between 3rd person and god view\nAll values are applied on restart");
            addPasteButtonEnabled = Config.Bind("Interface", "addBluprintPasteButton", true,
                "If enabled new button will be added to Blueprint Browser. Pressing it will paste curretly selected blueprint\nAll values are applied on restart");

            recipeChangeEnabled = Config.Bind("Interface", "recipeChange", true, "Add recipe change panel to blueprint inspectors\nAll values are applied on restart");
            logisticCargoChangeEnabled = Config.Bind("Interface", "changeLogisticCargo", true, "Allows changing cargo requested/provided by logistic stations");
            changeTierEnabled = Config.Bind("Interface", "changeTier", true, "Allows using change tier functionality\nAll values are applied on restart");
            beltHintsChangeEnable = Config.Bind("Interface", "beltHintChange", true,
                "Add belt hint change panel to blueprint inspectors\nAll values are applied on restart");
            keepBlueprintDesc = Config.Bind("Interface", "keepBlueprintDescription", true,
                "When pasting blueprint string into existing blueprint you can hold shift to keep description and icons");
            keepBrowserPath = Config.Bind("Interface", "keepBroserPath", true,
                "Preserve last open Blueprint Browser directory. Also when creating new blueprints, they will be saved in the last open directory");


            forcePasteEnabled = Config.Bind("Features", "forcePaste", true, "Allow to force paste using Shift+Click");
            axisLockEnabled = Config.Bind("Features", "axisLock", true, "Allows using Latitude/Longtitude axis locks\nAll values are applied on restart");
            gridControlFeature = Config.Bind("Features", "gridControl", true, "Allows changing grid size and its offset\nAll values are applied on restart");
            blueprintMirroring = Config.Bind("Features", "blueprintMirroring", true, "Allows mirroring Blueprints\nAll values are applied on restart");
            dragRemove = Config.Bind("Features", "dragRemove", true, "Allows using drag remove function\nAll values are applied on restart");
            pasteLocked = Config.Bind("Features", "PasteLockedRecipes", true,
                "Allow pasting assemblers with recipes which have not been unlocked yet. Assemblers with recipes that are not unlocked will not work.");
            moveWithDragNDrop = Config.Bind("Features", "moveBPWithDragNDrop", true, "Allow moving blueprints using drag and drop");
            factoryUndo = Config.Bind("Features", "factoryUndo", true,
                "Enable Factory Undo feature. Allows to undo/redo most building actions. Will force dragRemove to true");


            blueprintFoundations = Config.Bind("Features", "blueprintFoundations", true,
                "Allow blueprinting foundations along with buildings.\nAll values are applied on restart");

            resetFunctionsOnMenuExit = Config.Bind("Misc", "resetOnExit", true,
                "If enabled when you exit build mode, some functions (Axis/Grid lock, Mirror) will reset their state");
            canBlueprintOnGasGiants = Config.Bind("Misc", "bpOnGasGiants", true, "Allow using Blueprints on Gas Giants\nAll values are applied on restart");

            useFastDismantle = Config.Bind("Misc", "useFastDismantle", true,
                "When using drag remove tool or factory undo, an improved algorithm of removing entities will be used. It is about 20x faster, but might have some imperfections. If you encounter issues you can switch back to vanilla code.");
            excludeStations = Config.Bind("Misc", "excludeStations", true,
                "When using drag remove tool, logistic stations (and miners Mk.II) will not get removed. This is a safeguard against errors which occur most of the time when you try to mass dismantle logistic stations.");

            undoMaxHistory = Config.Bind("Misc", "undoMaxHistory", 50,
                "Defines undo history size. When history reaches it's capacity, old entries will get removed. When using Nebula host controls the used value");
            undoExcludeStations = Config.Bind("Misc", "undoExcludeStations", true, "When enabled factory undo will not undo/redo actions with logistic stations.");

            showUndoClearedMessage = Config.Bind("Misc", "showUndoClearedMessage", true, "Should a message informing you that undo history has been cleared be shown whenever you leave a planet?");

            Config.MigrateConfig<bool>("General", "Interface", new[] { "cameraToggle", "recipeChange", "changeLogisticCargo", "changeTier" });
            Config.MigrateConfig<bool>("General", "Features", new[] { "forcePaste", "axisLock", "gridControl", "gridControl", "blueprintFoundations" });
            Config.MigrateConfig<bool>("General", "Misc", new[] { "bpOnGasGiants" });

            if (factoryUndo.Value)
            {
                dragRemove.Value = true;
            }
        }

        private static void RegisterKeyBinds()
        {
            if (cameraToggleEnabled.Value)
            {
                CustomKeyBindSystem.RegisterKeyBind<PressKeyBind>(new BuiltinKey
                {
                    key = new CombineKey((int) KeyCode.J, 0, ECombineKeyAction.OnceClick, false),
                    conflictGroup = 3071,
                    name = "ToggleBPGodModeDesc",
                    canOverride = true
                });
            }

            if (forcePasteEnabled.Value)
            {
                CustomKeyBindSystem.RegisterKeyBind<HoldKeyBind>(new BuiltinKey
                {
                    key = new CombineKey(0, 1, ECombineKeyAction.OnceClick, false),
                    conflictGroup = 2052,
                    name = "ForceBPPlace",
                    canOverride = true
                });
            }

            if (axisLockEnabled.Value)
            {
                CustomKeyBindSystem.RegisterKeyBind<PressKeyBind>(new BuiltinKey
                {
                    key = new CombineKey((int) KeyCode.G, CombineKey.CTRL_COMB, ECombineKeyAction.OnceClick, false),
                    conflictGroup = 2052,
                    name = "LockLongAxis",
                    canOverride = true
                });

                CustomKeyBindSystem.RegisterKeyBind<PressKeyBind>(new BuiltinKey
                {
                    key = new CombineKey((int) KeyCode.T, CombineKey.CTRL_COMB, ECombineKeyAction.OnceClick, false),
                    conflictGroup = 2052,
                    name = "LockLatAxis",
                    canOverride = true
                });
            }

            if (gridControlFeature.Value)
            {
                CustomKeyBindSystem.RegisterKeyBind<PressKeyBind>(new BuiltinKey
                {
                    key = new CombineKey((int) KeyCode.B, CombineKey.CTRL_COMB, ECombineKeyAction.OnceClick, false),
                    conflictGroup = 2052,
                    name = "SetLocalOffset",
                    canOverride = true
                });
            }
            
            if (blueprintMirroring.Value)
            {
                CustomKeyBindSystem.RegisterKeyBind<PressKeyBind>(new BuiltinKey
                {
                    key = new CombineKey((int) KeyCode.G, CombineKey.SHIFT_COMB, ECombineKeyAction.OnceClick, false),
                    conflictGroup = 2052,
                    name = "MirrorLongAxis",
                    canOverride = true
                });

                CustomKeyBindSystem.RegisterKeyBind<PressKeyBind>(new BuiltinKey
                {
                    key = new CombineKey((int) KeyCode.T, CombineKey.SHIFT_COMB, ECombineKeyAction.OnceClick, false),
                    conflictGroup = 2052,
                    name = "MirrorLatAxis",
                    canOverride = true
                });
                
            }

            if (factoryUndo.Value)
            {
                CustomKeyBindSystem.RegisterKeyBind<PressKeyBind>(new BuiltinKey
                {
                    key = new CombineKey((int) KeyCode.Z, CombineKey.CTRL_COMB, ECombineKeyAction.OnceClick, false),
                    conflictGroup = 2052,
                    name = "FactoryUndo",
                    canOverride = true
                });

                CustomKeyBindSystem.RegisterKeyBind<PressKeyBind>(new BuiltinKey
                {
                    key = new CombineKey((int) KeyCode.Z, CombineKey.SHIFT_COMB, ECombineKeyAction.OnceClick, false),
                    conflictGroup = 2052,
                    name = "FactoryRedo",
                    canOverride = true
                });
            }
        }

        private void Update()
        {
            if (!gotPluginInfo)
            {
                if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(FREE_FOUNDATIONS_GUID))
                {
                    freeFoundationsIsInstalled = true;
                }
                if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(FREE_FOUNDATIONS_GUID_2))
                {
                    freeFoundationsIsInstalled = true;
                }
            
                if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(GENESIS_BOOK_MODGUID))
                {
                    genesisBookIsInstalled = true;
                }

                gotPluginInfo = true;
            }
            
            if (!GameMain.isRunning) return;
            if (GameMain.localPlanet == null) return;

            if (factoryUndo.Value)
            {
                if (CustomKeyBindSystem.GetKeyBind("FactoryUndo").keyValue)
                {
                   // PrintKeyDebugInfo();
                    UndoManager.TryUndo();
                }

                if (CustomKeyBindSystem.GetKeyBind("FactoryRedo").keyValue)
                {
                   // PrintKeyDebugInfo();
                    UndoManager.TryRedo();
                }
            }

            if (cameraToggleEnabled.Value && CustomKeyBindSystem.GetKeyBind("ToggleBPGodModeDesc").keyValue)
            {
                CameraFixPatch.mode = !CameraFixPatch.mode;
            }

            if (axisLockEnabled.Value && CustomKeyBindSystem.GetKeyBind("LockLongAxis").keyValue)
            {
                GridSnappingPatches.LockLongitude();
            }
            
            if (axisLockEnabled.Value &&  CustomKeyBindSystem.GetKeyBind("LockLatAxis").keyValue)
            {
                GridSnappingPatches.LockLatitude();
            }
            
            if (gridControlFeature.Value && CustomKeyBindSystem.GetKeyBind("SetLocalOffset").keyValue)
            {
                GridSnappingPatches.SetOffset();
            }

            if (forcePasteEnabled.Value)
            {
                BlueprintPastePatch.isEnabled = CustomKeyBindSystem.GetKeyBind("ForceBPPlace").keyValue;
            }
            
            if (blueprintMirroring.Value && CustomKeyBindSystem.GetKeyBind("MirrorLongAxis").keyValue)
            {
                BlueprintUtilsPatch2.mirrorLong = !BlueprintUtilsPatch2.mirrorLong;
                BlueprintUtilsPatch2.UpdateBlueprintDisplay();
            }
            
            if (blueprintMirroring.Value && CustomKeyBindSystem.GetKeyBind("MirrorLatAxis").keyValue)
            {
                BlueprintUtilsPatch2.mirrorLat = !BlueprintUtilsPatch2.mirrorLat;
                BlueprintUtilsPatch2.UpdateBlueprintDisplay();
            }
            
        }

        private void PrintKeyDebugInfo()
        {
           HashSet<KeyCode> keysToCheck = new HashSet<KeyCode>((KeyCode[])Enum.GetValues(typeof(KeyCode)));
           string keysDown = keysToCheck.Select(code =>
           {
               bool isPressed = Input.GetKeyDown(code);
               if (isPressed)
               {
                   return code.ToString();
               }

               return "";
           }).Where(s => !string.IsNullOrEmpty(s)).Join(null, " ");
           
           string keysHeld = keysToCheck.Select(code =>
           {
               bool isPressed = Input.GetKey(code);
               if (isPressed)
               {
                   return code.ToString();
               }

               return "";
           }).Where(s => !string.IsNullOrEmpty(s)).Join(null, " ");
           
           logger.LogInfo($"Key Debug Info: down: {keysDown}, pressed: {keysHeld}");
        }

        public bool CheckVersion(string hostVersion, string clientVersion)
        {
            return hostVersion.Equals(clientVersion);
        }

        public string Version => VERSION;
    }
}