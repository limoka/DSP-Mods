using HarmonyLib;

namespace xiaoye97.Patches
{
    [HarmonyPatch]
    public static class UITip_Patch
    {
        /// <summary>
        /// Display ID at item prompt
        /// </summary>
        [HarmonyPostfix, HarmonyPatch(typeof(UIItemTip), "SetTip")]
        private static void ItemTipPatch(UIItemTip __instance, int itemId)
        {
            if (LDBToolPlugin.ShowProto.Value)
            {
                __instance.nameText.text += $" {itemId}";
                LDBToolPlugin.lastTip = __instance;
            }
        }
    }
}