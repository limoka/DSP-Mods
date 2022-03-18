using System;
using System.Collections.Generic;
using System.Linq;
using NebulaAPI;
using UnityEngine;

namespace BlueprintTweaks.FactoryUndo
{
    public class PlayerUndo
    {
        public FixedSizeStack<IUndoAction> undoStack;
        public FixedSizeStack<IUndoAction> redoStack;

        public List<IUndoAction> notifyBuildListeners = new List<IUndoAction>();
        public List<IUndoAction> notifyDismantleListeners = new List<IUndoAction>();
        

        public PlayerUndo()
        {
            undoStack = new FixedSizeStack<IUndoAction>(BlueprintTweaksPlugin.undoMaxHistory.Value);
            redoStack = new FixedSizeStack<IUndoAction>(BlueprintTweaksPlugin.undoMaxHistory.Value);
        }

        internal void ResetUndo()
        {
            undoStack.Clear();
            redoStack.Clear();
            notifyBuildListeners.Clear();
            notifyDismantleListeners.Clear();

            if (NebulaModAPI.IsMultiplayerActive)
            {
                if (NebulaModAPI.MultiplayerSession.Factories.IsIncomingRequest.Value) return;
            }
            
            UIRealtimeTip.Popup("UndoClearedMessage".Translate(), false);
            VFAudio.Create("cancel-0", null, Vector3.zero, true, 4);
        }

        internal void AddUndoItem(IUndoAction action)
        {
            undoStack.Push(action);
            notifyBuildListeners.Add(action);
            notifyDismantleListeners.Add(action);
            
            foreach (IUndoAction undoAction in redoStack)
            {
                notifyBuildListeners.Remove(undoAction);
                notifyDismantleListeners.Remove(undoAction);
            }
            redoStack.Clear();
        }

        internal bool TryUndo(out string message, out bool sound)
        {
            sound = false;
            message = "";
            if (undoStack.Count <= 0)
            {
                sound = true;
                message = "UndoHistoryEmptyMessage";
                return true;
            }
            if (GameMain.localPlanet?.factory == null) return false;
            if (GameMain.mainPlayer?.controller == null) return false;
            
            PlanetFactory factory = GameMain.localPlanet.factory;
            PlayerAction_Build actionBuild = GameMain.mainPlayer.controller.actionBuild;

            IUndoAction action = undoStack.Pop();

            bool success = false;
            
            try
            {
                success = action.Undo(factory, actionBuild);
            }
            catch (Exception e)
            {
                BlueprintTweaksPlugin.logger.LogWarning($"Failed to undo, message: {e.Message}, stacktrace:\n{e.StackTrace}");
            }
            
            if (success)
            {
                message = "UndoSuccessText";
                redoStack.Push(action);
            }
            else
            {
                message = "UndoFailureText";
                sound = true;
                notifyBuildListeners.Remove(action);
                notifyDismantleListeners.Remove(action);
            }

            return true;
        }

        internal bool TryRedo(out string message, out bool sound)
        {
            sound = false;
            message = "";
            if (redoStack.Count <= 0)
            {
                sound = true;
                message = "RedoHistoryEmptyMessage";
                return true;
            }
            if (GameMain.localPlanet?.factory == null) return false;
            if (GameMain.mainPlayer?.controller == null) return false;
            
            PlanetFactory factory = GameMain.localPlanet.factory;
            PlayerAction_Build actionBuild = GameMain.mainPlayer.controller.actionBuild;

            IUndoAction action = redoStack.Pop();
            
            bool success = false;

            try
            {
                success = action.Redo(factory, actionBuild);
            }
            catch (Exception e)
            {
                BlueprintTweaksPlugin.logger.LogWarning($"Failed to redo, message: {e.Message}, stacktrace:\n{e.StackTrace}");
            }
            
            if (success)
            {
                message = "RedoSuccessText";
                undoStack.Push(action);
            }
            else
            {
                sound = true;
                message = "RedoFailureText";
                notifyBuildListeners.Remove(action);
                notifyDismantleListeners.Remove(action);
            }

            return true;
        }
    }
}