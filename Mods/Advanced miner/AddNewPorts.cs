using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using CommonAPI;
using HarmonyLib;
using UnityEngine;

namespace AdvancedMiner
{
    [HarmonyPatch]
    public static class AddNewPorts
    {
        [HarmonyPatch(typeof(MinerComponent), "InternalUpdate")]
        [HarmonyPostfix]
        public static void OutputToSecondPort(ref MinerComponent __instance, PlanetFactory factory)
        {
            if (__instance.productId <= 0) return;

            for (int slotId = __instance.lastUsedPort; slotId < 3; slotId++)
            {
                if (TryInsertItem(ref __instance, factory, slotId)) return;
            }

            if (__instance.lastUsedPort != 0)
            {
                for (int slotId = 0; slotId < __instance.lastUsedPort; slotId++)
                {
                    if (TryInsertItem(ref __instance, factory, slotId)) return;
                }
            }

            __instance.lastUsedPort = 0;
        }

        private static bool TryInsertItem(ref MinerComponent __instance, PlanetFactory factory, int slotId)
        {
            int insertTarget = GetInsertTarget(__instance, slotId);
            if (insertTarget <= 0) return false;
            
            if (__instance.productCount > 0)
            {
                if (!factory.InsertInto(insertTarget, 0, __instance.productId)) return false;

                __instance.productCount--;
                if (__instance.productCount == 0)
                {
                    __instance.productId = 0;
                }
                return false;
            }

            __instance.lastUsedPort = slotId;
            return true;
        }

        [HarmonyPatch(typeof(MinerComponent), "InternalUpdate")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SplitPortOutput(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .End()
                .MatchBack(false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(MinerComponent), nameof(MinerComponent.productCount))),
                    new CodeMatch(OpCodes.Ldc_I4_0)
                )
                .Advance(1)
                .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Pop))
                .RemoveInstruction()
                .SetOpcodeAndAdvance(OpCodes.Br);
                
                
                
            return matcher.InstructionEnumeration();
        }
        
        public static void SetMinerInsertTarget(FactorySystem system, int slotId, int minerId, int insertTarget)
        {
            if (slotId == 0)
            {
                system.SetMinerInsertTarget(minerId, insertTarget);
                return;
            }

            if (minerId == 0 || system.minerPool[minerId].id != minerId) return;
            if (insertTarget > 0 && system.factory.entityPool[insertTarget].id == insertTarget)
            {
                if (slotId == 1)
                {
                    system.minerPool[minerId].insertTarget2 = insertTarget;
                }
                else
                {
                    system.minerPool[minerId].insertTarget3 = insertTarget;
                }
                return;
            }

            if (slotId == 1)
            {
                system.minerPool[minerId].insertTarget2 = 0;
            }
            else
            {
                system.minerPool[minerId].insertTarget3 = 0;
            }
        }
        
        public static int GetInsertTarget(FactorySystem system, int slotId, int minerId)
        {
            return GetInsertTarget(system.minerPool[minerId], slotId);
        }

        public static int GetInsertTarget(MinerComponent miner, int slotId)
        {
            if (slotId == 0)
            {
                return miner.insertTarget;
            }
            if (slotId == 1)
            {
                return miner.insertTarget2;
            }
            if (slotId == 2)
            {
                return miner.insertTarget3;
            }

            return 0;
        }
        
        [HarmonyPatch(typeof(MinerComponent), "SetEmpty")]
        [HarmonyPostfix]
        public static void SetEmpty(ref MinerComponent __instance)
        {
            __instance.insertTarget2 = 0;
        }
        
        [HarmonyPatch(typeof(PlanetFactory), "ApplyEntityDisconnection")]
        [HarmonyPrefix]
        public static bool ApplyEntityDisconnection(PlanetFactory __instance, int otherEntityId, int removingEntityId, int otherSlotId, int removingSlotId)
        {
            int minerId = __instance.entityPool[otherEntityId].minerId;
            if (minerId <= 0) return true;
            
            if (GetInsertTarget(__instance.factorySystem, otherSlotId, minerId) == removingEntityId && removingEntityId != 0)
            {
                SetMinerInsertTarget(__instance.factorySystem, otherSlotId, minerId, 0);
            }

            return false;
        }
        
        [HarmonyPatch(typeof(PlanetFactory), "ApplyInsertTarget")]
        [HarmonyPrefix]
        public static bool ApplyInsertTarget(PlanetFactory __instance, int entityId, int insertTarget, int slotId, int offset)
        {
            int minerId = __instance.entityPool[entityId].minerId;
            if (minerId <= 0) return true;
            
            SetMinerInsertTarget(__instance.factorySystem, slotId, minerId, insertTarget);

            return false;
        }
        
        [HarmonyPatch(typeof(PlanetFactory), "CreateEntityLogicComponents")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> ChangeMinerPortBehavior(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldarg_1),
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(FactorySystem), nameof(FactorySystem.SetMinerInsertTarget)))
                )
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 4))
                .SetInstructionAndAdvance(Transpilers.EmitDelegate<Action<FactorySystem, int, int, int>>((factory, minerId, entityId, slot) =>
                {
                    SetMinerInsertTarget(factory, slot, minerId, entityId);
                }));
                
                
            return matcher.InstructionEnumeration();
        }
        
        


    }
}