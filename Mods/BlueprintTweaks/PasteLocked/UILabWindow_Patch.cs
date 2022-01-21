using CommonAPI;
using HarmonyLib;

namespace BlueprintTweaks.PasteLocked
{
    [RegisterPatch(BlueprintTweaksPlugin.PASTE_LOCKED)]
    public static class UILabWindow_Patch
    {
        [HarmonyPatch(typeof(UILabWindow), "_OnUpdate")]
        [HarmonyPostfix]
        public static void OnUIUpdate(UILabWindow __instance)
        {
            if (__instance.labId == 0 || __instance.factory == null)
            {
                return;
            }
            LabComponent labComponent = __instance.factorySystem.labPool[__instance.labId];

            if (labComponent.recipeIsLocked)
            {
                __instance.stateText.text = "recipeLockedWarn".Translate();
                __instance.stateText.color = __instance.workStoppedColor;
            }
        }
    }
}