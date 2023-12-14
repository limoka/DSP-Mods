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

        /*[HarmonyPatch(typeof(BlueprintUtils), "GenerateBlueprintData")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> AddMoreData(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher matcher = new CodeMatcher(instructions);
            matcher.MatchForward(false
                , new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(AssemblerComponent), nameof(FactorySystem.assemblerPool)))
                , new CodeMatch(OpCodes.Ldloc_S)
                , new CodeMatch(OpCodes.Ldelema)
                , new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(AssemblerComponent), nameof(AssemblerComponent.id)))
            );

            matcher
                .RemoveInstructions(5)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Pop))

            matcher.InsertAndAdvance(Transpilers.EmitDelegate<Func<FactorySystem, bool>>((system) =>
            {
                if (assemblerComponentEx.assemblerNextIds[_this.factory.index][_this.assemblerPool[j].id] != 0)
                {
                    int entityId2 = _this.assemblerPool[j].entityId;
                    int entityRootId = _this.assemblerPool[assemblerComponentEx.assemblerNextIds[_this.factory.index][_this.assemblerPool[j].id]].entityId;
                    float num16 = networkServes[consumerPool[_this.assemblerPool[j].pcId].networkId];
                    entityAnimPool[entityId2].state = entityAnimPool[entityRootId].state;
                    entityAnimPool[entityId2].power = entityAnimPool[entityRootId].power;
                    entityAnimPool[entityId2].time = entityAnimPool[entityRootId].time;
                    entitySignPool[entityId2].signType = entitySignPool[entityRootId].signType;
                }
                else if (this.assemblerPool[j].id == j)
                {
                    return true;
                }
            }));

            return matcher.InstructionEnumeration();
        }*/
    }
}