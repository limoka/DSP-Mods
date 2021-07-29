using System;
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
    [BepInPlugin(MODGUID, MOD_DISP_NAME, VERSION)]
    [BepInDependency(NEBULA_MODID, BepInDependency.DependencyFlags.SoftDependency)]
    public class BlueprintTweaksPlugin : BaseUnityPlugin
    {
        public const string MODNAME = "BlueprintTweaks";
        
        public const string MODGUID = "org.kremnev8.plugin.BlueprintTweaks";
        
        public const string MOD_DISP_NAME = "Blueprint Tweaks";
        
        public const string VERSION = "1.0.7";

        public const string NEBULA_MODID = "dsp.nebula-multiplayer";

        public static ManualLogSource logger;
        public static ResourceData resource;

        public static bool cameraToggleEnabled;
        public static bool recipeChangeEnabled;
        public static bool forcePasteEnabled;
        public static bool axisLockEnabled;
        public static bool changeTierEnabled;
        public static bool canBlueprintOnGasGiants;
        public static bool gridControlFeature;

        public static bool nebulaInstalled;

        private void Awake()
        {
            logger = Logger;

            cameraToggleEnabled = Config.Bind("General", "cameraToggle", true, "Allow toggling camera between 3rd person and god view\nAll values are applied on restart").Value;
            recipeChangeEnabled = Config.Bind("General", "recipeChange", true, "Add recipe change panel to blueprint inspectors\nAll values are applied on restart").Value;
            forcePasteEnabled = Config.Bind("General", "forcePaste", true, "Allow using key to force blueprint placement\nAll values are applied on restart").Value;
            axisLockEnabled = Config.Bind("General", "axisLock", true, "Allow using Latitude/Longtitude axis locks\nAll values are applied on restart").Value;
            changeTierEnabled = Config.Bind("General", "changeTier", true, "Allow using change tier functionality\nAll values are applied on restart").Value;
            canBlueprintOnGasGiants = Config.Bind("General", "bpOnGasGiants", true, "Allow using Blueprints on Gas Giants\nAll values are applied on restart").Value;
            gridControlFeature = Config.Bind("General", "gridControl", true, "Allow changing grid size and its offset\nAll values are applied on restart").Value;

            
            string pluginfolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            resource = new ResourceData(MODNAME, "blueprinttweaks", pluginfolder);
            resource.LoadAssetBundle("blueprinttweaks");
            ProtoRegistry.AddResource(resource);
            
            ProtoRegistry.RegisterString("KEYToggleBPGodModeDesc", "Toggle Blueprint God Mode");
            ProtoRegistry.RegisterString("RecipesLabel", "Recipes");
            ProtoRegistry.RegisterString("ChangeTipText", "Left-click to change recipe");
            
            ProtoRegistry.RegisterString("ChangeTipTitle", "Change Recipe");
            ProtoRegistry.RegisterString("ChangeTipDesc", "Left-click to change recipe. When you click, picker menu will open, where a new recipe can be selected. All machines that used the old recipe will now use the new recipe. This change will take effect after saving.");
            ProtoRegistry.RegisterString("KEYForceBPPlace", "Force Blueprint placement");
            
            ProtoRegistry.RegisterString("KEYLockLongAxis", "Lock Longtitude axis");
            ProtoRegistry.RegisterString("KEYLockLatAxis", "Lock Latitude axis");
            ProtoRegistry.RegisterString("KEYSetLocalOffset", "Set grid snapping offset");
            
            ProtoRegistry.RegisterString("GridSizeLabel", "Blueprint Size");
            ProtoRegistry.RegisterString("GridLongSize", "Width");
            ProtoRegistry.RegisterString("GridLatSize", "Height");
            
            ProtoRegistry.RegisterString("CantPasteThisInGasGiantWarn", "This Blueprint can't be pasted on a Gas Giant.");
            
            KeyBindPatch.Init();

            foreach (var pluginInfo in BepInEx.Bootstrap.Chainloader.PluginInfos)
            {
                if (pluginInfo.Value.Metadata.GUID != NEBULA_MODID) continue;

                nebulaInstalled = true;
                break;
            }
            
            Harmony harmony = new Harmony(MODGUID);
            
            harmony.PatchAll(typeof(KeyBindPatch));
            harmony.PatchAll(typeof(UIItemPickerPatch));
            
            if (changeTierEnabled)
                harmony.PatchAll(typeof(UIBlueprintComponentItemPatch));
            if (canBlueprintOnGasGiants)
                harmony.PatchAll(typeof(PlayerControllerPatch));
            if (axisLockEnabled || gridControlFeature)
                harmony.PatchAll(typeof(GridSnappingPatches));
            if (cameraToggleEnabled)
                harmony.PatchAll(typeof(CameraFixPatch));
            if (recipeChangeEnabled || gridControlFeature)
                harmony.PatchAll(typeof(UIBlueprintInspectorPatch));
            if (forcePasteEnabled)
                harmony.PatchAll(typeof(BlueprintPastePatch));

            logger.LogInfo("Blueprint tweaks mod is initialized!");
        }

        private void Update()
        {
            if (cameraToggleEnabled && KeyBindPatch.GetKeyBind("ToggleBPGodModeDesc").keyValue)
            {
                CameraFixPatch.mode = !CameraFixPatch.mode;
            }

            if (axisLockEnabled && KeyBindPatch.GetKeyBind("LockLongAxis").keyValue)
            {
                GridSnappingPatches.LockLongtitude();
            }
            
            if (axisLockEnabled &&  KeyBindPatch.GetKeyBind("LockLatAxis").keyValue)
            {
                GridSnappingPatches.LockLatitude();
            }
            
            if (gridControlFeature && KeyBindPatch.GetKeyBind("SetLocalOffset").keyValue)
            {
                GridSnappingPatches.SetOffset();
            }

            if (forcePasteEnabled)
            {
                BlueprintPastePatch.isEnabled = KeyBindPatch.GetKeyBind("ForceBPPlace").keyValue;
            }
        }
    }
}