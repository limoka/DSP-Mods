using System;
using System.Collections.Generic;
using System.Linq;
using BlueprintTweaks.FactoryUndo;
using BlueprintTweaks.Nebula;
using CommonAPI;
using HarmonyLib;
using NebulaAPI;
using NebulaAPI.GameState;
using NebulaAPI.Networking;
using PowerNetworkStructures;
using UnityEngine;

namespace BlueprintTweaks
{
    [RegisterPatch(BlueprintTweaksPlugin.DRAG_REMOVE)]
    public static class FastRemoveHelper
    {
        internal static Dictionary<int, int> takeBackCount = new Dictionary<int, int>();
        internal static Dictionary<int, int> takeBackInc = new Dictionary<int, int>();
        internal static bool captureTempCargos;

        private static List<HashSet<int>> powerConRemoval = new List<HashSet<int>>();
        private static bool[] _divideline_marks;

        public static bool excludeStationOverride;

        public static bool ShouldExcludeStations => excludeStationOverride || BlueprintTweaksPlugin.excludeStations.Value;

        public static void SwitchDelete(PlanetFactory factory, List<int> targetIds, List<int> edgeIds)
        {
            if (targetIds.Count <= 0) return;

            if (NebulaModAPI.IsMultiplayerActive)
            {
                IMultiplayerSession session = NebulaModAPI.MultiplayerSession;
                int planetId = session.Factories.TargetPlanet != NebulaModAPI.PLANET_NONE ? session.Factories.TargetPlanet : factory.planet?.id ?? -1;


                if (session.LocalPlayer.IsHost || !session.Factories.IsIncomingRequest.Value)
                {
                    session.Network.SendPacket(new FastRemoveRequestPacket(planetId, targetIds.ToArray(), edgeIds.ToArray(),
                        session.Factories.PacketAuthor == NebulaModAPI.AUTHOR_NONE
                            ? session.LocalPlayer.Id
                            : session.Factories.PacketAuthor, true, ShouldExcludeStations));
                }

                if (!session.LocalPlayer.IsHost && !session.Factories.IsIncomingRequest.Value)
                {
                    return;
                }
            }

            GenerateUndoData(factory, targetIds);

            if (targetIds.Count < 25)
            {
                RegularDeleteEntities(factory, targetIds);
                return;
            }

            if (edgeIds.Count == 0)
            {
                foreach (int objectId in targetIds)
                {
                    if (objectId > 0 && factory.entityPool[objectId].beltId > 0)
                    {
                        RegularDeleteEntities(factory, targetIds);
                        return;
                    }
                }

                RegularDeleteEntities(factory, targetIds);
                return;
            }

            if (BlueprintTweaksPlugin.useFastDismantle.Value)
            {
                FastDeleteEntities(factory, targetIds, edgeIds);
            }
            else
            {
                RegularDeleteEntities(factory, targetIds);
            }
        }

        private static void GenerateUndoData(PlanetFactory factory, List<int> targetIds)
        {
            if (NebulaModAPI.IsMultiplayerActive)
            {
                if (NebulaModAPI.MultiplayerSession.LocalPlayer.IsClient)
                {
                    return;
                }
            }

            try
            {
                List<int> filteredIds = targetIds.Where(objId =>
                {
                    if (objId < 0)
                    {
                        int modelIndex = factory.prebuildPool[-objId].modelIndex;
                        ModelProto modelProto = LDB.models.Select(modelIndex);

                        if (modelProto == null) return false;
                        return !modelProto.prefabDesc.isStation || !ShouldExcludeStations;
                    }

                    if (objId > 0)
                    {
                        int modelIndex = factory.entityPool[objId].modelIndex;
                        ModelProto modelProto = LDB.models.Select(modelIndex);

                        if (modelProto == null) return false;
                        return !modelProto.prefabDesc.isStation || !ShouldExcludeStations;
                    }

                    return false;
                }).ToList();

                BlueprintData blueprint = UndoUtils.GenerateBlueprint(filteredIds, out Vector3 position);
                if (blueprint.buildings.Length > 0 && !position.Equals(Vector3.zero))
                {
                    PlayerUndo data = UndoManager.GetCurrentPlayerData();

                    data.AddUndoItem(new UndoDismantle(data, filteredIds, blueprint, new[] { position }, 0));
                }
            }
            catch (Exception e)
            {
                BlueprintTweaksPlugin.logger.LogWarning($"Failed to generate undo for drag dismantle: {e.Message}, stacktrace:\n{e.StackTrace}");
            }
        }

