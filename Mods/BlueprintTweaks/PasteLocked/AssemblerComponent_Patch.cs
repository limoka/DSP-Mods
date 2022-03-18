using CommonAPI;
using HarmonyLib;

namespace BlueprintTweaks.PasteLocked
{
    [RegisterPatch(BlueprintTweaksPlugin.PASTE_LOCKED)]
    public static class AssemblerComponent_Patch
    {
        [HarmonyPatch(typeof(AssemblerComponent), "InternalUpdate")]
        [HarmonyPrefix]
        public static bool CheckAssemblerRecipe(ref AssemblerComponent __instance)
        {
            if (!__instance.recipeIsLocked)
            {
                __instance.recipeIsLocked = GameMain.history.RecipeUnlocked(__instance.recipeId);
            }

            return __instance.recipeIsLocked;
        }

        [HarmonyPatch(typeof(AssemblerComponent), "SetRecipe")]
        [HarmonyPostfix]
        public static void OnSetRecipe(ref AssemblerComponent __instance)
        {
            if (__instance.recipeId > 0)
            {
                __instance.recipeIsLocked = GameMain.history.RecipeUnlocked(__instance.recipeId);
            }
        }
    }
}