using NebulaAPI;

namespace BlueprintTweaks.FactoryUndo.Nebula
{
    public interface IFactoryPacket
    {
        public int PlanetId { get; }
        public int AuthorId { get; }
    }
    
    public abstract class RemoteFactoryProcessor<T> : BasePacketProcessor<T>
    where T : IFactoryPacket
    {
        public abstract void ProcessPacket(PlanetFactory factory, PlayerAction_Build actionBuild, T packet, INebulaConnection conn);
        
        public override void ProcessPacket(T packet, INebulaConnection conn)
        {
            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);
            if (planet.factory == null)
            {
                return;
            }

            PlayerAction_Build pab = GameMain.mainPlayer.controller != null ? GameMain.mainPlayer.controller.actionBuild : null;
            BuildTool_BlueprintPaste buildTool = pab.blueprintPasteTool;
            BuildTool_None toolNone = pab.noneTool;

            IFactoryManager factoryManager = NebulaModAPI.MultiplayerSession.Factories;

            if (buildTool == null) return;
            
            factoryManager.TargetPlanet = packet.PlanetId;
            factoryManager.PacketAuthor = packet.AuthorId;

            PlanetFactory tmpFactory = null;
            bool loadExternalPlanetData = GameMain.localPlanet?.id != planet.id;

            if (loadExternalPlanetData)
            {
                tmpFactory = buildTool.factory;
                factoryManager.AddPlanetTimer(packet.PlanetId);
            }

            factoryManager.EventFactory = planet.factory;

            buildTool.factory = planet.factory;
            toolNone.factory = planet.factory;
            pab.factory = planet.factory;

            using (factoryManager.IsIncomingRequest.On())
            {
                ProcessPacket(planet.factory, pab, packet, conn);
            }

            if (loadExternalPlanetData)
            {
                buildTool.factory = tmpFactory;
                toolNone.factory = tmpFactory;
                pab.factory = tmpFactory;
            }

            GameMain.mainPlayer.mecha.buildArea = Configs.freeMode.mechaBuildArea;
            factoryManager.EventFactory = null;

            factoryManager.TargetPlanet = NebulaModAPI.PLANET_NONE;
            factoryManager.PacketAuthor = NebulaModAPI.AUTHOR_NONE;
        }
    }
}