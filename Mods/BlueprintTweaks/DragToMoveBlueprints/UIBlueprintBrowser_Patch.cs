using System;
using System.IO;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BlueprintTweaks.DragToMoveBlueprints
{
    [HarmonyPatch]
    public static class UIBlueprintBrowser_Patch
    {
        private static int dragThreshold = 4;

        private static Vector2 dragMouseBegin;

        private static RectTransform commonTools;
        private static UIDragBlueprint drag;
        private static Camera canvasCamera;

        [HarmonyPatch(typeof(UIBlueprintBrowser), nameof(UIBlueprintBrowser._OnCreate))]
        [HarmonyPostfix]
        public static void OnCreate()
        {
            commonTools = (RectTransform) UIRoot.instance.uiGame.transform.Find("Common Tools");
            GameObject prefab = BlueprintTweaksPlugin.resource.bundle.LoadAsset<GameObject>("Assets/BlueprintTweaks/UI/Browser/UIDragBlueprint.prefab");
            GameObject inst = Object.Instantiate(prefab, commonTools);
            drag = inst.GetComponent<UIDragBlueprint>();
        }

        [HarmonyPatch(typeof(UIBlueprintBrowser), nameof(UIBlueprintBrowser._OnUpdate))]
        [HarmonyPostfix]
        public static void OnUpdate(UIBlueprintBrowser __instance)
        {
            if (drag.isActive) return;

            if (UIRoot.ScreenPointIntoRect(Input.mousePosition, __instance.contentTrans, out Vector2 rectPoint))
            {
                int x = Mathf.FloorToInt((rectPoint.x - 5) / 110);
                int y = Mathf.FloorToInt((-rectPoint.y - 5) / 120);

                int index = x + y * 8;
                
                if (x < 0 || x >= 8) return;
                if (index < 0 || index >= __instance.fileItems.Count) return;

                if (Input.GetMouseButtonDown(0))
                {
                    dragMouseBegin = Input.mousePosition;
                }

                if (Input.GetMouseButton(0))
                {
                    Vector2 vector = (Vector2) Input.mousePosition - dragMouseBegin;
                    if (Mathf.Abs(vector.x) + Mathf.Abs(vector.y) > dragThreshold)
                    {
                        if (__instance.fileItems[index].inited)
                            drag.SetHand(__instance.fileItems[index]);
                    }
                }
            }
        }

        public static bool MouseInRect(this RectTransform rectTransform)
        {
            if (canvasCamera == null)
            {
                canvasCamera = UIRoot.instance.overlayCanvas.worldCamera;
            }

            Vector2 mousePos = Input.mousePosition;
            return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, mousePos, canvasCamera);
        }

        internal static string GetMoveTarget(UIBlueprintBrowser browser)
        {
            if (drag.displayFile == null || drag.displayFile.fullPath.Equals("")) return "";

            if (UIRoot.ScreenPointIntoRect(Input.mousePosition, browser.contentTrans, out Vector2 vector))
            {
                int x = Mathf.FloorToInt((vector.x - 5) / 110);
                int y = Mathf.FloorToInt((-vector.y - 5) / 120);

                if (x >= 0 && x < 8)
                {
                    int index = x + y * 8;

                    if (index >= 0 && index < browser.fileItems.Count)
                    {
                        UIBlueprintFileItem target = browser.fileItems[index];
                        if (target.isDirectory)
                        {
                            return target.fullPath;
                        }
                    }
                }
            }

            try
            {
                var addrItem = browser.addrItems.First(item => item.rectTrans.MouseInRect());
                return addrItem.fullPath;
            }
            catch (InvalidOperationException) { }

            return "";
        }

        internal static void CheckDragEnd(UIBlueprintBrowser browser)
        {
            string targetPath = GetMoveTarget(browser);
            if (!targetPath.Equals(""))
            {
                MoveToDir(browser, targetPath);
            }
        }

        private static void MoveToDir(UIBlueprintBrowser browser, string targetPath)
        {
            string sourcePath = drag.displayFile.fullPath;
            string sourceFileName = sourcePath.Split('/').Last();
            string destPath = targetPath.SlashDirectory() + sourceFileName;

            if (!drag.displayFile.isDirectory)
            {
                try
                {
                    File.Move(sourcePath, destPath);
                }
                catch (IOException e)
                {
                    bool exists = File.Exists(destPath);
                    if (exists)
                    {
                        UIMessageBox.Show("FileAlreadyExistsTitle".Translate(), "FileAlreadyExistsDesc".Translate(), "确定".Translate(), UIMessageBox.WARNING);
                    }
                    BlueprintTweaksPlugin.logger.LogWarning($"Error moving files: {e.Message}");
                    return;
                }
            }
            else if (drag.displayFile.isDirectory)
            {
                if (targetPath.Equals(drag.displayFile.fullPath)) return;

                try
                {
                    Directory.Move(sourcePath, destPath);
                }
                catch (IOException e)
                {
                    bool exists = Directory.Exists(destPath);
                    if (exists)
                    {
                        UIMessageBox.Show("FileAlreadyExistsTitle".Translate(), "FileAlreadyExistsDesc".Translate(), "确定".Translate(), UIMessageBox.WARNING);
                    }
                    BlueprintTweaksPlugin.logger.LogWarning($"Error moving files: {e.Message}");
                    return;
                }
            }
            browser.SetCurrentDirectory(browser.currentDirectoryInfo.FullName);
        }
    }
}