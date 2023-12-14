using CommonAPI.Systems;
using HarmonyLib;
using UnityEngine;

namespace BlueprintTweaks.KeepBPName
{
    [HarmonyPatch]
    public static class UIBlueprintInspectorPatch
    {
        public static string shortDesc;
        public static string desc;
        public static EIconLayout layout;
        public static int icon0;
        public static int icon1;
        public static int icon2;
        public static int icon3;
        public static int icon4;
        

        [HarmonyPatch(typeof(UIBlueprintInspector), "OnPasteClick")]
        [HarmonyPrefix]
        public static void OnPasteClickPrefix(UIBlueprintInspector __instance, out bool __state)
        {
            string systemCopyBuffer = GUIUtility.systemCopyBuffer;
            if (__instance.blueprint == null || string.IsNullOrEmpty(systemCopyBuffer))
            {
                __state = false;
                return;
            }
            
            __state = CustomKeyBindSystem.GetKeyBind("ForceBPPlace").keyValue;
            shortDesc = __instance.blueprint.shortDesc;
            desc = __instance.blueprint.desc;
            layout = __instance.blueprint.layout;
            icon0 = __instance.blueprint.icon0;
            icon1 = __instance.blueprint.icon1;
            icon2 = __instance.blueprint.icon2;
            icon3 = __instance.blueprint.icon3;
            icon4 = __instance.blueprint.icon4;
        }

        [HarmonyPatch(typeof(UIBlueprintInspector), "OnPasteClick")]
        [HarmonyPostfix]
        public static void OnPasteClickPostfix(UIBlueprintInspector __instance, bool __state)
        {
            if (__state)
            {
                __instance.blueprint.shortDesc = shortDesc;
                __instance.blueprint.desc = desc;
                __instance.blueprint.layout = layout;
                __instance.blueprint.icon0 = icon0;
                __instance.blueprint.icon1 = icon1;
                __instance.blueprint.icon2 = icon2;
                __instance.blueprint.icon3 = icon3;
                __instance.blueprint.icon4 = icon4;
                __instance.Refresh(false, true, true);
            }
        }
    }
}