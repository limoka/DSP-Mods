using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace BlueprintTweaks.GlobalPatch
{
    [HarmonyPatch]
    public class BuildTool_BlueprintPaste_Patch_3
    {
        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), nameof(BuildTool_BlueprintPaste.OnCameraPostRender))]
        [HarmonyPrefix]
        public static bool CheckNull(BuildTool_BlueprintPaste __instance)
        {
            return __instance.active;
        }
    }
}