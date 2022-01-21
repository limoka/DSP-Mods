using CommonAPI;
using HarmonyLib;

namespace BlueprintTweaks.PasteLocked
{
    [RegisterPatch(BlueprintTweaksPlugin.PASTE_LOCKED)]
    public static class LabComponent_Patch
    {
        [HarmonyPatch(typeof(LabComponent), "InternalUpdateAssemble")]
        [HarmonyPrefix]
        public static bool CheckAssemblerRecipe(ref LabComponent __instance)
        {
            if (__instance.recipeIsLocked)
            {
                __instance.recipeIsLocked = !GameMain.history.RecipeUnlocked(__instance.recipeId);
            }

            return !__instance.recipeIsLocked;
        }

        [HarmonyPatch(typeof(LabComponent), "SetFunction")]
        [HarmonyPostfix]
        public static void OnSetRecipe(ref LabComponent __instance)
        {
            if (__instance.recipeId > 0)
            {
                __instance.recipeIsLocked = !GameMain.history.RecipeUnlocked(__instance.recipeId);
            }
        }
    }
}