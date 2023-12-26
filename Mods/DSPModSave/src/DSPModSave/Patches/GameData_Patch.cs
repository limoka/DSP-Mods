using HarmonyLib;

namespace crecheng.DSPModSave.Patches
{
    [HarmonyPatch]
    public class GameData_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameData), "NewGame")]
        static void EnterGame()
        {
            DSPModSavePlugin.logger.LogInfo("Enter New Game");
            foreach (var d in DSPModSavePlugin.allModData.Values)
            {
                d.mod.IntoOtherSave();
            }
        }
    }
}