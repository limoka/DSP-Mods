using System.Linq;
using BlueprintTweaks.FactoryUndo;
using BlueprintTweaks.FactoryUndo.Nebula;
using NebulaAPI;
using NebulaAPI.Networking;
using NebulaAPI.Packets;

namespace BlueprintTweaks.Nebula
{
    public class RemoveRequestPacket : IFactoryPacket
    {
        public int PlanetId { get; set; }
        public int[] ObjIds { get; set; }
        public int[] EdgeObjIds { get; set; }
        public int AuthorId { get; set; }
        public bool UseEdgeVariant { get; set; }
        public bool ExcludeStations { get; set; }

        public RemoveRequestPacket() { }
        public RemoveRequestPacket(int planetId, int[] objIds, int[] edgeObjIds, int authorId, bool variant, bool excludeStations)
        {
            AuthorId = authorId;
            PlanetId = planetId;
            ObjIds = objIds;
            EdgeObjIds = edgeObjIds;
            UseEdgeVariant = variant;
            ExcludeStations = excludeStations;
        }
        
        [RegisterPacketProcessor]
        public class RemoveRequestHandler : RemoteFactoryProcessor<RemoveRequestPacket>
        {
            public override void ProcessPacket(PlanetFactory factory, PlayerAction_Build actionBuild, RemoveRequestPacket packet, INebulaConnection conn)
            {
                RemoveHelper.excludeStationOverride = packet.ExcludeStations;
                if (packet.UseEdgeVariant)
                {
                    RemoveHelper.SwitchDelete(factory, packet.ObjIds.ToList(), packet.EdgeObjIds.ToList());
                }
                else
                {
                    RemoveHelper.SwitchDelete(factory, packet.ObjIds.ToList());
                }
                RemoveHelper.excludeStationOverride = false;
            }
        }
    }
}