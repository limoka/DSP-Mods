using CommonAPI;
using HarmonyLib;
using UnityEngine;

namespace BlueprintTweaks
{
    [RegisterPatch(BlueprintTweaksPlugin.DRAG_REMOVE)]
    public static class UIBuildingGridPatch2
    {
        private static readonly int cursorGratBox = Shader.PropertyToID("_CursorGratBox");
        private static readonly int zMin = Shader.PropertyToID("_ZMin");
        private static readonly int tintColor = Shader.PropertyToID("_TintColor");
        private static readonly int reformMode = Shader.PropertyToID("_ReformMode");

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
                __instance.material.SetColor(tintColor, __instance.dismantleColor);
                __instance.material.SetFloat(reformMode, 0f);
                __instance.material.SetFloat(zMin, -0.5f);
                __instance.material.SetVector(cursorGratBox, (Vector4) BlueprintTweaksPlugin.tool.selectGratBox);
            }
        }
    }
}