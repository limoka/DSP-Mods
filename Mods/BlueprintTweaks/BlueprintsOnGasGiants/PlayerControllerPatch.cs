using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
// ReSharper disable InconsistentNaming

namespace BlueprintTweaks
{
    [HarmonyPatch]
    public static class PlayerControllerPatch
    {
        
        [HarmonyPatch(typeof(PlayerController), "OpenBlueprintCopyMode")]
        [HarmonyPatch(typeof(PlayerController), "OpenBlueprintPasteMode")]
        [HarmonyPatch(typeof(BuildTool_BlueprintCopy), "DetermineActive")]
        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), "CheckBuildConditionsPrestage")]
        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), "DetermineActive")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> RemoveGasGiantBlocks(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlanetData), nameof(PlanetData.gasItems)))
                ).Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Pop))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_0))
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlanetData), nameof(PlanetData.gasItems)))
                ).Advance(2)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Pop))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_0));
            
            return matcher.InstructionEnumeration();
        }
        
        [HarmonyPatch(typeof(BuildTool_BlueprintCopy), "UpdateRaycast")]
        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), "UpdateRaycast")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> ChangeCastDistance(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldc_R4, 800f)
                )
                .SetInstruction(Transpilers.EmitDelegate<Func<float>>(() => GameCamera.instance.blueprintPoser.planetRadius * 2.4f));
                
            
            return matcher.InstructionEnumeration();
        }
        
        
        [HarmonyPatch(typeof(PlayerController), "OpenBlueprintPasteMode")]
        [HarmonyPrefix]
        public static bool CheckBpForGasGiants(PlayerController __instance, BlueprintData blueprint)
        {
            if (GameMain.history.blueprintLimit == 0)
            {
                return false;
            }
            if (blueprint == null)
            {
                blueprint = __instance.actionBuild.blueprintClipboard;
            }
            if (BlueprintData.IsNullOrEmpty(blueprint))
            {
                return false;
            }

            if (__instance.gameData.localPlanet?.gasItems == null || __instance.gameData.localPlanet.gasItems.Length == 0) return true;
            
            foreach (BlueprintBuilding building in blueprint.buildings)
            {
                ItemProto item = LDB.items.Select(building.itemId);
                if (!item.BuildInGas)
                {
                    UIRealtimeTip.Popup("CantPasteThisInGasGiantWarn".Translate());
                    return false;
                }
            }

            return true;
        }
        
        [HarmonyPatch(typeof(PlayerController), "OpenBlueprintCopyMode")]
        [HarmonyPatch(typeof(PlayerController), "OpenBlueprintPasteMode")]
        [HarmonyPostfix]
        public static void OnBPOpenOnGasGiants(PlayerController __instance)
        {
            GameCamera.instance.blueprintPoser.planetRadius = __instance.gameData.localPlanet.realRadius;
        }
    }
}