        public static void SwitchDelete(PlanetFactory factory, List<int> objectIds)
        {
            if (objectIds.Count <= 0) return;

            if (NebulaModAPI.IsMultiplayerActive)
            {
                IMultiplayerSession session = NebulaModAPI.MultiplayerSession;
                int planetId = session.Factories.TargetPlanet != NebulaModAPI.PLANET_NONE ? session.Factories.TargetPlanet : factory.planet?.id ?? -1;


                if (session.LocalPlayer.IsHost || !session.Factories.IsIncomingRequest.Value)
                {
                    session.Network.SendPacket(new FastRemoveRequestPacket(planetId, objectIds.ToArray(), Array.Empty<int>(),
                        session.Factories.PacketAuthor == NebulaModAPI.AUTHOR_NONE
                            ? session.LocalPlayer.Id
                            : session.Factories.PacketAuthor, false, ShouldExcludeStations));
                }

                if (!session.LocalPlayer.IsHost && !session.Factories.IsIncomingRequest.Value)
                {
                    return;
                }
            }

            if (objectIds.Count < 25)
            {
                RegularDeleteEntities(factory, objectIds);
                return;
            }

            if (BlueprintTweaksPlugin.useFastDismantle.Value)
            {
                var player = GameMain.mainPlayer;
                PlayerAction_Build actionBuild = player.controller.actionBuild;

                float divLine = GetDivideLine(factory, actionBuild, objectIds);
                BPGratBox boundingBox = BlueprintUtils.GetBoundingRange(factory.planet, factory.planet.aux, objectIds.ToArray(), objectIds.Count, divLine);
                HashSet<int> edgeObjIds = new HashSet<int>();

                int segmentCnt = 200;
                if (GameMain.localPlanet?.aux?.activeGrid != null)
                {
                    segmentCnt = GameMain.localPlanet.aux.activeGrid.segment;
                }

                foreach (int objectId in objectIds)
                {
                    if (objectId <= 0 || factory.entityPool[objectId].id != objectId) continue;

                    if (boundingBox.IsOnEdgeOfGratBox(factory.entityPool[objectId].pos, segmentCnt))
                    {
                        edgeObjIds.Add(objectId);
                    }
                }

                if (edgeObjIds.Count == 0)
                {
                    foreach (int objectId in objectIds)
                    {
                        if (objectId <= 0 || factory.entityPool[objectId].beltId <= 0) continue;

                        RegularDeleteEntities(factory, objectIds);
                        return;
                    }
                }

                FastDeleteEntities(factory, objectIds, edgeObjIds.ToList());
            }
            else
            {
                RegularDeleteEntities(factory, objectIds);
            }
        }

        private static float GetDivideLine(PlanetFactory factory, PlayerAction_Build actionBuild, List<int> objectIds)
        {
            if (objectIds.Count == 0)
            {
                return -Mathf.PI;
            }

            _divideline_marks ??= new bool[1001];
            Array.Clear(_divideline_marks, 0, 1001);

            int segment = 200;
            if (factory.planet.aux.activeGrid != null)
            {
                segment = factory.planet.aux.activeGrid.segment;
            }

            const double divisor = 2 * Mathf.PI / 1000;
            foreach (int objId in objectIds)
            {
                if (actionBuild.noneTool.GetPrefabDesc(objId) != null)
                {
                    Vector3 position = actionBuild.noneTool.GetObjectPose(objId).position.normalized;
                    float latitudeRad = Mathf.Asin(position.y);
                    float longitudeRad = Mathf.Atan2(position.x, -position.z);

                    int longitudeSegmentCount = BlueprintUtils.GetLongitudeSegmentCount(latitudeRad, segment);
                    int latGrid = Mathf.CeilToInt((Mathf.CeilToInt((float)segment / longitudeSegmentCount) - 1) * 0.5f);
                    int longGrid = (int)Math.Round((longitudeRad + Mathf.PI) / divisor);

                    for (int i = -latGrid; i <= latGrid; i++)
                    {
                        int index = (int)Mathf.Repeat(longGrid + i, 1000f);
                        _divideline_marks[index] = true;
                    }
                }
            }

            int gridx1 = -1;
            int gridy1 = -1;
            int gridx2 = -1;
            int grixy2 = -1;
            int gridPos2 = 0;
            for (int j = 0; j < 1001; j++)
            {
                if (_divideline_marks[j])
                {
                    if (gridx2 == -1)
                    {
                        gridx2 = j;
                        gridx1 = j;
                        gridy1 = j;
                    }
                    else
                    {
                        int num11 = j - gridx2;
                        if (num11 > gridPos2)
                        {
                            grixy2 = gridx2;
                            gridPos2 = num11;
                        }

                        gridx2 = j;
                        gridy1 = j;
                    }
                }
            }

            int gridPos1 = gridx1 + 1000 - gridy1;
            if (gridPos1 > gridPos2)
            {
                int gridPos = gridy1 + gridPos1 / 2;
                return Mathf.Repeat((float)(gridPos * divisor), 2 * Mathf.PI) - Mathf.PI;
            }

            gridPos2 = grixy2 + gridPos2 / 2;
            return (float)(gridPos2 * divisor - Mathf.PI);
        }

