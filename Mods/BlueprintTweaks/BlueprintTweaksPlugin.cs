using System;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Logging;
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
        
        public const string VERSION = "1.0.0";

        public static ManualLogSource logger;

        private void Awake()
        {
            logger = Logger;
            
            Harmony harmony = new Harmony(MAINGUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            
            logger.LogInfo("Blueprint tweaks mod is initialized!");
        }

        private void Update()
        {
            if (KeyBindPatch.toggleBpMode)
            {
                CameraFixPatch.mode = !CameraFixPatch.mode;
            }
        }
    }
}