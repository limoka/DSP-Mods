using System.Collections.Generic;
using UnityEngine;

namespace BlueprintTweaks.FactoryUndo
{
    public class UndoDismantle : UndoBlueprint
    {
        public UndoDismantle(PlayerUndo undoData, IEnumerable<int> objectIds, BlueprintData blueprint, Vector3[] dotsSnapped, float yaw) 
            : base(undoData, objectIds, blueprint, dotsSnapped, yaw)
        {
            
        }

        public override bool Undo(PlanetFactory factory, PlayerAction_Build actionBuild)
        {
            return base.Redo(factory, actionBuild);
        }

        public override bool Redo(PlanetFactory factory, PlayerAction_Build actionBuild)
        {
            return base.Undo(factory, actionBuild);
        }
    }
}