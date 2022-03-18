using System.Collections.Generic;
using UnityEngine;

namespace BlueprintTweaks.FactoryUndo
{
    public class UndoPathAddon : UndoBuild
    {
        public UndoPathAddon(PlayerUndo undoData, int objectId) : base(undoData, objectId) { }
        private UndoPathAddon(PlayerUndo undoData, IEnumerable<int> objectIds) : base(undoData, objectIds) { }

        public override bool Redo(PlanetFactory factory, PlayerAction_Build actionBuild)
        {
            if (previews == null) return false;
            if (previews.Count != 1) return false;
            
            BuildTool_PathAddon pathAddon = actionBuild.assistTool;

            pathAddon.InitTool();
            pathAddon.handbp = previews[0];
            bool condition;
            
            using (UndoManager.IgnoreAllEvents.On())
            {
                pathAddon.ActiveColliders();
                pathAddon.FindPotentialBelt();
                pathAddon.FindPotentialBeltStrict();
                condition = pathAddon.CheckBuildConditions();

                if (condition)
                {
                    pathAddon.CreatePrebuilds();
                }

                objectIds.Clear();
                objectIds.Add(pathAddon.handbp.objId);

                if (objectIds.Count > 0)
                {
                    if (!undoData.notifyBuildListeners.Contains(this))
                        undoData.notifyBuildListeners.Add(this);
                    if (!undoData.notifyDismantleListeners.Contains(this))
                        undoData.notifyDismantleListeners.Add(this);
                }
            }
                
            previews.Clear();
            previews = null;
            return condition;
        }
    }
}