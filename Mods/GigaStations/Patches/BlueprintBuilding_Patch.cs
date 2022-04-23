using HarmonyLib;

namespace GigaStations
{
    [HarmonyPatch]
    public static class BlueprintBuilding_Patch
    {
        [HarmonyPatch(typeof(BlueprintBuilding), nameof(BlueprintBuilding.Import))]
        [HarmonyPostfix]
        public static void UpdateBlueprintData(BlueprintBuilding __instance)
        {
            if (__instance.itemId != GigaStationsPlugin.ils.ID && 
                __instance.itemId != GigaStationsPlugin.pls.ID && 
                __instance.itemId != GigaStationsPlugin.collector.ID) // not my stations
            {
                return;
            }

            ItemProto proto = LDB.items.Select(__instance.itemId);
            __instance.modelIndex = (short)proto.prefabDesc.modelIndex;
        }
    }
}