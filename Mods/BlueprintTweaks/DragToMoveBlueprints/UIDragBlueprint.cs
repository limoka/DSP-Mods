using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace BlueprintTweaks.DragToMoveBlueprints
{
    public class UIDragBlueprint : MonoBehaviour
    {
        public RectTransform feedbackTrans;
        public Text feedbackText;
        
        private RectTransform rtrans;
        private UIBlueprintFileItem internalFile;
        private string lastPath;
        
        public UIBlueprintBrowser browser;
        public UIBlueprintFileItem displayFile;

        public bool isActive;

        private void Awake()
        {
            browser = UIRoot.instance.uiGame.blueprintBrowser;
            rtrans = (RectTransform)transform;
            feedbackTrans.gameObject.SetActive(false);
            SetActive(false);
        }

        private void Update()
        {
            if (GameMain.mainPlayer == null) return;
            if (displayFile == null || !isActive) return;

            if (!Input.GetMouseButton(0))
            {
                UIBlueprintBrowser_Patch.CheckDragEnd(browser);
                SetActive(false);
                return;
            }
            
            if (UIRoot.ScreenPointIntoRect(Input.mousePosition, (RectTransform) rtrans.parent, out Vector2 vector))
            {
                vector.x = Mathf.Round(vector.x);
                vector.y = Mathf.Round(vector.y);
                rtrans.anchoredPosition = vector;
                PlayerController controller = GameMain.mainPlayer.controller;
                if (controller.cmd.type == ECommand.Build && (controller.cmd.state > 0 || controller.cmd.mode == 3))
                {
                    SetActive(false);
                }
            }

            string fullPath = UIBlueprintBrowser_Patch.GetMoveTarget(browser);
            if (fullPath.Equals(""))
            {
                feedbackTrans.gameObject.SetActive(false);
            }
            else if (!fullPath.Equals(lastPath))
            {
                feedbackTrans.gameObject.SetActive(true);
                feedbackText.text = $"{"MoveBlueprintTip".Translate()} {GetRelativePath(fullPath)}";
                feedbackTrans.sizeDelta = new Vector2(feedbackText.preferredWidth, 30);
            }
            
            lastPath = fullPath;
        }
        
        public static string GetRelativePath(string fullpath)
        {
            string rootPath = GameConfig.blueprintFolder.SlashDirectory();
            int rootPathLen = rootPath.Length;
            fullpath = fullpath.SlashDirectory();
            if (fullpath.Equals(rootPath, StringComparison.OrdinalIgnoreCase)) return "/";
            if (fullpath.Length <= rootPathLen) return "";

            string text = fullpath.Substring(0, rootPathLen);
            string text2 = fullpath.Substring(rootPathLen, fullpath.Length - rootPathLen);

            if (!text.Equals(rootPath, StringComparison.OrdinalIgnoreCase)) return "";
            if (Directory.Exists(fullpath)) return text2;

            return "";
        }

        public void SetActive(bool active)
        {
            isActive = active;
            gameObject.SetActive(active);
            if (displayFile != null)
            {
                displayFile.gameObject.SetActive(active);
            }
            if (internalFile != null)
            {
                internalFile.gameObject.SetActive(!active && internalFile.inited);
            }
            if (browser != null)
            {
                browser.browserScroll.vertical = !active;
            }
            
        }

        public void SetHand(UIBlueprintFileItem newItem)
        {
            if (newItem == null || newItem.fullPath.Equals(""))
            {
                SetActive(false);
                return;
            }

            if (displayFile == null)
            {
                displayFile = Instantiate(browser.fileItemPrefab, transform);
                displayFile._Create();
                displayFile._Init(null);
                displayFile._OnUnregEvent();
                ((RectTransform) displayFile.gameObject.transform).sizeDelta *= 0.8f;
                displayFile.GetComponent<UIButton>().enabled = false;
            }
            
            displayFile.SetItemLayout(0, newItem.isDirectory, newItem.fullPath, newItem.shortName);
            displayFile.rectTrans.anchoredPosition = Vector2.zero;
            internalFile = newItem;

            if (UIRoot.ScreenPointIntoRect(Input.mousePosition, (RectTransform) rtrans.parent, out Vector2 vector))
            {
                vector.x = Mathf.Round(vector.x);
                vector.y = Mathf.Round(vector.y);
                rtrans.anchoredPosition = vector;
            }

            SetActive(true);
        }
        
    }
}