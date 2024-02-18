using NebulaAPI;
using NebulaAPI.Networking;
using NebulaAPI.Packets;

namespace BlueprintTweaks.FactoryUndo.Nebula
{
    public class ActionResultPacket
    {
        public string message { get; set; }
        public bool sound { get; set; }
        
        public ActionResultPacket() {}

        public ActionResultPacket(string message, bool sound)
        {
            this.message = message;
            this.sound = sound;
        }

        [RegisterPacketProcessor]
        public class ActionResultProcessor : BasePacketProcessor<ActionResultPacket>
        {
            public override void ProcessPacket(ActionResultPacket packet, INebulaConnection conn)
            {
                UIRealtimeTip.Popup(packet.message.Translate(), packet.sound);
            }
        }
    }
}