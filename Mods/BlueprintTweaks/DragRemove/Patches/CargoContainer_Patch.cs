using CommonAPI;
using HarmonyLib;

namespace BlueprintTweaks
{
    [RegisterPatch(BlueprintTweaksPlugin.DRAG_REMOVE)]
    public static class CargoContainer_Patch
    {
        [HarmonyPatch(typeof(CargoContainer), nameof(CargoContainer.AddTempCargo))]
        [HarmonyPrefix]
        public static bool CaptureTempCargos(byte stack, short item, int inc)
        {
            if (!FastRemoveHelper.captureTempCargos) return true;

            if (item == 0) return false;
            
            FastRemoveHelper.takeBackCount[item] += stack;
            FastRemoveHelper.takeBackInc[item] += inc;
            
            return false;
        }
    }
}