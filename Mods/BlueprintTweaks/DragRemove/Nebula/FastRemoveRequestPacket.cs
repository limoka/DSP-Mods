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

        public FastRemoveRequestPacket() { }
        public FastRemoveRequestPacket(int planetId, int[] objIds, int[] edgeObjIds, int authorId, bool variant)
        {
            AuthorId = authorId;
            PlanetId = planetId;
            ObjIds = objIds;
            EdgeObjIds = edgeObjIds;
            UseEdgeVariant = variant;
        }
        
        [RegisterPacketProcessor]
        public class FastRemoveRequestHandler : RemoteFactoryProcessor<FastRemoveRequestPacket>
        {
            public override void ProcessPacket(PlanetFactory factory, PlayerAction_Build actionBuild, FastRemoveRequestPacket packet, INebulaConnection conn)
            {
                if (packet.UseEdgeVariant)
                {
                    FastRemoveHelper.SwitchDelete(factory, packet.ObjIds.ToList(), packet.EdgeObjIds.ToList());
                }
                else
                {
                    FastRemoveHelper.SwitchDelete(factory, packet.ObjIds.ToList());
                }
            }
        }
    }
}