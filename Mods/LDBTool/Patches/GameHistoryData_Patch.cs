using HarmonyLib;

namespace xiaoye97.Patches
{
    [HarmonyPatch]
    public static class GameHistoryData_Patch
    {
        /// <summary>
        /// Fixed an issue where new items were not displayed in the composition menu
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPostfix, HarmonyPatch(typeof(GameHistoryData), "Import")]
        private static void HistoryPatch(GameHistoryData __instance)
        {
            foreach (var proto in LDBTool.TotalDict[ProtoIndex.GetIndex(typeof(RecipeProto))])
            {
                var recipe = proto as RecipeProto;
                if (recipe.preTech != null)
                {
                    if (__instance.TechState(recipe.preTech.ID).unlocked)
                    {
                        if (!__instance.RecipeUnlocked(recipe.ID))
                        {
                            __instance.UnlockRecipe(recipe.ID);
                        }
                    }
                }
            }
        }
    }
}