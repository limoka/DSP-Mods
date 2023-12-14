using HarmonyLib;

namespace FasterMachines
{
    [HarmonyPatch]
    public static class BlueprintData_Patch
    {

        [HarmonyPatch(typeof(BlueprintData), nameof(BlueprintData.Import))]
        [HarmonyPostfix]
        public static void FixBlueprint(BlueprintData __instance)
        {
            foreach (BlueprintBuilding building in __instance.buildings)
            {
                foreach (ItemProto item in BetterMachinesPlugin.bmItems)
                {
                    if (building.itemId == item.ID)
                    {
                        if (building.modelIndex == item.ModelIndex) return;

                        building.modelIndex = (short)item.ModelIndex;
                    }
                }

                if (building.itemId == BetterMachinesPlugin.oldChemPlantID.Value)
                {
                    building.itemId = 2317;
                    building.modelIndex = 376;
                }
            }
        }
    }
}