using BepInEx.Configuration;
using CommonAPI;
using CommonAPI.Systems;
using UnityEngine;

namespace AdvancedMiner
{
    public class CustomMinerDesc : ConfigurableDesc
    {
        public override string configCategory => "AdvancedMiners";
        public override ConfigFile modConfig => DSPAdvancedMiner.config;
        
        [UseConfigFile("How much range this tier has(Range of miner mk.1 is 7.75m). Note that this applies only to new miners built, already existing will not have their range changed!")]
        public float miningRange = 7.75f;
        
        [UseConfigFile("Mining speed multiplier for this tier. Note that this applies only to new miners built, already existing will not have their speed changed!")]
        public float miningSpeed = 1f;
        
        public const string MINER_RANGE_NAME = DSPAdvancedMiner.MODID + ":logicNodes";
        
        public override void ApplyProperties(PrefabDesc desc)
        {
            base.ApplyProperties(desc);
            
            desc.SetProperty(MINER_RANGE_NAME, miningRange);
            float period = desc.minerPeriod / miningSpeed;
            desc.minerPeriod = Mathf.RoundToInt(period);
        }
    }
}