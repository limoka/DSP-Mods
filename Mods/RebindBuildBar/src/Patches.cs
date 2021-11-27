using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Configuration;
using CommonAPI;
using CommonAPI.Systems;
using HarmonyLib;
using UnityEngine;
using xiaoye97;
using Object = UnityEngine.Object;

namespace RebindBuildBar
{
    [HarmonyPatch]
    public static class Patches
    {
        public static UIBuildMenu buildMenu;
        public static ConfigFile customBarBind = new ConfigFile($"{Paths.ConfigPath}/RebindBuildBar/CustomBarBind.cfg", true);

        private static Vector2 pickerPos = new Vector2(-300, 238);

        public static Color textColor = new Color(0.3821f, 0.8455f, 1f, 0.7843f);
        public static Color lockedTextColor = new Color(0.8f, 0, 0, 0.5f);

        public static Color normalColor = new Color(0.588f, 0.588f, 0.588f, 0.7f);
        public static Color disabledColor = new Color(0.2f, 0.15f, 0.15f, 0.7f);

        public delegate bool RefAction<T1, in T2, in T3>(ref T1 arg1, T2 arg2, T3 arg3);

        private static string _lockedText;

        public static string lockedText
        {
            get
            {
                if (string.IsNullOrEmpty(_lockedText))
                {
                    _lockedText = "LockedTipText".Translate();
                    if (_lockedText.Length < 10)
                    {
                        int len = (10 - _lockedText.Length) / 2;
                        _lockedText += new string(' ', len);
                    }
                }

                return _lockedText;
            }
        }


        [HarmonyPatch(typeof(UIBuildMenu), "_OnCreate")]
        [HarmonyPostfix]
        public static void InitResetButton(UIBuildMenu __instance)
        {
            buildMenu = __instance;

            GameObject buttonPrefab = RebindBuildBarPlugin.resources.bundle.LoadAsset<GameObject>("Assets/RebindBuildBar/UI/buildmenu-button.prefab");
            GameObject buttonGo = Object.Instantiate(buttonPrefab, __instance.childGroup.transform, false);
            ((RectTransform) buttonGo.transform).anchoredPosition = new Vector2(-300, 5);

            UIButton button = buttonGo.GetComponent<UIButton>();
            button.onClick += _ =>
            {
                bool heldCtrl = CustomKeyBindSystem.GetKeyBind("ReassignBuildBar").keyValue;
                string msg = (heldCtrl ? "ResetBuildMenuQuestion1" : "ResetBuildMenuQuestion2").Translate();
                
                UIMessageBox.Show("ResetBuildMenuQuestionTitle".Translate(), msg, "否".Translate(), "是".Translate(),
                    UIMessageBox.QUESTION,
                    null, () =>
                    {
                        ResetBuildBarItems(heldCtrl);
                        buildMenu.SetCurrentCategory(buildMenu.currentCategory);
                        VFAudio.Create("ui-click-0", null, Vector3.zero, true);
                    });
            };
        }


        public static void ResetBuildBarItems(bool heldCtrl)
        {
            UIBuildMenu.staticLoaded = false;
            if (heldCtrl)
            {
                UIBuildMenu.StaticLoad();
                LDBTool.SetBuildBar();

                ResetConfigFile();
                customBarBind.Save();
            }
            else
            {
                ReloadOneCategory(buildMenu.currentCategory);

                ResetConfigFile();
                customBarBind.Save();
            }
        }

        public static void ReloadOneCategory(int category)
        {
            foreach (ItemProto item in LDB.items.dataArray)
            {
                int buildIndex = item.BuildIndex;
                if (buildIndex > 0)
                {
                    int itemCategory = buildIndex / 100;
                    int buttonIndex = buildIndex % 100;
                    if (itemCategory == category && itemCategory <= 15 && buttonIndex <= 12)
                    {
                        UIBuildMenu.protos[itemCategory, buttonIndex] = item;
                    }
                }
            }
            
            foreach (var kv in LDBTool.BuildBarDict)
            {
                foreach (var kv2 in kv.Value)
                {
                    var item = LDB.items.Select(kv2.Value);
                    if (item != null && kv.Key == category)
                    {
                        UIBuildMenu.protos[kv.Key, kv2.Key] = item;
                    }
                }
            }

            UIBuildMenu.staticLoaded = true;
        }

