using System.IO;

namespace BlueprintTweaks
{
    public static class ReformExtensions
    {
        public static void Export(this ReformData data, BinaryWriter w)
        {
            w.Write((byte) 0);
            w.Write(data.areaIndex);
            w.Write(data.type);
            w.Write(data.color);
            w.Write(data.localLatitude);
            w.Write(data.localLongitude);
        }

        public static void Import(this ReformData data, BinaryReader r)
        {
            r.ReadByte();
            data.areaIndex = r.ReadInt32();
            data.type = r.ReadInt32();
            data.color = r.ReadInt32();
            data.localLatitude = r.ReadSingle();
            data.localLongitude = r.ReadSingle();
        }
    }
}