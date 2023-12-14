using System.Collections.Generic;
using BlueprintTweaks.FactoryUndo.Nebula;
using NebulaAPI;

namespace BlueprintTweaks.FactoryUndo
{
    public static class UndoManager
    {
        public static Dictionary<ushort, PlayerUndo> undos = new Dictionary<ushort, PlayerUndo>();
        public static ToggleSwitch IgnoreAllEvents = new ToggleSwitch();

        #region GLOBAL

        public static void Init()
        {
            if (NebulaModAPI.NebulaIsInstalled)
            {
                NebulaModAPI.OnPlayerJoinedGame += OnPlayerJoined;
                NebulaModAPI.OnPlayerLeftGame += OnPlayerLeft;
                NebulaModAPI.OnMultiplayerGameStarted += OnGameStarted;
            }

            ushort localId = GetLocalUserId();
            undos.Add(localId, new PlayerUndo(localId));
        }

        public static ushort GetLocalUserId()
        {
            if (NebulaModAPI.IsMultiplayerActive)
            {
                return NebulaModAPI.MultiplayerSession.LocalPlayer.Id;
            }

            return 1;
        }
        
        public static PlayerUndo GetPlayerData(int playerId)
        {
            return undos[(ushort)playerId];
        }

        public static PlayerUndo GetCurrentPlayerData()
        {
            if (NebulaModAPI.IsMultiplayerActive)
            {
                IFactoryManager factoryManager = NebulaModAPI.MultiplayerSession.Factories;
                if (factoryManager.IsIncomingRequest.Value)
                {
                    ushort playerId = (ushort)factoryManager.PacketAuthor;
                    return undos[playerId];
                }
            }
            
            return undos[GetLocalUserId()];
        }

        #endregion

        #region SERVER

        public static void OnPlayerJoined(IPlayerData player)
        {
            undos.Add(player.PlayerId, new PlayerUndo(player.PlayerId));
        }

        public static void OnPlayerLeft(IPlayerData player)
        {
            undos.Remove(player.PlayerId);
        }

        public static void OnGameStarted()
        {
            IMultiplayerSession session = NebulaModAPI.MultiplayerSession;
            if (session.LocalPlayer.IsHost)
            {
                undos.Clear();
                ushort localId = GetLocalUserId();
                undos.Add(localId, new PlayerUndo(localId));
            }
        }

        #endregion

        #region CLIENT

        public static void TryUndo()
        {
            string message;
            bool sound;
            PlanetFactory factory = GameMain.localPlanet.factory;
            
            if (NebulaModAPI.IsMultiplayerActive)
            {
                IMultiplayerSession session = NebulaModAPI.MultiplayerSession;
                if (session.LocalPlayer.IsHost)
                {
                   
                    if (undos[GetLocalUserId()].TryUndo(factory, out message, out sound))
                    {
                        UIRealtimeTip.Popup(message.Translate(), sound);
                    }
                }
                else
                {
                    session.Network.SendPacket(new UndoRequestPacket(GameMain.localPlanet?.id ?? -1, session.LocalPlayer.Id));
                }
                return;
            }

            if (undos[GetLocalUserId()].TryUndo(factory, out message, out sound))
            {
                UIRealtimeTip.Popup(message.Translate(), sound);
            }
        }

        public static void TryRedo()
        {
            string message;
            bool sound;
            PlanetFactory factory = GameMain.localPlanet.factory;
            
            if (NebulaModAPI.IsMultiplayerActive)
            {
                IMultiplayerSession session = NebulaModAPI.MultiplayerSession;
                if (session.LocalPlayer.IsHost)
                {
                    if (undos[GetLocalUserId()].TryRedo(factory, out message, out sound))
                    {
                        UIRealtimeTip.Popup(message.Translate(), sound);
                    }
                }
                else
                {
                    session.Network.SendPacket(new RedoRequestPacket(GameMain.localPlanet?.id ?? -1, session.LocalPlayer.Id));
                }
                return;
            }

            if (undos[GetLocalUserId()].TryRedo(factory, out message, out sound))
            {
                UIRealtimeTip.Popup(message.Translate(), sound);
            }
        }

        #endregion
    }
}