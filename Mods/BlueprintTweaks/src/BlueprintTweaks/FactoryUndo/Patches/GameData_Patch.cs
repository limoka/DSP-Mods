using BlueprintTweaks.FactoryUndo.Nebula;
using CommonAPI;
using HarmonyLib;
using NebulaAPI;
using UnityEngine;

namespace BlueprintTweaks.FactoryUndo
{
    [RegisterPatch(BlueprintTweaksPlugin.FACTORY_UNDO)]
    public static class GameData_Patch
    {
        [HarmonyPatch(typeof(GameData), nameof(GameData.LeavePlanet))]
        [HarmonyPostfix]
        public static void OnDoUpgragePost()
        {
            if (NebulaModAPI.IsMultiplayerActive)
            {
                IMultiplayerSession session = NebulaModAPI.MultiplayerSession;
                if (session.LocalPlayer.IsClient)
                {
                    session.Network.SendPacket(new ClearUndoRequestPacket(session.LocalPlayer.Id));
                    UIRealtimeTip.Popup("UndoClearedMessage".Translate(), false);
                    VFAudio.Create("cancel-0", null, Vector3.zero, true, 4);
                    return;
                }
            }

            PlayerUndo data = UndoManager.GetPlayerData(UndoManager.GetLocalUserId());
            data.ResetUndo();
        }
    }
}