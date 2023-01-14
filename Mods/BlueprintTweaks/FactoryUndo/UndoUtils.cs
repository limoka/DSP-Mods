using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlueprintTweaks.FactoryUndo
{
    public static class UndoUtils
    {
        public static void CheckInserters(this BuildTool_Click tool)
        {
            foreach (BuildPreview preview in tool.buildPreviews)
            {
                if (preview.desc.isInserter)
                {
                    tool.MatchInserter(preview);
                }
            }
        }

        public static void InitTool(this BuildTool tool)
        {
            PlayerAction_Build pab = tool.controller.actionBuild;
            if (pab.model == null)
            {
                pab.model = new BuildModel();
                pab.model.Init(pab);
            }
            pab.model.Open();

            tool.SetFactoryReferences();
            if (tool.tmpPackage == null)
            {
                tool.tmpPackage = new StorageComponent(tool.player.package.size);
            }

            if (tool.tmpPackage.size != tool.player.package.size)
            {
                tool.tmpPackage.SetSize(tool.player.package.size);
            }

            Array.Copy(tool.player.package.grids, tool.tmpPackage.grids, tool.tmpPackage.size);

            tool._Init(GameMain.data);
        }

        public static BlueprintData GenerateBlueprint(List<int> objectIds, out Vector3 position)
        {
            position = Vector3.zero;
            if (GameMain.mainPlayer?.controller == null) return BlueprintData.CreateNew();
            if (GameMain.localPlanet == null) return BlueprintData.CreateNew();

            BuildTool_BlueprintCopy copy = GameMain.mainPlayer.controller.actionBuild.blueprintCopyTool;
            BlueprintData oldBlueprint = copy.blueprint?.Clone();
            string oldPath = copy.blueprintPath;

            using (UndoManager.IgnoreAllEvents.On())
            {
                copy.InitTool();
                copy.selectedObjIds.Clear();
                foreach (int objectId in objectIds)
                {
                    copy.selectedObjIds.Add(objectId);
                }

                copy.blueprint = BlueprintData.CreateNew();
                copy.GenerateGratBox();
                copy.GenerateBlueprintData();

                Vector2 minPos = GetMinLongLat(objectIds, copy);

                float radius = GameMain.localPlanet.realRadius;
                position = BlueprintUtils.GetDir(minPos.x, minPos.y) * radius;
                position = copy.actionBuild.planetAux.Snap(position, true);
            }

            BlueprintData result = copy.blueprint;
            copy.blueprint = oldBlueprint;
            copy.blueprintPath = oldPath;

            return result;
        }

        private static Vector2 GetMinLongLat(List<int> objectIds, BuildTool_BlueprintCopy copy)
        {
            float minLat = 10;
            float minLong = 10;

            float objLat = 0;
            float objLong = 0;

            foreach (int objectId in objectIds)
            {
                if (objectId > 0)
                {
                    ref EntityData data = ref copy.factory.entityPool[objectId];
                    if (data.id == objectId)
                    {
                        BlueprintUtils.GetLongitudeLatitudeRad(data.pos.normalized, ref objLong, ref objLat);
                        if (objLong < minLong)
                        {
                            minLong = objLong;
                        }

                        if (objLat < minLat)
                        {
                            minLat = objLat;
                        }
                    }
                }
                else if (objectId < 0)
                {
                    ref PrebuildData data = ref copy.factory.prebuildPool[-objectId];
                    if (data.id == -objectId)
                    {
                        BlueprintUtils.GetLongitudeLatitudeRad(data.pos.normalized, ref objLong, ref objLat);
                        if (objLong < minLong)
                        {
                            minLong = objLong;
                        }

                        if (objLat < minLat)
                        {
                            minLat = objLat;
                        }
                    }
                }
            }

            return new Vector2(minLong, minLat);
        }
    }
}