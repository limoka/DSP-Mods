using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using CommonAPI;
using HarmonyLib;
using NebulaAPI;
using UnityEngine;

namespace BlueprintTweaks.FactoryUndo
{
    [RegisterPatch(BlueprintTweaksPlugin.FACTORY_UNDO)]
    public static class PlayerActionBuild_Patch
    {
        
        [HarmonyPatch(typeof(PlayerAction_Build), nameof(PlayerAction_Build.NotifyBuilt))]
        [HarmonyPostfix]
        public static void OnNotifyBuilt(int preObjId, int postObjId)
        {
            if (NebulaModAPI.IsMultiplayerActive)
            {
                if (NebulaModAPI.MultiplayerSession.LocalPlayer.IsClient) return;
            }

            foreach (PlayerUndo data in UndoManager.undos.Values)
            {
                foreach (IUndoAction action in data.notifyBuildListeners)
                {
                    action.NotifyBuild(preObjId, postObjId);
                }

                for (int i = 0; i < data.notifyBuildListeners.Count; i++)
                {
                    IUndoAction action = data.notifyBuildListeners[i];
                    if (!action.HasAnyPrebuilds())
                    {
                        data.notifyBuildListeners.RemoveAt(i);
                        data.notifyDismantleListeners.Add(action);
                        i--;
                    }
                }
            }
        }
        
        [HarmonyPatch(typeof(PlayerAction_Build), nameof(PlayerAction_Build.NotifyDismantled))]
        [HarmonyPostfix]
        public static void OnNotifyDismantled(int objId)
        {
            if (NebulaModAPI.IsMultiplayerActive)
            {
                if (NebulaModAPI.MultiplayerSession.LocalPlayer.IsClient) return;
            }
            
            if (UndoManager.IgnoreAllEvents.Value) return;

            foreach (PlayerUndo data in UndoManager.undos.Values)
            {
                foreach (IUndoAction action in data.notifyDismantleListeners)
                {
                    action.NotifyDismantled(objId);
                }

                data.notifyDismantleListeners.RemoveAll(action => action.IsEmpty());
            }
        }
    }
}