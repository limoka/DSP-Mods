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
    }

    [HarmonyPatch]
    public static class MessagePatch
    {
        public delegate void RefAction<T1>(ref T1 arg1);
        
        [HarmonyPatch(typeof(GameLoader), "FixedUpdate")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AddModificationWarn(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldloc_0),
                    new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(string), nameof(string.IsNullOrEmpty))),
                    new CodeMatch(OpCodes.Brtrue)
                )
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, 0))
                .InsertAndAdvance(
                    Transpilers.EmitDelegate<RefAction<string>>((ref string text) =>
                    {
                        if (SaveFixPatch.updateCounter > 0)
                            text = text + "\r\n" + string.Format(("ModificationWarn").Translate(), SaveFixPatch.updateCounter);
                    }));


            return matcher.InstructionEnumeration();
        }
    }
}