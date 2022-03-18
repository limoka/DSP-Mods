using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace BlueprintTweaks.FactoryUndo
{
    public class UndoBuild : UndoAction
    {
        protected static int[] tmpIds = new int[20];
        public static BuildingParameters undoClipboard = BuildingParameters.zero;
        
        public UndoBuild(PlayerUndo undoData, int objectId) : base(undoData)
        {
            objectIds = new List<int> {objectId};
        }
        
        public UndoBuild(PlayerUndo undoData, IEnumerable<int> objectIds) : base(undoData)
        {
            this.objectIds = new List<int>(objectIds);
        }

        protected List<int> objectIds;
        protected List<BuildPreview> previews;

        protected static Dictionary<int, BuildPreview> tmpPreviews = new Dictionary<int, BuildPreview>();

        public override void NotifyBuild(int preObjId, int postObjId)
        {
            int index = objectIds.FindIndex(i => preObjId == i);
            if (index >= 0)
            {
                objectIds[index] = postObjId;
            }
        }

        public override void NotifyDismantled(int objId)
        {
            objectIds.Remove(objId);
        }

        public override bool HasAnyPrebuilds()
        {
            return objectIds.Any(i => i < 0);
        }

        public override bool IsEmpty()
        {
            return objectIds == null || objectIds.Count == 0;
        }

        public override bool Undo(PlanetFactory factory, PlayerAction_Build actionBuild)
        {
            bool success = SaveBuildData(factory, actionBuild);

            using (UndoManager.IgnoreAllEvents.On())
            {
                foreach (int objectId in objectIds)
                {
                    if (objectId != 0)
                    {
                        actionBuild.DoDismantleObject(objectId);
                    }
                }
            }

            undoData.notifyBuildListeners.Remove(this);
            undoData.notifyDismantleListeners.Remove(this);

            return success;
        }

        protected bool SaveBuildData(PlanetFactory factory, PlayerAction_Build actionBuild)
        {
            bool anySuccess = false;

            previews = new List<BuildPreview>(objectIds.Count);
            tmpPreviews.Clear();

            foreach (int objectId in objectIds)
            {
                if (objectId > 0)
                {
                    ref EntityData data = ref factory.entityPool[objectId];
                    if (data.id == objectId)
                    {
                        BuildPreview preview = InitPreviewFromObject(factory, actionBuild, data.id);
                        previews.Add(preview);
                        anySuccess = true;
                    }
                }
                else if (objectId < 0)
                {
                    ref PrebuildData data = ref factory.prebuildPool[-objectId];
                    if (data.id == -objectId)
                    {
                        BuildPreview preview = InitPreviewFromObject(factory, actionBuild, -data.id);
                        previews.Add(preview);
                        anySuccess = true;
                    }
                }
            }

            tmpPreviews.Clear();
            return anySuccess;
        }

        private static BuildPreview InitPreviewFromObject(PlanetFactory factory, PlayerAction_Build actionBuild, int objectId, bool checkConn = true)
        {
            BuildPreview preview;

            if (tmpPreviews.ContainsKey(objectId))
            {
                preview = tmpPreviews[objectId];
            }
            else
            {
                preview = new BuildPreview();
                preview.ResetAll();
                tmpPreviews.Add(objectId, preview);
            }
            

            preview.item = actionBuild.noneTool.GetItemProto(objectId);
            preview.desc = actionBuild.noneTool.GetPrefabDesc(objectId);

            Pose pose = actionBuild.noneTool.GetObjectPose(objectId);
            Pose pose2 = actionBuild.noneTool.GetObjectPose2(objectId);

            preview.lpos = pose.position;
            preview.lrot = pose.rotation;
            preview.lpos2 = pose2.position;
            preview.lrot2 = pose2.rotation;

            undoClipboard.CopyFromFactoryObject(objectId, factory);

            preview.recipeId = undoClipboard.recipeId;
            preview.filterId = undoClipboard.filterId;
            preview.parameters = undoClipboard.parameters;
            if (undoClipboard.parameters != null)
            {
                preview.paramCount = undoClipboard.parameters.Length;
            }

            if (!checkConn) return preview;
            
            factory.ReadObjectConn(objectId, 0, out _, out int otherObjId, out int _);
            if (otherObjId != 0)
            {
                if (tmpPreviews.ContainsKey(otherObjId))
                {
                    preview.output = tmpPreviews[otherObjId];
                    preview.outputFromSlot = 0;
                    preview.outputToSlot = 1;
                }
                else
                {
                    BuildPreview other = new BuildPreview();
                    preview.output = other;
                    preview.outputFromSlot = 0;
                    preview.outputToSlot = 1;
                    preview.objId = otherObjId;
                    tmpPreviews.Add(otherObjId, other);
                }
            }

            return preview;
        }

        public override bool Redo(PlanetFactory factory, PlayerAction_Build actionBuild)
        {
            if (previews == null) return false;

            BuildTool_Click click = actionBuild.clickTool;

            click.InitTool();

            if (previews.Count == 1)
            {
                BuildPreview preview = previews[0];

                if (preview.desc.multiLevel)
                {
                    Vector3 prevBuildingPos = preview.lpos - preview.lrot * preview.desc.lapJoint;
                    int count = factory.planet.physics.nearColliderLogic.GetBuildingsInAreaNonAlloc(prevBuildingPos, 0.25f, tmpIds, false);
                    if (count > 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            int objId = tmpIds[i];
                            Pose objPos = actionBuild.noneTool.GetObjectPose(objId);
                            PrefabDesc desc = actionBuild.noneTool.GetPrefabDesc(objId);
                            if (desc != null && preview.desc.modelIndex == desc.modelIndex &&
                                (objPos.position - prevBuildingPos).magnitude < 0.1f)
                            {
                                preview.inputObjId = objId;
                                preview.inputFromSlot = 15;
                                preview.inputToSlot = 14;
                                break;
                            }
                        }
                    }
                }
            }

            click.buildPreviews.Clear();
            click.buildPreviews.AddRange(previews);

            bool condition;

            using (UndoManager.IgnoreAllEvents.On())
            {
                click.CheckInserters();
                condition = click.CheckBuildConditions();

                if (condition)
                {
                    click.CreatePrebuilds();
                }

                objectIds.Clear();
                foreach (BuildPreview preview in click.buildPreviews)
                {
                    objectIds.Add(preview.objId);
                }

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