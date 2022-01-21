using System.IO;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace BlueprintTweaks.BlueprintBrowserUIChanges
{
    [HarmonyPatch]
    public static class UIBlueprintBrowserPatch
    {
        public static UIBlueprintFileItem lastClickedFile;
        
        [HarmonyPatch(typeof(UIBlueprintBrowser), "_OnCreate")]
        [HarmonyPostfix]
        public static void OnCreate(UIBlueprintBrowser __instance)
        {
            //Move original buttons
            __instance.addrTrans.offsetMin = new Vector2(280, -36);
            ((RectTransform) __instance.upLevelButton.transform).anchoredPosition += new Vector2(45, 0);
            ((RectTransform) __instance.newFolderButton.transform).anchoredPosition += new Vector2(45, 0);
            ((RectTransform) __instance.newFileButton.transform).anchoredPosition += new Vector2(45, 0);

            //Create new Button
            GameObject cut = __instance.cutButton.gameObject;
            GameObject paste = Object.Instantiate(cut, cut.transform.parent, true);
            RectTransform pasteTrs = (RectTransform)paste.transform;
            
            //Change its position and size
            pasteTrs.anchoredPosition = new Vector2(82, 2);
            pasteTrs.sizeDelta = new Vector2(40, 40);

            //Change button icon
            RectTransform pasteIconTrs = (RectTransform)pasteTrs.Find("icon");
            Sprite newIcon = BlueprintTweaksPlugin.resource.bundle.LoadAsset<Sprite>("Assets/BlueprintTweaks/Icons/paste-icon.png");
            pasteIconTrs.GetComponent<Image>().sprite = newIcon;

            Button pasteButton =  pasteTrs.GetComponent<Button>();
            pasteButton.onClick.RemoveAllListeners();
            pasteButton.onClick.AddListener(OnPasteButtonClicked);
        }

        [HarmonyPatch(typeof(UIBlueprintFileItem), "OnThisClick")]
        [HarmonyPostfix]
        public static void OnItemClicked(UIBlueprintFileItem __instance)
        {
            lastClickedFile = __instance;
        }

        public static void OnPasteButtonClicked()
        {
            if (lastClickedFile == null) return;
            
            if (lastClickedFile.isDirectory)
            {
                if (!Directory.Exists(lastClickedFile.fullPath)) return;
                
                lastClickedFile.browser.SetCurrentDirectory(lastClickedFile.fullPath);
                return;
            }

            if (!VFInput.readyToBuild) return;
            BlueprintData blueprint = BlueprintData.CreateFromFile(lastClickedFile.fullPath);
            if (BlueprintData.IsNullOrEmpty(blueprint)) return;
            
            GameMain.mainPlayer.controller.OpenBlueprintPasteMode(blueprint, lastClickedFile.fullPath);
            lastClickedFile.browser._Close();
        }
    }
}