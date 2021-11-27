using BepInEx.Configuration;
using CommonAPI;
using CommonAPI.Systems;
using UnityEngine;

namespace FasterMachines
{
    public class SmelterDesc : ConfigurableDesc
    {
        public override string configCategory => "Smelter";
        public override ConfigFile modConfig => BetterMachinesPlugin.config;

        [UseConfigFile("Smelting speed multiplier for this tier. Mk1 smelter has multiplier of 1.")]
        public float smeltSpeed;

        public override void ApplyProperties(PrefabDesc desc)
        {
            base.ApplyProperties(desc);
            
            desc.assemblerSpeed = Mathf.RoundToInt(smeltSpeed * 10000f);
            desc.assemblerRecipeType = ERecipeType.Smelt;
            desc.isAssembler = true;
        }
    }
}