using BepInEx.Configuration;
using CommonAPI;
using CommonAPI.Systems;

namespace FasterMachines
{
    public class LiquidTankDesc : ConfigurableDesc
    {
        public override string configCategory => "LiquidTank";
        public override ConfigFile modConfig => BetterMachinesPlugin.config;

        [UseConfigFile("How much capacity will this tier have. Mk1 Liquid Tank has capacity of 10000.")]
        public int fluidCapacity;

        public override void ApplyProperties(PrefabDesc desc)
        {
            base.ApplyProperties(desc);
            
            desc.isTank = true;
            desc.fluidStorageCount = fluidCapacity;
        }
    }
}