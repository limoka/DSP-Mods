using NebulaAPI;

namespace BlueprintTweaks.FactoryUndo.Nebula
{
    public class ClearUndoRequestPacket
    {
        public int AuthorId { get; set; }
        
        public ClearUndoRequestPacket() {}

        public ClearUndoRequestPacket(int authorId)
        {
            AuthorId = authorId;
        }
        
        [RegisterPacketProcessor]
        public class ClearUndoRequestProcessor : BasePacketProcessor<ClearUndoRequestPacket>
        {
            public override void ProcessPacket(ClearUndoRequestPacket packet, INebulaConnection conn)
            {
                if (NebulaModAPI.MultiplayerSession.LocalPlayer.IsHost)
                {
                    PlayerUndo data = UndoManager.GetPlayerData(packet.AuthorId);
                    using (NebulaModAPI.MultiplayerSession.Factories.IsIncomingRequest.On())
                    {
                        data.ResetUndo();
                    }
                }   
            }
        }
    }
}