using UnityEngine;

namespace BlueprintTweaks.FactoryUndo
{
    public interface IUndoAction
    {
        void NotifyBuild(int preObjId, int postObjId);
        void NotifyDismantled(int objId);
        bool Undo(PlanetFactory factory, PlayerAction_Build actionBuild);
        bool Redo(PlanetFactory factory, PlayerAction_Build actionBuild);
        bool HasAnyPrebuilds();
        bool IsEmpty();
    }
}