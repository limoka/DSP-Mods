using System.IO;

namespace BlueprintTweaks.BlueprintDataSaveEdit
{
    public interface ICustomBlueprintDataSerializer
    {
        void Export(BlueprintData blueprint, BinaryWriter writer);
        void Import(BlueprintData blueprint, BinaryReader reader);
    }
}