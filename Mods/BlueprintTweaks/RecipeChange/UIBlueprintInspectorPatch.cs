using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace BlueprintTweaks
{
    [HarmonyPatch]
    public static class UIBlueprintInspectorPatch
    {

        public static Dictionary<UIBlueprintInspector, UIRecipesPanel> panels = new Dictionary<UIBlueprintInspector, UIRecipesPanel>();

        [HarmonyPatch(typeof(UIBlueprintInspector), "_OnCreate")]
        [HarmonyPostfix]
        public static void OnCreate(UIBlueprintInspector __instance)
        {
            if (panels != null && !panels.ContainsKey(__instance))
            {
                GameObject prefab = BlueprintTweaksPlugin.resource.bundle.LoadAsset<GameObject>("assets/blueprinttweaks/ui/recipe-panel.prefab");
                GameObject obj = Object.Instantiate(prefab, __instance.transform, false);
                UIRecipesPanel panel = obj.GetComponent<UIRecipesPanel>();
                panel.Create(__instance);
                panels.Add(__instance, panel);
            }
        }
        
        [HarmonyPatch(typeof(UIBlueprintInspector), "Refresh")]
        [HarmonyPostfix]
        public static void OnRefresh(UIBlueprintInspector __instance,  bool refreshComponent)
        {
            if (panels != null && panels.ContainsKey(__instance) && refreshComponent)
            {
                UIRecipesPanel panel = panels[__instance];
                panel.RefreshUI();
                
                int textHeight = Mathf.RoundToInt((__instance.descTextInput.preferredHeight - 0.1f) / 2f) * 2;
                if (textHeight < 38)
                {
                    textHeight = 38;
                }
                int textHeightOffs = textHeight + 136;
                int componentSize = 22 + ((__instance.componentCount - 1) / 8 + 1) * 46;
                int num7 = (__instance.usage == UIBlueprintInspector.EUsage.Browser) ? 18 : 24;

                int recipePanelSize = 22 + ((panel.recipeCount - 1) / 8 + 1) * 46;
                if (panel.recipeCount == 0)
                {
                    recipePanelSize = 22;
                }

                panel.panelTrs.sizeDelta = new Vector2(panel.panelTrs.sizeDelta.x, recipePanelSize);
                panel.panelTrs.anchoredPosition = new Vector2(panel.panelTrs.anchoredPosition.x, -textHeightOffs - componentSize - 2 * num7 );
                
                __instance.group3.anchoredPosition = new Vector2(__instance.group3.anchoredPosition.x, -textHeightOffs - componentSize - recipePanelSize - 3 * num7);
            }
        }
    }
}