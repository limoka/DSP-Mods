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
    [CommonAPISubmoduleDependency(nameof(ProtoRegistry), nameof(CustomKeyBindSystem), nameof(PickerExtensionsSystem))]
    public class BlueprintTweaksPlugin : BaseUnityPlugin, IMultiplayerMod
    {
        public const string MODNAME = "BlueprintTweaks";
        
        public const string MODGUID = "org.kremnev8.plugin.BlueprintTweaks";
        
        public const string MOD_DISP_NAME = "Blueprint Tweaks";
        
        public const string VERSION = "1.5.8";

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

        private void Awake()
        {
            logger = Logger;

            #region Config

            cameraToggleEnabled = Config.Bind("Interface", "cameraToggle", true, "Allows toggling camera between 3rd person and god view\nAll values are applied on restart");
            addPasteButtonEnabled = Config.Bind("Interface", "addBluprintPasteButton", true, "If enabled new button will be added to Blueprint Browser. Pressing it will paste curretly selected blueprint\nAll values are applied on restart");
            
            recipeChangeEnabled = Config.Bind("Interface", "recipeChange", true, "Add recipe change panel to blueprint inspectors\nAll values are applied on restart");
            logisticCargoChangeEnabled = Config.Bind("Interface", "changeLogisticCargo", true, "Allows changing cargo requested/provided by logistic stations");
            changeTierEnabled = Config.Bind("Interface", "changeTier", true, "Allows using change tier functionality\nAll values are applied on restart");
            beltHintsChangeEnable = Config.Bind("Interface", "beltHintChange", true, "Add belt hint change panel to blueprint inspectors\nAll values are applied on restart");
            keepBlueprintDesc = Config.Bind("Interface", "keepBlueprintDescription", true, "When pasting blueprint string into existing blueprint you can hold shift to keep description and icons");
            keepBrowserPath = Config.Bind("Interface", "keepBroserPath", true, "Preserve last open Blueprint Browser directory. Also when creating new blueprints, they will be saved in the last open directory");

            
            
            forcePasteEnabled = Config.Bind("Features", "forcePaste", true, "Allow to force paste using Shift+Click");
            axisLockEnabled = Config.Bind("Features", "axisLock", true, "Allows using Latitude/Longtitude axis locks\nAll values are applied on restart");
            gridControlFeature = Config.Bind("Features", "gridControl", true, "Allows changing grid size and its offset\nAll values are applied on restart");
            blueprintMirroring = Config.Bind("Features", "blueprintMirroring", true, "Allows mirroring Blueprints\nAll values are applied on restart");
            dragRemove = Config.Bind("Features", "dragRemove", true, "Allows using drag remove function\nAll values are applied on restart");
            pasteLocked = Config.Bind("Features", "PasteLockedRecipes", true, "Allow pasting assemblers with recipes which have not been unlocked yet. Assemblers with recipes that are not unlocked will not work.");
            moveWithDragNDrop = Config.Bind("Features", "moveBPWithDragNDrop", true, "Allow moving blueprints using drag and drop");
            factoryUndo = Config.Bind("Features", "factoryUndo", true, "Enable Factory Undo feature. Allows to undo/redo most building actions. Will force dragRemove to true");

            
            blueprintFoundations = Config.Bind("Features", "blueprintFoundations", true, "Allow blueprinting foundations along with buildings.\nAll values are applied on restart");
            
            resetFunctionsOnMenuExit = Config.Bind("Misc", "resetOnExit", true, "If enabled when you exit build mode, some functions (Axis/Grid lock, Mirror) will reset their state");
            canBlueprintOnGasGiants = Config.Bind("Misc", "bpOnGasGiants", true, "Allow using Blueprints on Gas Giants\nAll values are applied on restart");

            useFastDismantle = Config.Bind("Misc", "useFastDismantle", true, "When using drag remove tool or factory undo, an improved algorithm of removing entities will be used. It is about 20x faster, but might have some imperfections. If you encounter issues you can switch back to vanilla code.");
            excludeStations = Config.Bind("Misc", "excludeStations", true, "When using drag remove tool, logistic stations (and miners Mk.II) will not get removed. This is a safeguard against errors which occur most of the time when you try to mass dismantle logistic stations.");

            undoMaxHistory = Config.Bind("Misc", "undoMaxHistory", 50, "Defines undo history size. When history reaches it's capacity, old entries will get removed. When using Nebula host controls the used value");
            undoExcludeStations = Config.Bind("Misc", "undoExcludeStations", true, "When enabled factory undo will not undo/redo actions with logistic stations.");

            
            Config.MigrateConfig<bool>("General", "Interface", new []{"cameraToggle", "recipeChange", "changeLogisticCargo", "changeTier"});
            Config.MigrateConfig<bool>("General", "Features", new []{"forcePaste", "axisLock", "gridControl", "gridControl", "blueprintFoundations"});
            Config.MigrateConfig<bool>("General", "Misc", new []{"bpOnGasGiants"});

            if (factoryUndo.Value)
            {
                dragRemove.Value = true;
            }
            
            #endregion
            
            Config.Save();

            string pluginfolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            resource = new ResourceData(MODNAME, "blueprinttweaks", pluginfolder);
            resource.LoadAssetBundle("blueprinttweaks");
            ProtoRegistry.AddResource(resource);
            
            Harmony harmony = new Harmony(MODGUID);

            #region Strings

            ProtoRegistry.RegisterString("KEYToggleBPGodModeDesc", "Toggle Blueprint God Mode", "切换上帝模式浏览蓝图");
            ProtoRegistry.RegisterString("RecipesLabel", "Recipes", "配方");
            ProtoRegistry.RegisterString("ChangeTipText", "Left-click to change recipe", "左键点击更改配方");
            
            ProtoRegistry.RegisterString("ChangeTipTitle", "Change Recipe", "更改配方");
            ProtoRegistry.RegisterString("ChangeTipDesc", 
                "Left-click to change recipe. When you click, picker menu will open, where a new recipe can be selected. All machines that used the old recipe will now use selected recipe. This change will take effect after saving.", 
                "左键点击更改配方。点击将打开选择菜单，可在其中选择新配方。所有使用旧配方的机器将更新到选定的新配方。此更改将在保存后生效。");
            ProtoRegistry.RegisterString("KEYForceBPPlace", "Force Blueprint placement", "强制蓝图放置");
            
            ProtoRegistry.RegisterString("KEYLockLongAxis", "Lock Longitude axis", "经度锁定");
            ProtoRegistry.RegisterString("KEYLockLatAxis", "Lock Latitude axis", "纬度锁定");
            ProtoRegistry.RegisterString("KEYSetLocalOffset", "Set grid snapping offset", "设定网格捕捉偏移");
            
            ProtoRegistry.RegisterString("GridSizeLabel", "Blueprint Size and Anchor", "蓝图大小和锚点");
            ProtoRegistry.RegisterString("GridLongSize", "Width", "宽度");
            ProtoRegistry.RegisterString("GridLatSize", "Height", "高度");
            
            ProtoRegistry.RegisterString("CantPasteThisInGasGiantWarn", 
                "This Blueprint can't be pasted on a Gas Giant.", 
                "此蓝图无法放置在气态/冰巨星上。");
            
            ProtoRegistry.RegisterString("FoundationsLabel", "Foundations", "地基");
            ProtoRegistry.RegisterString("foundationsBPCountLabel", "recorded", "块地基");
            ProtoRegistry.RegisterString("foundationBPEnabledLabel", "Blueprint foundations", "蓝图包含地基");
            
            ProtoRegistry.RegisterString("TransportLabel", "Logistics", "物流");
            ProtoRegistry.RegisterString("ChangeTipText2", "Left-click to change requested item", "左键点击更改物流清单物品");
            ProtoRegistry.RegisterString("ChangeTip2Title", "Change requested items", "更改物流清单物品");
            ProtoRegistry.RegisterString("ChangeTip2Desc", 
                "Left-click to change requested item. When you click, picker menu will open, where a new item can be selected. Logistic station that used the old item will now use selected item. This change will take effect after saving.", 
                "左键点击更改物流清单物品。点击将打开选择菜单，可在其中选择新的物流清单物品。使用旧物流清单物品的物流塔将更新到选定的新物流清单物品。此更改将在保存后生效。");

            
            ProtoRegistry.RegisterString("copyColorsLabel", "Copy Custom foundation colors", "附带自定义调色板");
            ProtoRegistry.RegisterString("copyColorsTip", "Copy Custom foundation colors", "附带自定义调色板");
            ProtoRegistry.RegisterString("copyColorsTipDesc", 
                "When enabled, Custom foundation colors will be saved with Blueprint Data. When such Blueprint will be pasted, current planet's Custom colors will be replaced with colors stored in the Blueprint",
                "启用后，地基的自定义调色板将与蓝图数据一同保存。粘贴此类蓝图时，当前行星的地基自定义调色板将被蓝图中的调色板替代。");
            
            ProtoRegistry.RegisterString("hasColorsLabel", "Contains Color data", "含有颜色数据");
            
            ProtoRegistry.RegisterString("foundationsBlueprintTip", "Blueprint Foundations", "蓝图包含地基");
            ProtoRegistry.RegisterString("foundationsBlueprintTipDesc", 
                "When enabled, all Foundations (Including their colors and types) in your selection will be saved to the Blueprint. If there are buildings that lack support, but blueprint has foundations under them they will successfully be pasted",
                "启用后，您选中的所有地基（包括它们的颜色和类型）都将保存到蓝图中。另外只要蓝图中的建筑下方包含地基，即便施工场地缺乏地基支撑，蓝图也能成功粘贴");

            ProtoRegistry.RegisterString("KEYMirrorLongAxis", "Mirror Blueprint in Longitude axis", "经向镜像");
            ProtoRegistry.RegisterString("KEYMirrorLatAxis", "Mirror Blueprint in Latitude axis", "纬向镜像");
            
            
            ProtoRegistry.RegisterString("BeltHintsLabel", "Belt Hints", "腰带提示");
            ProtoRegistry.RegisterString("HintsChangeTipText", "Left-click to change belt hints", "左键单击更改腰带提示");
            
            ProtoRegistry.RegisterString("ChangeHintsTipTitle", "You can change belt hints", "您可以更改腰带提示");
            ProtoRegistry.RegisterString("ChangeHintsTipDesc", 
                "Left-click to change hints on belts. When you click, picker menu will open, where a new icon can be selected. All belts that used the old icon will now use selected icon. This change will take effect after saving.", 
                "左键点击更改腰带提示。点击将打开选择菜单，可在其中选择新腰带提示。所有使用旧腰带提示的输送带将更新到选定的新腰带提示。此更改将在保存后生效。");
            
            ProtoRegistry.RegisterString("recipeLockedWarn", "Recipe is locked", "食谱已锁定");
            
            ProtoRegistry.RegisterString("BPBrowserPasteButtonTipTitle", "Paste Blueprint [Double click]", "粘贴蓝图[双击]");
            ProtoRegistry.RegisterString("BPBrowserPasteButtonTipDesc", "Start pasting current selected blueprint", "开始张贴当前选定的蓝图");
            
            ProtoRegistry.RegisterString("MoveBlueprintTip", "Move to", "移动到");
            
            ProtoRegistry.RegisterString("FileAlreadyExistsTitle", "Can't move blueprint!", "动不了蓝图！");
            ProtoRegistry.RegisterString("FileAlreadyExistsDesc", 
                "Blueprint with same name already exists in target location! Please rename your blueprint and try again.",
                "同名蓝图已存在于目标位置！ 请重命名蓝图并重试。");

            ProtoRegistry.RegisterString("KEYFactoryUndo", "Undo", "撤消");
            ProtoRegistry.RegisterString("KEYFactoryRedo", "Redo", "重做");
            
            ProtoRegistry.RegisterString("KEYDSPTrashButton", "Select Trash", "选择垃圾");

            ProtoRegistry.RegisterString("NotEnoughFoundationsMessage", 
                "You need {0} more foundations to place this Blueprint!",
                "你需要 {0} 更多的基础来放置这个蓝图！");
            
            ProtoRegistry.RegisterString("FoundCountMessage", 
                "Will consume {0} foundations",
                "会消耗 {0} 基础");
            
            
            ProtoRegistry.RegisterString("UndoSuccessText", "Undone successfully!", "撤消成功！");
            ProtoRegistry.RegisterString("UndoFailureText", "Failed to undo!", "未能撤消！");
            ProtoRegistry.RegisterString("RedoSuccessText", "Redone successfully!", "重做成功！");
            ProtoRegistry.RegisterString("RedoFailureText", "Failed to redo!", "重做失败！");
            
            ProtoRegistry.RegisterString("UndoClearedMessage", "Undo history cleared!", "撤消历史清除！");
            
            ProtoRegistry.RegisterString("UndoHistoryEmptyMessage", "Undo history is empty!", "撤消历史是空的！");
            ProtoRegistry.RegisterString("RedoHistoryEmptyMessage", "Redo history is empty!", "重做历史是空的！");
            
            ProtoRegistry.RegisterString("AnchorSetLabel", "Anchors", "锚赂");
            ProtoRegistry.RegisterString("AnchorTipTitle", "Set your anchor position", "设置锚点位置");
            ProtoRegistry.RegisterString("AnchorTipText", 
                "Use buttons below to set your anchor position as you like. This change will take effect after saving.",
                "使用下面的按钮来设置你喜欢的锚点位置. 此更改将在保存后生效。");
            
            #endregion
            
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

                CustomKeyBindSystem.RegisterKeyBind<PressKeyBind>(new BuiltinKey
                {
                    key = new CombineKey((int) KeyCode.Z, 0, ECombineKeyAction.OnceClick, false),
                    conflictGroup = 2052,
                    name = "DSPTrashButton",
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