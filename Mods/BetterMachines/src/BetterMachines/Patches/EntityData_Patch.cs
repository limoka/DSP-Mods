using HarmonyLib;

namespace FasterMachines
{
    [HarmonyPatch]
    public static class EntityData_Patch
    {
        public static int updateCounter = 0;

        [HarmonyPatch(typeof(EntityData), "Import")]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        public static void Postfix(ref EntityData __instance)
        {
            int protoID = __instance.protoId;
            int modelIndex = __instance.modelIndex;

            foreach (ItemProto item in BetterMachinesPlugin.bmItems)
            {
                if (protoID == item.ID)
                {
                    if (modelIndex == item.ModelIndex) return;

                    __instance.modelIndex = (short)item.ModelIndex;
                    updateCounter++;
                }
            }

            if (protoID == BetterMachinesPlugin.oldChemPlantID.Value)
            {
                __instance.protoId = 2317;
                __instance.modelIndex = 376;
                updateCounter++;
            }
        }

        internal static string GetFixMessage()
        {
            if (updateCounter <= 0) return "";

            return string.Format(("BMModModificationWarn").Translate(), updateCounter);
        }
    }
}