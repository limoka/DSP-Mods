using System.Collections.Generic;

namespace BlueprintTweaks.FactoryUndo
{
    public struct UpgradeData
    {
        public UpgradeData(int objId, int oldGrade, int newGrade)
        {
            this.objId = objId;
            this.oldGrade = oldGrade;
            this.newGrade = newGrade;
        }

        public int objId;
        public int oldGrade;
        public int newGrade;
    }

    public class UndoUpgrade : UndoAction
    {
        public List<UpgradeData> targets;

        public UndoUpgrade(PlayerUndo undoData, IEnumerable<UpgradeData> targets) : base(undoData)
        {
            this.targets = new List<UpgradeData>(targets);
        }

        public override void NotifyBuild(int preObjId, int postObjId)
        {
        }

        public override void NotifyDismantled(int objId)
        {
            targets.RemoveAll(data => data.objId == objId);
        }

        public override bool HasAnyPrebuilds()
        {
            return false;
        }

        public override bool IsEmpty()
        {
            return targets != null && targets.Count == 0;
        }

        public override bool Undo(PlanetFactory factory, PlayerAction_Build actionBuild)
        {
            bool success = true;

            using (UndoManager.IgnoreAllEvents.On())
            {
                foreach (UpgradeData data in targets)
                {
                    if (!actionBuild.DoUpgradeObject(data.objId, data.oldGrade, 0, out int error))
                    {
                        success = false;
                        if (error == 1)
                        {
                            BlueprintTweaksPlugin.logger.LogDebug($"Can't upgrade {data.objId} to {data.oldGrade}, not enough items");
                        }
                    }
                }
            }

            return success;
        }

        public override bool Redo(PlanetFactory factory, PlayerAction_Build actionBuild)
        {
            bool success = true;

            using (UndoManager.IgnoreAllEvents.On())
            {
                foreach (UpgradeData data in targets)
                {
                    if (!actionBuild.DoUpgradeObject(data.objId, data.newGrade, 0, out int error))
                    {
                        success = false;
                        if (error == 1)
                        {
                            BlueprintTweaksPlugin.logger.LogDebug($"Can't upgrade {data.objId} to {data.newGrade}, not enough items");
                        }
                    }
                }
            }

            return success;
        }
    }
}