using System;
using System.IO;
using HarmonyLib;

namespace crecheng.DSPModSave.Patches
{
    [HarmonyPatch]
    public class GameSave_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameSave), "SaveCurrentGame")]
        public static void SaveCurrentGame(bool __result, string saveName)
        {
            if (!__result) return;

            DSPModSavePlugin.OnSave(saveName);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameSave), "AutoSave")]
        public static void AutoSave(bool __result)
        {
            if (!__result) return;

            DSPModSavePlugin.OnAutoSave();
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameSave), "LoadCurrentGame")]
        public static void PreLoadCurrentGame(bool __result, string saveName)
        {
            DSPModSavePlugin.OnPreLoad(saveName);
        }   
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameSave), "LoadCurrentGame")]
        public static void PostLoadCurrentGame(bool __result)
        {
            if (!__result) return;
            
            DSPModSavePlugin.OnPostLoad();
        } 
    }
}