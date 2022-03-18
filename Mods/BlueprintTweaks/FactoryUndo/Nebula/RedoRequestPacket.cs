using NebulaAPI;

namespace BlueprintTweaks.FactoryUndo.Nebula
{
    public class RedoRequestPacket : IFactoryPacket
    {
        public int PlanetId { get; set; }
        public int AuthorId { get; set; }

        public RedoRequestPacket() { }

        public RedoRequestPacket(int planetId, int playerId)
        {
            AuthorId = playerId;
            PlanetId = planetId;
        }

        [RegisterPacketProcessor]
        public class RedoRequestHandler : RemoteFactoryProcessor<RedoRequestPacket>
        {
            public override void ProcessPacket(PlanetFactory factory, PlayerAction_Build actionBuild, RedoRequestPacket packet, INebulaConnection conn)
            {
                INebulaPlayer player = NebulaModAPI.MultiplayerSession.Network.PlayerManager.GetPlayer(conn);
                if (UndoManager.undos[player.Id].TryRedo(out string message, out bool sound))
                {
                    conn.SendPacket(new ActionResultPacket(message, sound));
                }
            }
        }
    }
}