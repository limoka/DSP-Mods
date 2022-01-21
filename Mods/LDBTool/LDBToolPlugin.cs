using System.Collections.Generic;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using xiaoye97.UI;

[module: UnverifiableCode]
#pragma warning disable 618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore 618

namespace xiaoye97
{
    [BepInPlugin(MODGUID, MODNAME, VERSION)]
    public class LDBToolPlugin : BaseUnityPlugin
    {
        public const string MODNAME = "LDBTool";
        public const string MODGUID = "me.xiaoye97.plugin.Dyson.LDBTool";
        public const string VERSION = "2.0.3";
        
        internal static ManualLogSource logger;
        internal static ConfigEntry<bool> ShowProto;
        internal static ConfigEntry<KeyCode> ShowProtoHotKey, ShowItemProtoHotKey, ShowRecipeProtoHotKey;
        
        internal static UIItemTip lastTip;
        
        void Awake()
        {
            logger = Logger;
            
            ProtoIndex.InitIndex();
            LDBTool.Init();
            
            ShowProto = Config.Bind("config", "ShowProto", false, "是否开启数据显示");
            ShowProtoHotKey = Config.Bind("config", "ShowProtoHotKey", KeyCode.F5, "呼出界面的快捷键");
            ShowItemProtoHotKey = Config.Bind("config", "ShowItemProtoHotKey", KeyCode.I, "显示物品的Proto");
            ShowRecipeProtoHotKey = Config.Bind("config", "ShowRecipeProtoHotKey", KeyCode.R, "显示配方的Proto");

            Harmony harmony = new Harmony(MODGUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            
            logger.LogInfo("LDBTool is loaded successfully!");
        }


        void Update()
        {
            if (ShowProto.Value)
            {
                if (Input.GetKeyDown(ShowProtoHotKey.Value))
                {
                    ProtoDataUI.Show = !ProtoDataUI.Show;
                }

                if (SupportsHelper.UnityExplorerInstalled)
                {
                    if (Input.GetKeyDown(ShowItemProtoHotKey.Value))
                    {
                        TryShowItemProto();
                    }

                    if (Input.GetKeyDown(ShowRecipeProtoHotKey.Value))
                    {
                        TryShowRecipeProto();
                    }
                }
            }
        }

        void OnGUI()
        {
            if (ShowProto.Value && ProtoDataUI.Show)
            {
                ProtoDataUI.OnGUI();
            }
        }
        
        /// <summary>
        /// 尝试显示ItemProto，通过按键触发
        /// </summary>
        internal static void TryShowItemProto()
        {
            if (ShowProto.Value)
            {
                if (lastTip != null && lastTip.showingItemId != 0)
                {
                    var proto = LDB.items.Select(lastTip.showingItemId);
                    if (proto != null)
                    {
                        RUEHelper.ShowProto(proto);
                    }
                    else
                    {
                        var recipe = LDB.recipes.Select(-lastTip.showingItemId);
                        if (recipe != null)
                        {
                            foreach (var id in recipe.Results)
                            {
                                var item = LDB.items.Select(id);
                                RUEHelper.ShowProto(item);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 尝试显示RecipeProto，通过按键触发
        /// </summary>
        internal static void TryShowRecipeProto()
        {
            if (ShowProto.Value)
            {
                if (lastTip != null && lastTip.showingItemId != 0)
                {
                    var itemProto = LDB.items.Select(lastTip.showingItemId);
                    if (itemProto != null)
                    {
                        foreach (var proto in itemProto.recipes)
                        {
                            RUEHelper.ShowProto(proto);
                        }
                    }
                    else
                    {
                        var proto = LDB.recipes.Select(-lastTip.showingItemId);
                        RUEHelper.ShowProto(proto);
                    }
                }
            }
        }
    }
}