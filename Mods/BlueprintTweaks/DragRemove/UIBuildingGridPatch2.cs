using HarmonyLib;
using UnityEngine;

namespace BlueprintTweaks
{
    [RegisterPatch(BlueprintTweaksPlugin.DRAG_REMOVE)]
    public static class UIBuildingGridPatch2
    {
        private static readonly int cursorGratBox = Shader.PropertyToID("_CursorGratBox");
        private static readonly int selectColor = Shader.PropertyToID("_SelectColor");
        private static readonly int tintColor = Shader.PropertyToID("_TintColor");
        private static readonly int showDivideLine = Shader.PropertyToID("_ShowDivideLine");

        [HarmonyPatch(typeof(UIBuildingGrid), "Update")]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPostfix]
        public static void UpdateGrid(UIBuildingGrid __instance)
        {
            Player mainPlayer = GameMain.mainPlayer;

            PlanetFactory planetFactory = GameMain.localPlanet?.factory;
            if (planetFactory == null) return;
            
            if (GameMain.localPlanet.type == EPlanetType.Gas) return;

            PlayerAction_Build actionBuild = mainPlayer?.controller.actionBuild;
            if (actionBuild == null) return;
            
            if (actionBuild.blueprintMode != EBlueprintMode.None) return;
            
            if (!BlueprintTweaksPlugin.tool.active) return;
            if (BlueprintTweaksPlugin.tool.isSelecting)
            {
                __instance.blueprintMaterial.SetColor(tintColor, Color.clear);
                __instance.blueprintMaterial.SetVector(cursorGratBox, (Vector4) BlueprintTweaksPlugin.tool.preSelectGratBox);
                __instance.blueprintMaterial.SetVector(selectColor, __instance.dismantleColor);
                __instance.blueprintMaterial.SetFloat(showDivideLine, 0f);
                __instance.blueprintGridRnd.enabled = true;
            }
            else
            {
                __instance.blueprintMaterial.SetColor(tintColor, __instance.blueprintColor);
                __instance.blueprintMaterial.SetVector(cursorGratBox, Vector4.zero);
                __instance.blueprintMaterial.SetVector(selectColor, Vector4.one);
                __instance.blueprintMaterial.SetFloat(showDivideLine, 0f);
                __instance.blueprintGridRnd.enabled = false;
            }
            
            for (int l = 0; l < 64; l++)
            {
                __instance.blueprintMaterial.SetVector($"_CursorGratBox{l}", Vector4.zero);
                __instance.blueprintMaterial.SetFloat($"_CursorGratBoxInfo{l}", 0f);
            }
        }
    }
}