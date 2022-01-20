using System;
using HarmonyLib;

namespace xiaoye97.UI
{
    public static class SupportsHelper
    {
        private static readonly Type _unityExplorerType;
        
        public static bool UnityExplorerInstalled => _unityExplorerType != null;

        static SupportsHelper()
        {
            _unityExplorerType = AccessTools.TypeByName("UnityExplorer.InspectorManager");
        }
    }

    public interface ISkin
    {
        UnityEngine.GUISkin GetSkin();
    }
}
