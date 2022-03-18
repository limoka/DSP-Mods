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

            using (UndoManager.IgnoreAllEvents.On())
            {
                copy.InitTool();
                copy.selectedObjIds.Clear();
                foreach (int objectId in objectIds)
                {
                    copy.selectedObjIds.Add(objectId);
                }

                copy.blueprint = BlueprintData.CreateNew();
                copy.RefreshBlueprintData();

                Vector2 minPos = GetMinLongLat(objectIds, copy);

                float radius = GameMain.localPlanet.realRadius;
                position = BlueprintUtils.GetDir(minPos.x, minPos.y) * radius;
                position = copy.actionBuild.planetAux.Snap(position, true);
            }

            return copy.blueprint;
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

        public static HashSet<int> DetermineDismantle(this BuildTool_Dismantle tool)
        {
            HashSet<int> objectIds = new HashSet<int>();

            if ((VFInput._buildConfirm.onDown && tool.cursorType == 0 || (VFInput._buildConfirm.pressing && tool.cursorType == 1)) &&
                tool.buildPreviews.Count > 0)
            {
                foreach (BuildPreview buildPreview in tool.buildPreviews)
                {
                    if (buildPreview.condition == EBuildCondition.Ok)
                    {
                        if (BuildTool_Dismantle.showDemolishContainerQuery)
                        {
                            if (buildPreview.objId > 0 && buildPreview.desc.isStorage)
                            {
                                int storageId = tool.factory.entityPool[buildPreview.objId].storageId;
                                if (storageId != 0)
                                {
                                    if (tool.cursorType == 0)
                                    {
                                        return new HashSet<int>();
                                    }

                                    break;
                                }
                            }

                            if (buildPreview.objId > 0 && buildPreview.desc.isTank)
                            {
                                int tankId = tool.factory.entityPool[buildPreview.objId].tankId;
                                if (tankId != 0)
                                {
                                    if (tool.cursorType == 0)
                                    {
                                        return new HashSet<int>();
                                    }

                                    break;
                                }
                            }

                            if (buildPreview.objId > 0 && buildPreview.desc.isStation)
                            {
                                int stationId = tool.factory.entityPool[buildPreview.objId].stationId;
                                StationComponent stationComponent = tool.factory.transport.stationPool[stationId];
                                if (stationComponent != null)
                                {
                                    if (tool.cursorType == 0)
                                    {
                                        return new HashSet<int>();
                                    }

                                    break;
                                }
                            }
                        }

                        if (tool.cursorType == 0 && tool.ObjectIsBelt(buildPreview.objId))
                        {
                            tool.factory.ReadObjectConn(buildPreview.objId, 0, out bool _, out tool.neighborId0, out int _);
                            tool.factory.ReadObjectConn(buildPreview.objId, 1, out bool _, out tool.neighborId1, out int _);
                            tool.factory.ReadObjectConn(buildPreview.objId, 2, out bool _, out tool.neighborId2, out int _);
                            tool.factory.ReadObjectConn(buildPreview.objId, 3, out bool _, out tool.neighborId3, out int _);
                            if (!tool.ObjectIsBelt(tool.neighborId0))
                            {
                                tool.neighborId0 = 0;
                            }

                            if (!tool.ObjectIsBelt(tool.neighborId1))
                            {
                                tool.neighborId1 = 0;
                            }

                            if (!tool.ObjectIsBelt(tool.neighborId2))
                            {
                                tool.neighborId2 = 0;
                            }

                            if (!tool.ObjectIsBelt(tool.neighborId3))
                            {
                                tool.neighborId3 = 0;
                            }
                        }

                        objectIds.Add(buildPreview.objId);
                    }
                }
            }

            if (VFInput._buildConfirm.pressing && tool.cursorType == 0 && !tool.chainReaction)
            {
                foreach (BuildPreview buildPreview in tool.buildPreviews)
                {
                    if ((buildPreview.objId == tool.neighborId0 || buildPreview.objId == tool.neighborId1 || buildPreview.objId == tool.neighborId2 ||
                         buildPreview.objId == tool.neighborId3) && buildPreview.condition == EBuildCondition.Ok)
                    {
                        if (tool.ObjectIsBelt(buildPreview.objId))
                        {
                            tool.factory.ReadObjectConn(buildPreview.objId, 0, out bool _, out tool.neighborId0, out int _);
                            tool.factory.ReadObjectConn(buildPreview.objId, 1, out bool _, out tool.neighborId1, out int _);
                            tool.factory.ReadObjectConn(buildPreview.objId, 2, out bool _, out tool.neighborId2, out int _);
                            tool.factory.ReadObjectConn(buildPreview.objId, 3, out bool _, out tool.neighborId3, out int _);
                            if (!tool.ObjectIsBelt(tool.neighborId0))
                            {
                                tool.neighborId0 = 0;
                            }

                            if (!tool.ObjectIsBelt(tool.neighborId1))
                            {
                                tool.neighborId1 = 0;
                            }

                            if (!tool.ObjectIsBelt(tool.neighborId2))
                            {
                                tool.neighborId2 = 0;
                            }

                            if (!tool.ObjectIsBelt(tool.neighborId3))
                            {
                                tool.neighborId3 = 0;
                            }
                        }

                        objectIds.Add(buildPreview.objId);
                    }
                }

                return objectIds;
            }

            if (VFInput._buildConfirm.pressing && tool.cursorType == 1)
            {
                tool.neighborId0 = 0;
                tool.neighborId1 = 0;
                tool.neighborId2 = 0;
                tool.neighborId3 = 0;
                return objectIds;
            }

            tool.neighborId0 = 0;
            tool.neighborId1 = 0;
            tool.neighborId2 = 0;
            tool.neighborId3 = 0;
            return objectIds;
        }
    }
}