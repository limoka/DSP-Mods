namespace BlueprintTweaks.FactoryUndo
{
    public abstract class UndoAction : IUndoAction
    {
        protected PlayerUndo undoData;
        
        public abstract void NotifyBuild(int preObjId, int postObjId);
        public abstract void NotifyDismantled(int objId);
        public abstract bool Undo(PlanetFactory factory, PlayerAction_Build actionBuild);
        public abstract bool Redo(PlanetFactory factory, PlayerAction_Build actionBuild);
        public abstract bool HasAnyPrebuilds();
        public abstract bool IsEmpty();

        protected UndoAction(PlayerUndo undoData)
        {
            this.undoData = undoData;
        }
    }
}