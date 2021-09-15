using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace BlueprintTweaks
{
    public class CustomWindowData
    {
        public UIBlueprintPanel[] panels;

        public RectTransform mainPane;
    }

    [HarmonyPatch]
    public static class UIBlueprintInspectorPatch
    {
        public static Dictionary<UIBlueprintInspector, CustomWindowData> panelData = new Dictionary<UIBlueprintInspector, CustomWindowData>();

        public static List<string> panelPrefabs = new List<string>();

        public static void Init()
        {
            if (BlueprintTweaksPlugin.gridControlFeature.Value) 
                panelPrefabs.Add("assets/blueprinttweaks/ui/grid-size-panel.prefab");
            
            if (BlueprintTweaksPlugin.blueprintFoundations.Value)
                panelPrefabs.Add("assets/blueprinttweaks/ui/bp-foundations-panel.prefab");
            
            panelPrefabs.Add("assets/blueprinttweaks/ui/component-panel.prefab");
            
            if (BlueprintTweaksPlugin.recipeChangeEnabled.Value) 
                panelPrefabs.Add("assets/blueprinttweaks/ui/recipe-panel.prefab");
            
            if (BlueprintTweaksPlugin.logisticCargoChangeEnabled.Value)
                panelPrefabs.Add("Assets/BlueprintTweaks/UI/transport-panel.prefab");
            
            panelPrefabs.Add("assets/blueprinttweaks/ui/string-copy-panel.prefab");
        }
        
        
        [HarmonyPatch(typeof(UIBlueprintBrowser), "_OnCreate")]
        [HarmonyPostfix]
        public static void OnCreateBrowser(UIBlueprintBrowser __instance)
        {
            RectTransform mainTrs = (RectTransform) __instance.transform;
            mainTrs.sizeDelta = new Vector2(mainTrs.sizeDelta.x + 22, mainTrs.sizeDelta.y);
            RectTransform groupBg = (RectTransform) mainTrs.Find("inspector-group-bg");
            groupBg.sizeDelta = new Vector2(groupBg.sizeDelta.x + 22, groupBg.sizeDelta.y);
        }

        [HarmonyPatch(typeof(UIBlueprintInspector), "_OnCreate")]
        [HarmonyPostfix]
        public static void OnCreateInspector(UIBlueprintInspector __instance)
        {
            if (panelData != null && !panelData.ContainsKey(__instance))
            {
                GameObject scrollPrefab = BlueprintTweaksPlugin.resource.bundle.LoadAsset<GameObject>("assets/blueprinttweaks/ui/bp-panel-scroll.prefab");
                GameObject scroll = Object.Instantiate(scrollPrefab, __instance.rectTrans, false);
                GameObject contentPane = scroll.transform.Find("Viewport/pane").gameObject;

                __instance.group1.SetParent(contentPane.transform, false);
                __instance.group2.SetParent(contentPane.transform, false);
                __instance.group3.SetParent(contentPane.transform, false);

                RectTransform mainTrs = __instance.rectTrans;
                mainTrs.sizeDelta = new Vector2(mainTrs.sizeDelta.x + 16, mainTrs.sizeDelta.y);

                UIBlueprintPanel[] panelsList = new UIBlueprintPanel[panelPrefabs.Count];

                for (int i = 0; i < panelPrefabs.Count; i++)
                {
                    GameObject panelPrefab = BlueprintTweaksPlugin.resource.bundle.LoadAsset<GameObject>(panelPrefabs[i]);
                    GameObject panelGO = Object.Instantiate(panelPrefab, contentPane.transform, false);
                    UIBlueprintPanel panel = panelGO.GetComponent<UIBlueprintPanel>();
                    panel.Create(__instance);
                    panelsList[i] = panel;
                }

                __instance.group1.transform.SetAsLastSibling();

                CustomWindowData data = new CustomWindowData
                {
                    panels = panelsList,
                    mainPane = (RectTransform) contentPane.transform,
                };

                panelData.Add(__instance, data);
            }
        }

        [HarmonyPatch(typeof(UIBlueprintInspector), "_OnOpen")]
        [HarmonyPostfix]
        public static void OnOpen(UIBlueprintInspector __instance)
        {
            if (panelData != null && panelData.ContainsKey(__instance))
            {
                UIBlueprintPanel[] panels = panelData[__instance].panels;
                foreach (UIBlueprintPanel panel in panels)
                {
                    panel.OnOpen();
                }
            }
        }

        [HarmonyPatch(typeof(UIBlueprintInspector), "Refresh")]
        [HarmonyPostfix]
        public static void OnRefresh(UIBlueprintInspector __instance, bool refreshComponent)
        {
            if (panelData != null && panelData.ContainsKey(__instance))
            {
                UIBlueprintPanel[] panels = panelData[__instance].panels;
                RectTransform mainPane = panelData[__instance].mainPane;

                int textHeight = Mathf.RoundToInt((__instance.descTextInput.preferredHeight - 0.1f) / 2f) * 2;
                if (textHeight < 38)
                {
                    textHeight = 38;
                }

                int textHeightOffs = textHeight + 136;
                int padding = (__instance.usage == UIBlueprintInspector.EUsage.Browser) ? 18 : 24;

                int lastPos = textHeightOffs + padding;

                foreach (UIBlueprintPanel panel in panels)
                {
                    panel.RefreshUI();
                    int size = panel.verticalSize;

                    panel.panelTrs.anchoredPosition = new Vector2(panel.panelTrs.anchoredPosition.x, -lastPos);
                    lastPos += size + padding;
                }

                mainPane.sizeDelta = new Vector2(mainPane.sizeDelta.x, lastPos);
            }
        }
    }
}