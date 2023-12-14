using System.Collections.Generic;
using CommonAPI;
using HarmonyLib;
using NebulaAPI;

namespace BlueprintTweaks.FactoryUndo
{
    [RegisterPatch(BlueprintTweaksPlugin.FACTORY_UNDO)]
    public static class BuildTool_Upgrade_Patch
    {
        internal static HashSet<int> upgradeIds = new HashSet<int>();
        internal static List<UpgradeData> upgradeList = new List<UpgradeData>();

        internal static bool RecordUpgrades;
        
        [HarmonyPatch(typeof(BuildTool_Upgrade), nameof(BuildTool_Upgrade.UpgradeAction))]
        [HarmonyPrefix]
        public static void OnUpgradePre(BuildTool_Upgrade __instance)
        {
            if (UndoManager.IgnoreAllEvents.Value) return;
            
            if (NebulaModAPI.IsMultiplayerActive)
            {
                if (NebulaModAPI.MultiplayerSession.LocalPlayer.IsClient) return;
            }

            upgradeIds.Clear();
            upgradeList.Clear();
            RecordUpgrades = true;
        }

        [HarmonyPatch(typeof(BuildTool_Upgrade), nameof(BuildTool_Upgrade.UpgradeAction))]
        [HarmonyPostfix]
        public static void OnUpgradePost(BuildTool_Upgrade __instance)
        {
            if (UndoManager.IgnoreAllEvents.Value) return;
            
            if (NebulaModAPI.IsMultiplayerActive)
            {
                if (NebulaModAPI.MultiplayerSession.LocalPlayer.IsClient) return;
            }

            RecordUpgrades = false;

            if (upgradeList.Count > 0)
            {
                PlayerUndo data = UndoManager.GetCurrentPlayerData();
                data.AddUndoItem(new UndoUpgrade(data, upgradeList));
            }
        }
        
        [HarmonyPatch(typeof(PlayerAction_Build), nameof(PlayerAction_Build.DoUpgradeObject))]
        [HarmonyPrefix]
        public static void OnDoUpgragePre(PlayerAction_Build __instance, int objId, int grade, int upgrade)
        {
            if (UndoManager.IgnoreAllEvents.Value) return;
            if (NebulaModAPI.IsMultiplayerActive)
            {
                if (NebulaModAPI.MultiplayerSession.LocalPlayer.IsClient) return;
            }
            
            if (!RecordUpgrades) return;
            if (objId == 0) return;

            if (upgradeIds.Contains(objId)) return;

            upgradeIds.Add(objId);
            ItemProto itemProto = __instance.noneTool.GetItemProto(objId);
            if (itemProto != null && itemProto.canUpgrade)
            {
                int oldGrade = itemProto.Grade;
                ItemProto newItemProto = itemProto;

                if (grade > 0)
                {
                    newItemProto = itemProto.GetGradeItem(grade);
                }
                else if (upgrade != 0)
                {
                    newItemProto = itemProto.GetUpgradeItem(upgrade);
                }

                upgradeList.Add(new UpgradeData(objId, oldGrade, newItemProto.Grade));
            }
        }

        [HarmonyPatch(typeof(PlayerAction_Build), nameof(PlayerAction_Build.DoUpgradeObject))]
        [HarmonyPostfix]
        public static void OnDoUpgragePost(PlayerAction_Build __instance, int objId, bool __result)
        {
            if (NebulaModAPI.IsMultiplayerActive)
            {
                if (NebulaModAPI.MultiplayerSession.LocalPlayer.IsClient) return;
            }
            
            if (!__result)
            {
                upgradeList.RemoveAll(data => data.objId == objId);
            }
        }
    }
}