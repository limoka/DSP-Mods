using BepInEx.Configuration;
using CommonAPI.Systems;
using UnityEngine;

namespace FasterMachines
{
    public class CustomAssemblerDesc : ConfigurableDesc
    {
        public override string configCategory => "Assembler";
        public override ConfigFile modConfig => BetterMachinesPlugin.config;

        [UseConfigFile("Assembler processing speed multiplier for this tier. Mk1 Assembler has multiplier of 0.75")]
        public float assembleSpeed;

        public override void ApplyProperties(PrefabDesc desc)
        {
            base.ApplyProperties(desc);

            desc.assemblerSpeed = Mathf.RoundToInt(assembleSpeed * 10000f);
            desc.assemblerRecipeType = ERecipeType.Assemble;
            desc.isAssembler = true;
        }
    }
}