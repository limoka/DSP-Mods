using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace BlueprintTweaks
{
    public class CustomWindowData
    {
        public UIRecipesPanel recipesPanel;
        public UIBlueprintSizePanel sizePanel;
        
        public RectTransform mainPane;
    }
    
    [HarmonyPatch]
    public static class UIBlueprintInspectorPatch
    {

        public static Dictionary<UIBlueprintInspector, CustomWindowData> panels = new Dictionary<UIBlueprintInspector, CustomWindowData>();

        [HarmonyPatch(typeof(UIBlueprintBrowser), "_OnCreate")]
        [HarmonyPostfix]
        public static void OnCreateBrowser(UIBlueprintBrowser __instance)
        {
            RectTransform mainTrs = (RectTransform) __instance.transform;
            mainTrs.sizeDelta = new Vector2(mainTrs.sizeDelta.x + 22, mainTrs.sizeDelta.y);
            RectTransform groupBg = (RectTransform)mainTrs.Find("inspector-group-bg");
            groupBg.sizeDelta = new Vector2(groupBg.sizeDelta.x + 22, groupBg.sizeDelta.y);
        }

        [HarmonyPatch(typeof(UIBlueprintInspector), "_OnCreate")]
        [HarmonyPostfix]
        public static void OnCreateInspector(UIBlueprintInspector __instance)
        {
            if (panels != null && !panels.ContainsKey(__instance))
            {
                GameObject scrollPrefab = BlueprintTweaksPlugin.resource.bundle.LoadAsset<GameObject>("assets/blueprinttweaks/ui/bp-panel-scroll.prefab");
                GameObject scroll = Object.Instantiate(scrollPrefab, __instance.rectTrans, false);
                GameObject contentPane = scroll.transform.Find("Viewport/pane").gameObject;

                __instance.group1.SetParent(contentPane.transform, false);
                __instance.group2.SetParent(contentPane.transform, false);
                __instance.group3.SetParent(contentPane.transform, false);

                RectTransform mainTrs = __instance.rectTrans;
                mainTrs.sizeDelta = new Vector2(mainTrs.sizeDelta.x + 16, mainTrs.sizeDelta.y);

                GameObject recipePanelPrefab = BlueprintTweaksPlugin.resource.bundle.LoadAsset<GameObject>("assets/blueprinttweaks/ui/recipe-panel.prefab");
                GameObject recipePanelGO = Object.Instantiate(recipePanelPrefab, contentPane.transform, false);
                UIRecipesPanel recipesPanel = recipePanelGO.GetComponent<UIRecipesPanel>();
                recipesPanel.Create(__instance);
                recipePanelGO.SetActive(BlueprintTweaksPlugin.recipeChangeEnabled);
                
                GameObject sizePanelPrefab = BlueprintTweaksPlugin.resource.bundle.LoadAsset<GameObject>("assets/blueprinttweaks/ui/grid-size-panel.prefab");
                GameObject sizePanelGO = Object.Instantiate(sizePanelPrefab, contentPane.transform, false);
                UIBlueprintSizePanel sizePanel = sizePanelGO.GetComponent<UIBlueprintSizePanel>();
                sizePanel.Create(__instance);
                sizePanelGO.SetActive(BlueprintTweaksPlugin.gridControlFeature);
                
                __instance.group1.transform.SetAsLastSibling();
                
                CustomWindowData data = new CustomWindowData
                {
                    recipesPanel = recipesPanel, 
                    sizePanel = sizePanel,
                    mainPane = (RectTransform)contentPane.transform
                };

                panels.Add(__instance, data);
            }
        }
        
        [HarmonyPatch(typeof(UIBlueprintInspector), "Refresh")]
        [HarmonyPostfix]
        public static void OnRefresh(UIBlueprintInspector __instance,  bool refreshComponent)
        {
            if (panels != null && panels.ContainsKey(__instance))
            {
                UIRecipesPanel recipesPanel = panels[__instance].recipesPanel;
                UIBlueprintSizePanel sizePanel = panels[__instance].sizePanel;
                RectTransform mainPane = panels[__instance].mainPane;
                
                sizePanel.UpdateFields(__instance.blueprint);
                if (refreshComponent)
                {
                    recipesPanel.RefreshUI();
                }

                int textHeight = Mathf.RoundToInt((__instance.descTextInput.preferredHeight - 0.1f) / 2f) * 2;
                if (textHeight < 38)
                {
                    textHeight = 38;
                }
                int textHeightOffs = textHeight + 136;
                int componentSize = 22 + ((__instance.componentCount - 1) / 8 + 1) * 46;
                if (__instance.componentCount == 0)
                {
                    componentSize = 22;
                }
                
                int padding = (__instance.usage == UIBlueprintInspector.EUsage.Browser) ? 18 : 24;
                
                int recipePanelSize = 22 + ((recipesPanel.recipeCount - 1) / 8 + 1) * 46;
                if (recipesPanel.recipeCount == 0)
                {
                    recipePanelSize = 22;
                }
                
                int lastPos = textHeightOffs + padding;

                if (BlueprintTweaksPlugin.gridControlFeature)
                {
                    sizePanel.panelTrs.anchoredPosition = new Vector2(sizePanel.panelTrs.anchoredPosition.x, -lastPos);
                    lastPos += 69 + padding;
                }

                __instance.group2.anchoredPosition = new Vector2(__instance.group2.anchoredPosition.x, -lastPos);
                lastPos += componentSize + padding;


                if (BlueprintTweaksPlugin.recipeChangeEnabled)
                {
                    recipesPanel.panelTrs.sizeDelta = new Vector2(recipesPanel.panelTrs.sizeDelta.x, recipePanelSize);
                    recipesPanel.panelTrs.anchoredPosition = new Vector2(recipesPanel.panelTrs.anchoredPosition.x, -lastPos);
                    lastPos += recipePanelSize + padding;
                }

                __instance.group3.anchoredPosition = new Vector2(__instance.group3.anchoredPosition.x, -lastPos);
                lastPos += 110;

                mainPane.sizeDelta = new Vector2(mainPane.sizeDelta.x, lastPos);

            }
        }
    }
}