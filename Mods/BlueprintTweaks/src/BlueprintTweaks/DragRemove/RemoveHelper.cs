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
    public static class RemoveHelper
    {
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
                    session.Network.SendPacket(new RemoveRequestPacket(planetId, targetIds.ToArray(), edgeIds.ToArray(),
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
            }
            
            RegularDeleteEntities(factory, targetIds);
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
                    session.Network.SendPacket(new RemoveRequestPacket(planetId, objectIds.ToArray(), Array.Empty<int>(),
                        session.Factories.PacketAuthor == NebulaModAPI.AUTHOR_NONE
                            ? session.LocalPlayer.Id
                            : session.Factories.PacketAuthor, false, ShouldExcludeStations));
                }

                if (!session.LocalPlayer.IsHost && !session.Factories.IsIncomingRequest.Value)
                {
                    return;
                }
            }
          
            RegularDeleteEntities(factory, objectIds);
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
    }
}