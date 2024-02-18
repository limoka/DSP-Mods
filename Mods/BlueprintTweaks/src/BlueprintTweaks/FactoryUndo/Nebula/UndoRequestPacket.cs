using NebulaAPI;
using NebulaAPI.DataStructures;
using NebulaAPI.GameState;
using NebulaAPI.Networking;
using NebulaAPI.Packets;

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
                if (UndoManager.undos[(ushort)packet.AuthorId].TryUndo(factory, out string message, out bool sound))
                {
                    conn.SendPacket(new ActionResultPacket(message, sound));
                }
            }
        }
    }
}