using System;
using HarmonyLib;

namespace GigaStations
{
    [HarmonyPatch]
    public static class UIEntityBriefInfo_Patch
    {
        [HarmonyPatch(typeof(UIEntityBriefInfo), nameof(UIEntityBriefInfo._OnCreate))]
        [HarmonyPrefix]
        public static void ResizeArray(UIEntityBriefInfo __instance)
        {
            int needSize = Math.Max(GigaStationsPlugin.ilsMaxSlots, GigaStationsPlugin.plsMaxSlots);
            if (needSize > __instance.icons.Length)
            {
                Array.Resize(ref __instance.icons, needSize);
            }
        }
    }
}