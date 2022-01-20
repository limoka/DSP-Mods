using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace xiaoye97.UI
{
    /// <summary>
    /// 用来显示Proto数据，方便开发
    /// </summary>
    public static class ProtoDataUI
    {
        public static bool Show;
        private static Rect winRect = new Rect(0, 0, 500, 800);
        private static int selectIndex;
        private static int SelectIndex
        {
            get => selectIndex;
            set
            {
                if (selectIndex != value)
                {
                    selectIndex = value;
                    ProtoSetEx.needSearch = true;
                }
            }
        }

        private static string[] protoTypeNames = ProtoIndex.GetProtoNames();
        public static ISkin Skin;

        public static void OnGUI()
        {
            if (Skin != null) GUI.skin = Skin.GetSkin();
            winRect = GUILayout.Window(3562532, winRect, WindowFunc, "ProtoData");
        }

        public static void WindowFunc(int id)
        {
            if (Skin != null) GUI.skin = Skin.GetSkin();
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal(GUI.skin.box);
            SelectIndex = GUILayout.SelectionGrid(SelectIndex, protoTypeNames, 10);
            GUILayout.Space(20);
            if (GUILayout.Button("Close", GUILayout.Width(80)))
            {
                Show = false;
            }
            GUILayout.EndHorizontal();

            Type selectedType = ProtoIndex.GetProtoTypeAt(selectIndex);
            
            PropertyInfo protoProperty = typeof(LDB).GetProperties().First(property =>
            {
                Type setType = typeof(ProtoSet<>).MakeGenericType(selectedType);
                return setType.IsAssignableFrom(property.PropertyType);
            });
            
            object protoSet = protoProperty.GetValue(null);
            MethodInfo method = typeof(ProtoSetEx).GetMethod(nameof(ProtoSetEx.ShowSet), AccessTools.all).MakeGenericMethod(selectedType);
            method.Invoke(null, new[] {protoSet});

            GUILayout.EndVertical();
            GUI.DragWindow();
        }
    }

    public static class ProtoSetEx
    {
        private static Vector2 sv;
        private static Dictionary<Type, int> selectPages = new Dictionary<Type, int>();

        static ProtoSetEx()
        {
            foreach (PropertyInfo propertyInfo in typeof(LDB).GetProperties())
            {
                Type setType = propertyInfo.PropertyType;
                if (!setType.IsConstructedGenericType)
                {
                    setType = setType.BaseType;
                }

                Type protoType = setType.GetGenericArguments()[0];
                selectPages.Add(protoType, 0);
                
            }
        }
        
        private static string search = "";
        private static string Search
        {
            get => search;
            set
            {
                if (search != value)
                {
                    search = value;
                    needSearch = true;
                }
            }
        }
        public static bool needSearch = true;
        private static List<Proto> searchResultList = new List<Proto>();
        private static void SearchLDB<T>(ProtoSet<T> protoSet) where T : Proto
        {
            searchResultList.Clear();
            if (protoSet != null)
            {
                foreach (var proto in protoSet.dataArray)
                {
                    if (Search == "" || proto.ID.ToString().Contains(Search) || proto.Name.Contains(Search) || proto.Name.Translate().Contains(Search))
                    {
                        searchResultList.Add(proto);
                    }
                }
            }
            needSearch = false;
        }

        public static void ShowSet<T>(ProtoSet<T> protoSet) where T : Proto
        {
            if (ProtoDataUI.Skin != null) GUI.skin = ProtoDataUI.Skin.GetSkin();
            GUILayout.BeginHorizontal(GUI.skin.box);
            Search = GUILayout.TextField(Search, GUILayout.Width(200));
            if (needSearch)
            {
                SearchLDB(protoSet);
            }
            GUILayout.Label($"Page {selectPages[typeof(T)] + 1} / {searchResultList.Count / 100 + 1}", GUILayout.Width(80));
            if (GUILayout.Button("-", GUILayout.Width(20))) selectPages[typeof(T)]--;
            if (GUILayout.Button("+", GUILayout.Width(20))) selectPages[typeof(T)]++;
            if (selectPages[typeof(T)] < 0) selectPages[typeof(T)] = searchResultList.Count / 100;
            else if (selectPages[typeof(T)] > searchResultList.Count / 100) selectPages[typeof(T)] = 0;
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"index", GUILayout.Width(40));
            GUILayout.Label($"ID", GUILayout.Width(40));
            GUILayout.Label($"Name");
            GUILayout.Label($"TranslateName");
            if (SupportsHelper.UnityExplorerInstalled)
            {
                GUILayout.Label($"Show Data", GUILayout.Width(100));
            }
            GUILayout.EndHorizontal();
            sv = GUILayout.BeginScrollView(sv);
            for (int i = selectPages[typeof(T)] * 100; i < Mathf.Min(selectPages[typeof(T)] * 100 + 100, searchResultList.Count); i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{i}", GUILayout.Width(40));
                if (searchResultList[i] != null)
                {
                    GUILayout.Label($"{searchResultList[i].ID}", GUILayout.Width(40));
                    GUILayout.Label($"{searchResultList[i].Name}");
                    GUILayout.Label($"{searchResultList[i].name.Translate()}");
                    if (SupportsHelper.UnityExplorerInstalled)
                    {
                        if (GUILayout.Button($"Show Data", GUILayout.Width(100)))
                        {
                            ShowItem item = new ShowItem(searchResultList[i]);
                            item.Show();
                        }
                    }
                }
                else
                {
                    GUILayout.Label("null");
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
    }
}
