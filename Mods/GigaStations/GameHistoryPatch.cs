using HarmonyLib;
// ReSharper disable InconsistentNaming

namespace GigaStations
{
    [HarmonyPatch]
    public static class GameHistoryPatch
    {
        [HarmonyPatch(typeof(GameHistoryData), "NotifyTechUnlock")]
        [HarmonyPostfix]
        public static void NotifyTechUnlock(int _techId, int _level)
        {
            StationEditPatch.History_onTechUnlocked(_techId, _level);
        }
    }
}