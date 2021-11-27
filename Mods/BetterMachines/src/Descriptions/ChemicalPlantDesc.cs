using BepInEx.Configuration;
using CommonAPI;
using CommonAPI.Systems;
using UnityEngine;

namespace FasterMachines
{
    public class ChemicalPlantDesc : ConfigurableDesc
    {
        public override string configCategory => "ChemicalPlant";
        public override ConfigFile modConfig => BetterMachinesPlugin.config;

        [UseConfigFile("Chemical plant processing speed multiplier for this tier. Mk1 Chemical Plant has multiplier of 1.")]
        public float processingSpeed;

        public override void ApplyProperties(PrefabDesc desc)
        {
            base.ApplyProperties(desc);
            
            desc.assemblerSpeed = Mathf.RoundToInt(processingSpeed * 10000f);
            desc.assemblerRecipeType = ERecipeType.Chemical;
            desc.isAssembler = true;
        }
    }
}