        public static void RegularDeleteEntities(PlanetFactory factory, List<int> targetIds)
        {
            var player = GameMain.mainPlayer;

            PlayerAction_Build actionBuild = player.controller.actionBuild;

            using IDisposable toggle = UndoManager.IgnoreAllEvents.On();

            var stopwatch = new HighStopwatch();
            stopwatch.Begin();

            foreach (int objId in targetIds)
            {
                try
                {
                    if (actionBuild.noneTool.GetPrefabDesc(objId).isStation && ShouldExcludeStations)
                    {
                        continue;
                    }

                    DoDismantleObject(actionBuild, objId);
                }
                catch (Exception e)
                {
                    BlueprintTweaksPlugin.logger.LogWarning($"Error while dismantling entity {objId}, message: {e.Message}, stacktrace:\n{e.StackTrace}");
                }
            }

            var durationInS = stopwatch.duration;
            BlueprintTweaksPlugin.logger.LogDebug($"Took {durationInS} s to delete entities");
        }

        [HarmonyPatch(typeof(PlayerAction_Build), nameof(PlayerAction_Build.DoDismantleObject))]
        [HarmonyReversePatch]
        public static bool DoDismantleObject(PlayerAction_Build instance, int objId)
        {
            return true;
        }


        public static void FastDeleteEntities(PlanetFactory factory, List<int> targetIds, List<int> edgeIds)
        {
            Player player = GameMain.mainPlayer;

            using IDisposable toggle = UndoManager.IgnoreAllEvents.On();

            FactorySystem factorySystem = factory.factorySystem;
            CargoTraffic cargoTraffic = factory.cargoTraffic;
            PowerSystem powerSystem = factory.powerSystem;

            // Close all the build tools, so we don't have to worry about BuildTool.buildPreview
            foreach (BuildTool buildTool in player.controller.actionBuild.tools)
            {
                if (!(buildTool is DragRemoveBuildTool))
                    buildTool._Close();
            }

            powerConRemoval.Clear();

            if (powerConRemoval.Capacity < powerSystem.netCursor)
                powerConRemoval.Capacity = powerSystem.netCursor;

            // Close inspect
            player.controller.actionInspect.InspectNothing();

            for (int i = 0; i < powerSystem.netCursor; i++)
            {
                int maxConsumers = 10;
                if (powerSystem.netPool[i] != null)
                {
                    maxConsumers = powerSystem.netPool[i].consumers.Count;
                }

                powerConRemoval.Add(new HashSet<int>(maxConsumers));
            }

            foreach (int itemId in LDB.items.dataIndices.Keys)
            {
                takeBackCount[itemId] = 0;
                takeBackInc[itemId] = 0;
            }

            PowerConsumerComponent[] consumerPool = powerSystem.consumerPool;
            InserterComponent[] inserterPool = factorySystem.inserterPool;
            EntityData[] entityPool = factory.entityPool;

            void RemoveConsumerComponent(int id)
            {
                ref PowerConsumerComponent powerCon = ref consumerPool[id];
                if (powerCon.id != 0)
                {
                    if (powerCon.networkId >= powerConRemoval.Count)
                    {
                        //Debug.Log($"Power Net: {powerCon.networkId} is not in the list");
                        return;
                    }

                    if (powerConRemoval[powerCon.networkId].Contains(id)) return;

                    powerConRemoval[powerCon.networkId].Add(id);
                    powerCon.SetEmpty();
                    powerSystem.consumerRecycle[powerSystem.consumerRecycleCursor] = id;
                    powerSystem.consumerRecycleCursor++;
                }
            }

            void FinishRemovingPowerNets()
            {
                for (int i = 0; i < powerSystem.netCursor; i++)
                {
                    PowerNetwork network = powerSystem.netPool[i];
                    if (network == null || network.id != i) continue;
                    if (network.id >= powerConRemoval.Count) continue;

                    HashSet<int> consumersToRemove = powerConRemoval[network.id];
                    foreach (Node node in network.nodes)
                        node.consumers.RemoveAll(x => consumersToRemove.Contains(x));
                    network.consumers.RemoveAll(x => consumersToRemove.Contains(x));
                }

                if (factory.planet.factoryModel != null)
                    factory.planet.factoryModel.RefreshPowerConsumers();
            }

            void DeleteInserter(int entityId)
            {
                void TakeBackItemsOptimized(ref InserterComponent inserter)
                {
                    if (inserter.itemId <= 0 || inserter.stackCount <= 0) return;

                    takeBackCount[inserter.itemId] += inserter.itemCount;
                    takeBackInc[inserter.itemId] += inserter.itemInc;
                }


                int inserterId = factory.entityPool[entityId].inserterId;
                if (inserterId == 0) return;
                if (inserterPool[inserterId].id != inserterId) return;

                TakeBackItemsOptimized(ref inserterPool[inserterId]);
            }

            void DeleteEdgeBelts()
            {
                foreach (int edgeId in edgeIds)
                {
                    if (edgeId > 0 && factory.entityPool[edgeId].id == edgeId)
                    {
                        int beltId = factory.entityPool[edgeId].beltId;
                        if (beltId > 0)
                        {
                            ReturnBuildingItems(edgeId);
                            factory.RemoveEntityWithComponents(edgeId, false);
                            targetIds.Remove(edgeId);
                        }
                    }
                }
            }

            BeltComponent[] beltPool = cargoTraffic.beltPool;

            void DeleteBelt(int entityId)
            {
                int beltId = factory.entityPool[entityId].beltId;
                if (beltId <= 0) return;

                ref BeltComponent belt = ref beltPool[beltId];

                if (belt.id == beltId)
                {
                    cargoTraffic.RemoveBeltRenderer(beltId);
                    cargoTraffic.RemoveCargoPath(belt.segPathId);
                    belt.SetEmpty();
                    cargoTraffic.beltRecycle[cargoTraffic.beltRecycleCursor] = beltId;
                    cargoTraffic.beltRecycleCursor++;

                    factory.entityPool[entityId].beltId = 0;
                }
            }

            void ReturnBuildingItems(int objId)
            {
                ItemProto itemProto = player.controller.actionBuild.noneTool.GetItemProto(objId);
                int id = itemProto?.ID ?? 0;
                if (objId > 0)
                {
                    int powerAccId = entityPool[objId].powerAccId;
                    if (powerAccId > 0 && powerSystem.accPool[powerAccId].curEnergy == powerSystem.accPool[powerAccId].maxEnergy)
                    {
                        ItemProto proto2 = LDB.items.Select(entityPool[objId].protoId);
                        if (proto2 != null && proto2.HeatValue == 0L)
                        {
                            int modelIndex = proto2.ModelIndex;
                            proto2 = LDB.items.Select(entityPool[objId].protoId + 1);
                            if (proto2 != null && proto2.HeatValue == powerSystem.accPool[powerAccId].maxEnergy && modelIndex == proto2.ModelIndex)
                            {
                                id++;
                            }
                        }
                    }
                }

                int ObjectAssetValue(int objId1)
                {
                    if (objId1 == 0)
                    {
                        return 0;
                    }

                    if (objId1 < 0 && factory.prebuildPool[-objId1].itemRequired > 0)
                    {
                        return 0;
                    }

                    return 1;
                }

                int num3 = ObjectAssetValue(objId);
                if (num3 > 0)
                {
                    player.TryAddItemToPackage(id, num3, 0, true, objId);
                }
            }

            void TakeBackOptimizedGeneral(int objId)
            {
                int spraycoaterId = factory.entityPool[objId].spraycoaterId;
                if (spraycoaterId > 0)
                {
                    factory.cargoTraffic.TakeBackItems_Spraycoater(player, spraycoaterId);
                }

                int pilerId = factory.entityPool[objId].pilerId;
                if (pilerId > 0)
                {
                    factory.cargoTraffic.TakeBackItems_Piler(player, pilerId);
                }

                int minerId = factory.entityPool[objId].minerId;
                if (minerId > 0)
                {
                    factory.factorySystem.TakeBackItems_Miner(player, minerId);
                }

                int assemblerId = factory.entityPool[objId].assemblerId;
                if (assemblerId > 0)
                {
                    factory.factorySystem.TakeBackItems_Assembler(player, assemblerId);
                }

                int fractionateId = factory.entityPool[objId].fractionatorId;
                if (fractionateId > 0)
                {
                    factory.factorySystem.TakeBackItems_Fractionator(player, fractionateId);
                }

                int ejectorId = factory.entityPool[objId].ejectorId;
                if (ejectorId > 0)
                {
                    factory.factorySystem.TakeBackItems_Ejector(player, ejectorId);
                }

                int siloId = factory.entityPool[objId].siloId;
                if (siloId > 0)
                {
                    factory.factorySystem.TakeBackItems_Silo(player, siloId);
                }

                int labId = factory.entityPool[objId].labId;
                if (labId > 0)
                {
                    factory.factorySystem.TakeBackItems_Lab(player, labId);
                }

                int storageId = factory.entityPool[objId].storageId;
                if (storageId > 0)
                {
                    factory.factoryStorage.TakeBackItems_Storage(player, storageId);
                }

                int tankId = factory.entityPool[objId].tankId;
                if (tankId > 0)
                {
                    factory.factoryStorage.TakeBackItems_Tank(player, tankId);
                }

                int stationId = factory.entityPool[objId].stationId;
                if (stationId > 0)
                {
                    factory.transport.TakeBackItems_Station(player, stationId);
                }

                int powerGenId = factory.entityPool[objId].powerGenId;
                if (powerGenId > 0)
                {
                    PowerGeneratorComponent powerGeneratorComponent = factory.powerSystem.genPool[powerGenId];
                    if (powerGeneratorComponent.fuelId > 0 && powerGeneratorComponent.fuelCount > 0)
                    {
                        int upCount = player.TryAddItemToPackage(powerGeneratorComponent.fuelId, powerGeneratorComponent.fuelCount,
                            powerGeneratorComponent.fuelInc, true, objId);
                        UIItemup.Up(powerGeneratorComponent.fuelId, upCount);
                        factory.powerSystem.genPool[powerGenId].fuelId = 0;
                        factory.powerSystem.genPool[powerGenId].fuelCount = 0;
                        factory.powerSystem.genPool[powerGenId].fuelInc = 0;
                    }

                    if (powerGeneratorComponent.gamma)
                    {
                        int productId = powerGeneratorComponent.productId;
                        int num = (int)powerGeneratorComponent.productCount;
                        if (productId != 0 && num > 0)
                        {
                            int upCount2 = player.TryAddItemToPackage(productId, num, 0, true, objId);
                            UIItemup.Up(productId, upCount2);
                            factory.powerSystem.genPool[powerGenId].productCount = 0f;
                        }

                        int catalystId = powerGeneratorComponent.catalystId;
                        int catalystPoint = powerGeneratorComponent.catalystPoint;
                        int catalystIncPoint = powerGeneratorComponent.catalystIncPoint;
                        int num2 = powerGeneratorComponent.catalystPoint % 3600;
                        int catalystIncPoint2 = factory.split_inc(ref catalystPoint, ref catalystIncPoint, num2);
                        int num3 = catalystPoint / 3600;
                        int inc = catalystIncPoint / 3600;
                        if (catalystId != 0 && num3 > 0)
                        {
                            factory.powerSystem.genPool[powerGenId].catalystPoint = num2;
                            factory.powerSystem.genPool[powerGenId].catalystIncPoint = catalystIncPoint2;
                            int upCount3 = player.TryAddItemToPackage(catalystId, num3, inc, true, objId);
                            UIItemup.Up(catalystId, upCount3);
                        }
                    }
                }

                int powerExcId = factory.entityPool[objId].powerExcId;
                if (powerExcId > 0)
                {
                    PowerExchangerComponent powerExchangerComponent = factory.powerSystem.excPool[powerExcId];
                    if (powerExchangerComponent.emptyCount > 0)
                    {
                        int emptyCount = powerExchangerComponent.emptyCount;
                        int upCount4 = player.TryAddItemToPackage(powerExchangerComponent.emptyId, emptyCount, 0, true, objId);
                        UIItemup.Up(powerExchangerComponent.emptyId, upCount4);
                        factory.powerSystem.excPool[powerExcId].emptyCount = 0;
                    }

                    if (powerExchangerComponent.fullCount > 0)
                    {
                        int fullCount = powerExchangerComponent.fullCount;
                        int upCount5 = player.TryAddItemToPackage(powerExchangerComponent.fullId, fullCount, 0, true, objId);
                        UIItemup.Up(powerExchangerComponent.fullId, upCount5);
                        factory.powerSystem.excPool[powerExcId].fullCount = 0;
                    }

                    factory.powerSystem.excPool[powerExcId].emptyCount = 0;
                    factory.powerSystem.excPool[powerExcId].fullCount = 0;
                }
            }


            cargoTraffic.container.ClearTempCargos();
            cargoTraffic.container.BeginTemp();
            captureTempCargos = true;

            HighStopwatch stopwatch = new HighStopwatch();
            stopwatch.Begin();

            DeleteEdgeBelts();

            foreach (int entityId in targetIds)
            {
                if (entityId > 0)
                {
                    if (factory.entityPool[entityId].id != entityId) continue;
                    if (factory.entityPool[entityId].stationId > 0 && ShouldExcludeStations)
                    {
                        continue;
                    }

                    DeleteBelt(entityId);
                    DeleteInserter(entityId);

                    if (entityPool[entityId].powerConId != 0)
                    {
                        // Help remove the power consumers before removing the entity
                        RemoveConsumerComponent(entityPool[entityId].powerConId);
                    }

                    ReturnBuildingItems(entityId);
                    try
                    {
                        TakeBackOptimizedGeneral(entityId);
                        factory.RemoveEntityWithComponents(entityId, false);
                    }
                    catch (Exception e)
                    {
                        BlueprintTweaksPlugin.logger.LogWarning(
                            $"Error while dismantling entity {entityId}, message: {e.Message}, stacktrace:\n{e.StackTrace}");
                    }
                }
                else if (entityId < 0)
                {
                    int prebuildId = -entityId;
                    if (factory.prebuildPool[prebuildId].id == 0) continue;

                    ReturnBuildingItems(entityId);
                    try
                    {
                        factory.RemovePrebuildWithComponents(prebuildId);
                    }
                    catch (Exception e)
                    {
                        BlueprintTweaksPlugin.logger.LogWarning(
                            $"Error while dismantling prebuild {prebuildId}, message: {e.Message}, stacktrace:\n{e.StackTrace}");
                    }
                }
            }

            FinishRemovingPowerNets();

            cargoTraffic.container.EndTemp();
            captureTempCargos = false;

            double durationInS;
            if (NebulaModAPI.IsMultiplayerActive)
            {
                IMultiplayerSession session = NebulaModAPI.MultiplayerSession;
                if (session.LocalPlayer.IsHost &&
                    session.Factories.IsIncomingRequest.Value)
                {
                    try
                    {
                        session.Network.SendToMatching(
                            new ReturnItemsPacket(takeBackCount, takeBackInc),
                            nebulaPlayer => nebulaPlayer.Id == session.Factories.PacketAuthor
                        );
                    }
                    catch (InvalidOperationException)
                    {
                        BlueprintTweaksPlugin.logger.LogWarning($"Error sending return items to player ID: {session.Factories.PacketAuthor}!");
                    }


                    durationInS = stopwatch.duration;
                    BlueprintTweaksPlugin.logger.LogDebug($"Took {durationInS} s to fast delete");
                    return;
                }
            }

            foreach (KeyValuePair<int, int> kvp in takeBackCount)
                player.TryAddItemToPackage(kvp.Key, kvp.Value, takeBackInc[kvp.Key], true);

            durationInS = stopwatch.duration;
            BlueprintTweaksPlugin.logger.LogDebug($"Took {durationInS} s to fast delete");
        }
    }
}