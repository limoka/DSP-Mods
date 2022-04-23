using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace GigaStations
{
    [HarmonyPatch]
    public static class SaveFixPatch
    {

        public static int updateCounter = 0;

        [HarmonyPatch(typeof(EntityData), "Import")]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        public static void Postfix(ref EntityData __instance)
        {
            int protoID = __instance.protoId;
            int modelIndex = __instance.modelIndex;

            if (protoID == GigaStationsPlugin.pls.ID)
            {
                if (modelIndex == GigaStationsPlugin.plsModel.ID) return;
                
                __instance.modelIndex = (short) GigaStationsPlugin.plsModel.ID;
                updateCounter++;
            }else if (protoID == GigaStationsPlugin.ils.ID)
            {
                if (modelIndex == GigaStationsPlugin.ilsModel.ID) return;
                
                __instance.modelIndex = (short) GigaStationsPlugin.ilsModel.ID;
                updateCounter++;
            }else if (protoID == GigaStationsPlugin.collector.ID)
            {
                if (modelIndex == GigaStationsPlugin.collectorModel.ID) return;
                
                __instance.modelIndex = (short) GigaStationsPlugin.collectorModel.ID;
                updateCounter++;
            }
        }

        internal static string GetFixMessage()
        {
            if (updateCounter <= 0) return "";
            
            return string.Format(("ModificationWarn").Translate(), SaveFixPatch.updateCounter);
        }
    }
}