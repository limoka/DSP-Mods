using CommonAPI;
using HarmonyLib;

namespace BlueprintTweaks.PasteLocked
{
    [RegisterPatch(BlueprintTweaksPlugin.PASTE_LOCKED)]
    public static class UIAssemblerWindow_Patch
    {
        
        [HarmonyPatch(typeof(UIAssemblerWindow), "_OnUpdate")]
        [HarmonyPostfix]
        public static void OnUIUpdate(UIAssemblerWindow __instance)
        {
            if (__instance.assemblerId == 0 || __instance.factory == null)
            {
                return;
            }
            AssemblerComponent assemblerComponent = __instance.factorySystem.assemblerPool[__instance.assemblerId];

            if (assemblerComponent.recipeIsLocked)
            {
                __instance.stateText.text = "recipeLockedWarn".Translate();
                __instance.stateText.color = __instance.workStoppedColor;
            }
        }
    }
}