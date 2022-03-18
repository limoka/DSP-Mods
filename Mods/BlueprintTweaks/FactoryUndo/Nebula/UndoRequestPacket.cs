using NebulaAPI;

namespace BlueprintTweaks.FactoryUndo.Nebula
{
    public class UndoRequestPacket : IFactoryPacket
    {
        public int PlanetId { get; set; }
        public int AuthorId { get; set; }

        public UndoRequestPacket() { }

        public UndoRequestPacket(int planetId, int playerId)
        {
            AuthorId = playerId;
            PlanetId = planetId;
        }

        [RegisterPacketProcessor]
        public class UndoRequestHandler : RemoteFactoryProcessor<UndoRequestPacket>
        {
            public override void ProcessPacket(PlanetFactory factory, PlayerAction_Build actionBuild, UndoRequestPacket packet, INebulaConnection conn)
            {
                INebulaPlayer player = NebulaModAPI.MultiplayerSession.Network.PlayerManager.GetPlayer(conn);
                if (UndoManager.undos[player.Id].TryUndo(out string message, out bool sound))
                {
                    conn.SendPacket(new ActionResultPacket(message, sound));
                }
            }
        }
    }
}