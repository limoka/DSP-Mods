using HarmonyLib;
using UnityEngine;

namespace BlueprintTweaks
{
    [RegisterPatch(BlueprintTweaksPlugin.DRAG_REMOVE)]
    public static class UIBuildMenuPatch
    {
        public static UIButton dragSelectButton;
        public static UIBuildMenu menu;
        
        [HarmonyPatch(typeof(UIBuildMenu), "_OnCreate")]
        [HarmonyPostfix]
        public static void AddUIButton(UIBuildMenu __instance)
        {
            menu = __instance;
            GameObject buttonPrefab = BlueprintTweaksPlugin.resource.bundle.LoadAsset<GameObject>("Assets/BlueprintTweaks/UI/drag-remove-button.prefab");
            GameObject button = Object.Instantiate(buttonPrefab, __instance.uxGroup.transform, false);
            ((RectTransform) button.transform).anchoredPosition = new Vector2(-78, 11);
            button.SetActive(false);
            dragSelectButton = button.GetComponent<UIButton>();

            dragSelectButton.onClick += OnDragSelectClick;
        }

        [HarmonyPatch(typeof(UIBuildMenu), "UpdateUXPanel")]
        [HarmonyPostfix]
        public static void UpdateUI(UIBuildMenu __instance)
        {
            dragSelectButton.gameObject.SetActive(__instance.isDismantleMode);
            dragSelectButton.highlighted = __instance.dismantleTool.cursorType == 2;
        }
        
        [HarmonyPatch(typeof(UIBuildMenu), "UpdateUXPanelHotKey")]
        [HarmonyPostfix]
        public static void UpdateHotKeys(UIBuildMenu __instance)
        {
            if (!__instance.isDismantleMode) return;
            
            if (Input.GetKeyDown(KeyCode.F3))
            {
                OnDragSelectClick(0);
            }
        }
        
        private static void OnDragSelectClick(int obj)
        {
            if (menu.isDismantleMode)
            {
                menu.dismantleTool.cursorType = 2;
            }

            menu.UpdateUXPanel();
        }
    }
}