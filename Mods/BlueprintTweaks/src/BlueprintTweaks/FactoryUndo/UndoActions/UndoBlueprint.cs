using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlueprintTweaks.FactoryUndo
{
    public class UndoBlueprint : UndoBuild
    {
        protected BlueprintData blueprint;
        protected Vector3[] dotsSnapped;
        protected float yaw;

        public UndoBlueprint(PlayerUndo undoData, IEnumerable<int> objectIds, BlueprintData blueprint, Vector3[] dotsSnapped, float yaw) : base(undoData, objectIds)
        {
            this.blueprint = blueprint;
            this.dotsSnapped = dotsSnapped;
            this.yaw = yaw;
        }

        public override bool Undo(PlanetFactory factory, PlayerAction_Build actionBuild)
        {
            using (UndoManager.IgnoreAllEvents.On())
            {
                RemoveHelper.excludeStationOverride = BlueprintTweaksPlugin.undoExcludeStations.Value;
                RemoveHelper.SwitchDelete(factory, objectIds);
                RemoveHelper.excludeStationOverride = false;
            }

            undoData.notifyBuildListeners.Remove(this);

            return true;
        }

        public override bool Redo(PlanetFactory factory, PlayerAction_Build actionBuild)
        {
            if (blueprint == null) return false;
            if (dotsSnapped == null || dotsSnapped.Length == 0) return false;

            BuildTool_BlueprintPaste paste = actionBuild.blueprintPasteTool;

            BlueprintData oldBlueprint = paste.blueprint?.Clone();
            string oldPath = paste.blueprintPath;
            
            paste._OnOpen();
            paste.InitTool();

            paste.yaw = yaw;
            Array.Clear(paste.dotsSnapped, 0, paste.dotsSnapped.Length);
            for (int i = 0; i < dotsSnapped.Length; i++)
            {
                paste.dotsSnapped[i] = dotsSnapped[i];
            }

            paste.dotsCursor = dotsSnapped.Length;
            paste.blueprint = blueprint;

            paste.cursorValid = true;
            paste.GenerateBlueprintGratBoxes();
            paste.DeterminePreviewsPrestage(true);

            bool success = false;

            using (UndoManager.IgnoreAllEvents.On())
            {
                bool condition = paste.CheckBuildConditionsPrestage();
                if (condition)
                {
                    paste.ActiveColliders(actionBuild.model);
                    bool isOkay = paste.CheckBuildConditions();
                    paste.DeterminePreviews();
                    paste.DeactiveColliders(actionBuild.model);
                    
                    paste.result = (isOkay ? (paste.result & ~EBlueprintPasteResult.HasError) : (paste.result | EBlueprintPasteResult.HasError));

                    if (isOkay)
                    {
                        paste.CreatePrebuilds();
                        success = true;
                    }
                }
            }


            objectIds.Clear();
            for (int l = 0; l < paste.bpCursor; l++)
            {
                BuildPreview preview = paste.bpPool[l];
                objectIds.Add(preview.objId);
            }

            if (objectIds.Count > 0)
            {
                if (!undoData.notifyBuildListeners.Contains(this))
                    undoData.notifyBuildListeners.Add(this);
                if (!undoData.notifyDismantleListeners.Contains(this))
                    undoData.notifyDismantleListeners.Add(this);
            }

            paste.uiInspector._Close();
            paste.ResetStates();
            paste.blueprint = oldBlueprint;
            paste.blueprintPath = oldPath;
            
            return success;
        }
    }
}