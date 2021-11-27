using BepInEx.Configuration;
using CommonAPI;
using CommonAPI.Systems;

namespace FasterMachines
{
    public class CustomStorageDesc : ConfigurableDesc
    {
        public override string configCategory => "Storage";
        public override ConfigFile modConfig => BetterMachinesPlugin.config;

        [UseConfigFile("How many columns will this tier have. Mk1 Storage has 10 columns")]
        public int columnCount;
        [UseConfigFile("How many rows will this tier have. Mk1 Storage has 3 rows, Mk2 Storage has 6 rows.")]
        public int rowCount;
        
        
        public override void ApplyProperties(PrefabDesc desc)
        {
            base.ApplyProperties(desc);
            
            desc.storageCol = columnCount;
            desc.storageRow = rowCount;
            desc.isStorage = desc.storageCol * desc.storageRow > 0;
        }
    }
}