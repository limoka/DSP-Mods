using System.Linq;
using BlueprintTweaks.FactoryUndo;
using BlueprintTweaks.FactoryUndo.Nebula;
using NebulaAPI;

namespace BlueprintTweaks.Nebula
{
    public class FastRemoveRequestPacket : IFactoryPacket
    {
        public int PlanetId { get; set; }
        public int[] ObjIds { get; set; }
        public int[] EdgeObjIds { get; set; }
        public int AuthorId { get; set; }
        public bool UseEdgeVariant { get; set; }
        public bool ExcludeStations { get; set; }

        public FastRemoveRequestPacket() { }
        public FastRemoveRequestPacket(int planetId, int[] objIds, int[] edgeObjIds, int authorId, bool variant, bool excludeStations)
        {
            AuthorId = authorId;
            PlanetId = planetId;
            ObjIds = objIds;
            EdgeObjIds = edgeObjIds;
            UseEdgeVariant = variant;
            ExcludeStations = excludeStations;
        }
        
        [RegisterPacketProcessor]
        public class FastRemoveRequestHandler : RemoteFactoryProcessor<FastRemoveRequestPacket>
        {
            public override void ProcessPacket(PlanetFactory factory, PlayerAction_Build actionBuild, FastRemoveRequestPacket packet, INebulaConnection conn)
            {
                FastRemoveHelper.excludeStationOverride = packet.ExcludeStations;
                if (packet.UseEdgeVariant)
                {
                    FastRemoveHelper.SwitchDelete(factory, packet.ObjIds.ToList(), packet.EdgeObjIds.ToList());
                }
                else
                {
                    FastRemoveHelper.SwitchDelete(factory, packet.ObjIds.ToList());
                }
                FastRemoveHelper.excludeStationOverride = false;
            }
        }
    }
}