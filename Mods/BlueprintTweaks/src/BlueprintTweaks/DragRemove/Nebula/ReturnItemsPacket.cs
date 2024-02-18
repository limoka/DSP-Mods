using System.Collections.Generic;
using System.Linq;
using NebulaAPI;
using NebulaAPI.Interfaces;
using NebulaAPI.Networking;
using NebulaAPI.Packets;

namespace BlueprintTweaks.Nebula
{
    public struct ItemPackageL
    {
        public int stack;
        public int item;
        public int inc;

        public ItemPackageL(int _item, int _stack, int _inc)
        {
            stack = _stack;
            item = _item;
            inc = _inc;
        }
    }


    public class ReturnItemsPacket
    {
        public int dataCount { get; set; }
        public byte[] data { get; set; }

        public ReturnItemsPacket() { }

        public ReturnItemsPacket(Dictionary<int, int> counts, Dictionary<int, int> incCounts)
        {
            using IWriterProvider writer = NebulaModAPI.GetBinaryWriter();
            dataCount = counts.Count(pair => pair.Value > 0);

            foreach (var pair in counts)
            {
                if (pair.Value > 0)
                {
                    writer.BinaryWriter.Write(pair.Key);
                    writer.BinaryWriter.Write(pair.Value);
                    writer.BinaryWriter.Write(incCounts[pair.Key]);
                }
            }

            data = writer.CloseAndGetBytes();
        }

        public List<ItemPackageL> GetData()
        {
            List<ItemPackageL> items = new List<ItemPackageL>(dataCount);

            using IReaderProvider reader = NebulaModAPI.GetBinaryReader(data);

            for (int i = 0; i < dataCount; i++)
            {
                int itemId = reader.BinaryReader.ReadInt32();
                int count = reader.BinaryReader.ReadInt32();
                int inc = reader.BinaryReader.ReadInt32();
                items.Add(new ItemPackageL(itemId, count, inc));
            }

            return items;
        }

        [RegisterPacketProcessor]
        public class ReturnItemsHandler : BasePacketProcessor<ReturnItemsPacket>
        {
            public override void ProcessPacket(ReturnItemsPacket packet, INebulaConnection conn)
            {
                List<ItemPackageL> items = packet.GetData();
                foreach (ItemPackageL item in items)
                {
                    int upCount = GameMain.mainPlayer.TryAddItemToPackage(item.item, item.stack, item.stack, true);
                    UIItemup.Up(item.item, upCount);
                }
            }
        }
    }
}