        [HarmonyPatch(typeof(UIBuildMenu), "SetCurrentCategory")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> ShowLockedIcons(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(OpCodes.Callvirt)
                )
                .Advance(-2)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 4))
                .InsertAndAdvance(Transpilers.EmitDelegate<Action<UIBuildMenu, int>>(ButtonLogic));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UIBuildMenu), "_OnUpdate")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> ShowLockedIconsUpdate(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Proto), nameof(Proto.ID))),
                    new CodeMatch(OpCodes.Stloc_S),
                    new CodeMatch(OpCodes.Ldloc_0))
                .MatchForward(false, new CodeMatch(OpCodes.Ldloc_3))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_2))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(Transpilers.EmitDelegate<Func<bool, int, UIBuildMenu, bool>>((result, index, that) =>
                {
                    if (result) return true;
                    if (!GameMain.history.TechUnlocked(1001) || index == 9) return false;

                    that.isAnyCategoryUnlocked = true;
                    return true;
                }));


            matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldc_I4_1)
                )
                .MatchForward(false,
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(GameHistoryData), nameof(GameHistoryData.ItemUnlocked))));

            CodeMatcher matcher2 = matcher.Clone();
            matcher2.MatchForward(false,
                    new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(UIButton), nameof(UIButton.highlighted))))
                .Advance(1);

            Label label = (Label) matcher2.Operand;

            matcher.Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Pop))
                .SetInstruction(new CodeInstruction(OpCodes.Br, label));


            matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Component), "get_gameObject")),
                    new CodeMatch(OpCodes.Ldc_I4_0),
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(GameObject), nameof(GameObject.SetActive)))
                )
                .Advance(5)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 9))
                .InsertAndAdvance(Transpilers.EmitDelegate<Action<UIBuildMenu, int>>(ButtonLogic));

            return matcher.InstructionEnumeration();
        }

        private static void ButtonLogic(UIBuildMenu menu, int i)
        {
            if (UIBuildMenu.protos[menu.currentCategory, i] != null &&
                (UIBuildMenu.protos[menu.currentCategory, i].IsEntity || UIBuildMenu.protos[menu.currentCategory, i].BuildMode == 4))
            {
                int itemId = UIBuildMenu.protos[menu.currentCategory, i].ID;
                if (GameMain.history.ItemUnlocked(itemId))
                {
                    EnableButton(menu, i, itemId);
                }
                else
                {
                    if (menu.currentCategory == 9)
                    {
                        DisableButton(menu, i);
                    }
                    else
                    {
                        GhostButton(menu, i, itemId);
                    }
                }
            }
            else
            {
                if (menu.currentCategory == 9)
                {
                    DisableButton(menu, i);
                }
                else
                {
                    GhostButton(menu, i, 0);
                }
            }
        }

        private static void GhostButton(UIBuildMenu menu, int i, int itemId)
        {
            if (menu.childButtons[i] == null) return;

            if (itemId == 0)
            {
                menu.childIcons[i].enabled = false;
                menu.childButtons[i].tips.itemId = 0;
            }
            else
            {
                menu.childIcons[i].enabled = true;
                menu.childButtons[i].tips.itemId = itemId;
                menu.childButtons[i].tips.corner = 8;
                menu.childButtons[i].tips.delay = 0.2f;
                menu.childNumTexts[i].text = lockedText;
                menu.childNumTexts[i].color = lockedTextColor;
                if (menu.childIcons[i].sprite == null && UIBuildMenu.protos[menu.currentCategory, i] != null)
                {
                    menu.childIcons[i].sprite = UIBuildMenu.protos[menu.currentCategory, i].iconSprite;
                }

                menu.childIcons[i].color = disabledColor;
            }


            menu.childButtons[i].gameObject.SetActive(true);
            menu.childButtons[i].button.interactable = true;
        }

        private static void DisableButton(UIBuildMenu menu, int i)
        {
            if (menu.childButtons[i] == null) return;

            menu.childNumTexts[i].text = "";
            menu.childButtons[i].tips.itemId = 0;
            menu.childButtons[i].button.interactable = false;
            menu.childButtons[i].button.gameObject.SetActive(false);
        }


        private static void EnableButton(UIBuildMenu menu, int i, int itemId)
        {
            if (menu.childButtons[i] == null) return;

            StorageComponent package = menu.player.package;

            menu.childIcons[i].enabled = true;
            menu.childButtons[i].tips.itemId = itemId;
            menu.childButtons[i].tips.corner = 8;
            menu.childButtons[i].tips.delay = 0.2f;
            menu.childButtons[i].button.gameObject.SetActive(true);
            int num3 = package.GetItemCount(itemId);
            bool flag2 = menu.player.inhandItemId == itemId;
            if (flag2)
            {
                num3 += menu.player.inhandItemCount;
            }

            StringBuilderUtility.WriteKMG(menu.strb, 5, (long) num3, false);
            menu.childNumTexts[i].text = ((num3 > 0) ? menu.strb.ToString().Trim() : "");
            menu.childNumTexts[i].color = textColor;
            menu.childButtons[i].button.interactable = true;
            if (menu.childIcons[i].sprite == null && UIBuildMenu.protos[menu.currentCategory, i] != null)
            {
                menu.childIcons[i].sprite = UIBuildMenu.protos[menu.currentCategory, i].iconSprite;
            }

            menu.childIcons[i].color = normalColor;

            menu.childTips[i].color = menu.tipTextColor;
            menu.childButtons[i].highlighted = flag2;
        }


        [HarmonyPatch(typeof(VFPreload), "InvokeOnLoadWorkEnded")]
        [HarmonyPriority(Priority.Low)]
        [HarmonyPostfix]
        public static void LoadBar()
        {
            for (int i = 0; i < UIBuildMenu.protos.GetLength(0); i++)
            for (int j = 0; j < UIBuildMenu.protos.GetLength(1); j++)
            {
                if (i == 0 || j == 0) continue;
                if (i > 8 || j > 10) continue;

                int buildIndex = i * 100 + j;
                ItemProto proto = UIBuildMenu.protos[i, j];

                if (proto != null && proto.ID != 0)
                {
                    ConfigEntry<int> result = customBarBind.Bind("BuildBarBinds",
                        buildIndex.ToString(),
                        proto.ID,
                        $"Item: {proto.Name.Translate()}");
                    
                    if (result.Value == 0)
                    {
                        UIBuildMenu.protos[i, j] = default;
                    }
                    else if (result.Value > 0 && LDB.items.Exist(result.Value) && result.Value != proto.ID)
                    {
                        UIBuildMenu.protos[i, j] = LDB.items.Select(result.Value);
                    }
                }
                else
                {
                    ConfigEntry<int> result = customBarBind.Bind("BuildBarBinds",
                        buildIndex.ToString(),
                        0,
                        "Unused");

                    if (result.Value > 0 && LDB.items.Exist(result.Value))
                    {
                        UIBuildMenu.protos[i, j] = LDB.items.Select(result.Value);
                    }
                }
            }
        }

        private static void ResetConfigFile()
        {
            for (int i = 0; i < UIBuildMenu.protos.GetLength(0); i++)
            for (int j = 0; j < UIBuildMenu.protos.GetLength(1); j++)
            {
                if (i == 0 || j == 0) continue;
                if (i > 8 || j > 10) continue;

                int buildIndex = i * 100 + j;
                ItemProto proto = UIBuildMenu.protos[i, j];

                if (proto != null && proto.ID != 0)
                {
                    ConfigEntry<int> result = customBarBind.Bind("BuildBarBinds",
                        buildIndex.ToString(),
                        proto.ID,
                        $"Item: {proto.Name.Translate()}");

                    result.Value = proto.ID;
                    UIBuildMenu.protos[i, j] = proto;
                }
                else
                {
                    ConfigEntry<int> result = customBarBind.Bind("BuildBarBinds",
                        buildIndex.ToString(),
                        0,
                        "Unused");

                    result.Value = 0;
                    UIBuildMenu.protos[i, j] = default;
                }
            }
        }

        [HarmonyPatch(typeof(UIBuildMenu), "OnChildButtonClick")]
        [HarmonyPrefix]
        public static bool OnChildClick(UIBuildMenu __instance, int index)
        {
            if (!CustomKeyBindSystem.GetKeyBind("ReassignBuildBar").keyValue) return true;
            if (buildMenu.currentCategory < 1 || buildMenu.currentCategory >= 9) return true;

            int buildIndex = buildMenu.currentCategory * 100 + index;


            UIItemPickerExtension.Popup(pickerPos, proto =>
            {
                if (proto != null && proto.ID != 0)
                {
                    ConfigEntry<int> result = customBarBind.Bind("BuildBarBinds",
                        buildIndex.ToString(),
                        proto.ID,
                        $"Item: {proto.Name.Translate()}");
                    result.Value = proto.ID;
                    UIBuildMenu.protos[buildMenu.currentCategory, index] = proto;
                    buildMenu.SetCurrentCategory(buildMenu.currentCategory);
                    VFAudio.Create("ui-click-0", null, Vector3.zero, true);
                }
            }, true, proto => proto.ModelIndex != 0 && proto.CanBuild);
            UIRoot.instance.uiGame.itemPicker.OnTypeButtonClick(2);
            return false;
        }

        public static void Update()
        {
            if (buildMenu == null || !buildMenu.childGroup.gameObject.activeSelf) return;
            if (buildMenu.currentCategory < 1 || buildMenu.currentCategory >= 9) return;

            if (CustomKeyBindSystem.GetKeyBind("ClearBuildBar").keyValue)
            {
                for (int i = 1; i <= 10; i++)
                {
                    if (!buildMenu.childButtons[i].isPointerEnter) continue;

                    int buildIndex = buildMenu.currentCategory * 100 + i;

                    ConfigEntry<int> result = customBarBind.Bind("BuildBarBinds",
                        buildIndex.ToString(),
                        0,
                        "Cleared by player");
                    result.Value = 0;
                    UIBuildMenu.protos[buildMenu.currentCategory, i] = default;
                    buildMenu.SetCurrentCategory(buildMenu.currentCategory);
                    VFAudio.Create("ui-click-0", null, Vector3.zero, true);
                    return;
                }
            }

            if (!CustomKeyBindSystem.GetKeyBind("ReassignBuildBar").keyValue) return;

            for (int j = 1; j <= 10; j++)
            {
                if (Input.GetKeyDown(KeyCode.F1 + (j - 1)) && VFInput.inScreen && !VFInput.inputing)
                {
                    int buildIndex = buildMenu.currentCategory * 100 + j;

                    UIItemPickerExtension.Popup(pickerPos, proto =>
                    {
                        if (proto != null && proto.ID != 0)
                        {
                            ConfigEntry<int> result = customBarBind.Bind("BuildBarBinds",
                                buildIndex.ToString(),
                                proto.ID,
                                $"Item: {proto.Name.Translate()}");
                            result.Value = proto.ID;
                            UIBuildMenu.protos[buildMenu.currentCategory, j] = proto;
                            buildMenu.SetCurrentCategory(buildMenu.currentCategory);
                            VFAudio.Create("ui-click-0", null, Vector3.zero, true);
                        }
                    }, true, proto => proto.ModelIndex != 0 && proto.CanBuild);
                    UIRoot.instance.uiGame.itemPicker.OnTypeButtonClick(2);
                    return;
                }
            }
        }
    }
}