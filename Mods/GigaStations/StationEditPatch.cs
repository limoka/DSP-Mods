using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace GigaStations
{
    [HarmonyPatch]
    public class StationEditPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIRoot), "OnGameMainObjectCreated")]
        public static void OnGameMainObjectCreatedPostfix(UIRoot __instance)
        {
           
            if (GameMain.history.TechUnlocked(3508))
            {
                GigaStationsPlugin.logger.LogInfo("\nLogistic carrier capacity Level 8\nSetting Carrier Capacity Multipliers from settings...");
                GigaStationsPlugin.logger.LogInfo($"\nLevel 8\nSetting Vessels Capacity: 1000 * {GigaStationsPlugin.vesselCapacityMultiplier} = {1000 * GigaStationsPlugin.vesselCapacityMultiplier}\nSetting Drones Capacity: 100 * {GigaStationsPlugin.droneCapacityMultiplier} = {100 * GigaStationsPlugin.droneCapacityMultiplier}");
                GameMain.history.logisticShipCarries = GigaStationsPlugin.vesselCapacityMultiplier * 1000;
                GameMain.history.logisticDroneCarries = GigaStationsPlugin.droneCapacityMultiplier * 100;
            }
            else if (GameMain.history.TechUnlocked(3507))
            {
                GigaStationsPlugin.logger.LogInfo("\nLogistic carrier capacity Level 7\nSetting Carrier Capacity Multipliers from settings...");
                GigaStationsPlugin.logger.LogInfo($"\nLevel 7\nSetting Vessels Capacity: 800 * {GigaStationsPlugin.vesselCapacityMultiplier} = {800 * GigaStationsPlugin.vesselCapacityMultiplier}\nSetting Drones Capacity: 80 * {GigaStationsPlugin.droneCapacityMultiplier} = {80 * GigaStationsPlugin.droneCapacityMultiplier}");
                GameMain.history.logisticShipCarries = GigaStationsPlugin.vesselCapacityMultiplier * 800;
                GameMain.history.logisticDroneCarries = GigaStationsPlugin.droneCapacityMultiplier * 80;
            }
            else if (GameMain.history.TechUnlocked(3506))
            {
                GigaStationsPlugin.logger.LogInfo("\nLogistic carrier capacity Level 6\nSetting Carrier Capacity Multipliers from settings...");
                GigaStationsPlugin.logger.LogInfo($"\nLevel 6\nSetting Vessels Capacity: 600 * {GigaStationsPlugin.vesselCapacityMultiplier} = {600 * GigaStationsPlugin.vesselCapacityMultiplier}\nSetting Drones Capacity: 70 * {GigaStationsPlugin.droneCapacityMultiplier} = {70 * GigaStationsPlugin.droneCapacityMultiplier}");
                GameMain.history.logisticShipCarries = GigaStationsPlugin.vesselCapacityMultiplier * 600;
                GameMain.history.logisticDroneCarries = GigaStationsPlugin.droneCapacityMultiplier * 70;
            }
            else if (GameMain.history.TechUnlocked(3505))
            {
                GigaStationsPlugin.logger.LogInfo("\nLogistic carrier capacity Level 5\nSetting Carrier Capacity Multipliers from settings...");
                GigaStationsPlugin.logger.LogInfo($"\nLevel 5\nSetting Vessels Capacity: 500 * {GigaStationsPlugin.vesselCapacityMultiplier} = {500 * GigaStationsPlugin.vesselCapacityMultiplier}\nSetting Drones Capacity: 60 * {GigaStationsPlugin.droneCapacityMultiplier} = {60 * GigaStationsPlugin.droneCapacityMultiplier}");
                GameMain.history.logisticShipCarries = GigaStationsPlugin.vesselCapacityMultiplier * 500;
                GameMain.history.logisticDroneCarries = GigaStationsPlugin.droneCapacityMultiplier * 60;
            }
            else if (GameMain.history.TechUnlocked(3504))
            {
                GigaStationsPlugin.logger.LogInfo("\nLogistic carrier capacity Level 4\nSetting Carrier Capacity Multipliers from settings...");
                GigaStationsPlugin.logger.LogInfo($"\nLevel 4\nSetting Vessels Capacity: 400 * {GigaStationsPlugin.vesselCapacityMultiplier} = {400 * GigaStationsPlugin.vesselCapacityMultiplier}\nSetting Drones Capacity: 50 * {GigaStationsPlugin.droneCapacityMultiplier} = {50 * GigaStationsPlugin.droneCapacityMultiplier}");
                GameMain.history.logisticShipCarries = GigaStationsPlugin.vesselCapacityMultiplier * 400;
                GameMain.history.logisticDroneCarries = GigaStationsPlugin.droneCapacityMultiplier * 50;
            }
            else if (GameMain.history.TechUnlocked(3503))
            {
                GigaStationsPlugin.logger.LogInfo("\nLogistic carrier capacity Level 3\nSetting Carrier Capacity Multipliers from settings...");
                GigaStationsPlugin.logger.LogInfo($"\nLevel 3\nSetting Vessels Capacity: 300 * {GigaStationsPlugin.vesselCapacityMultiplier} = {300 * GigaStationsPlugin.vesselCapacityMultiplier}\nSetting Drones Capacity: 40 * {GigaStationsPlugin.droneCapacityMultiplier} = {40 * GigaStationsPlugin.droneCapacityMultiplier}");
                GameMain.history.logisticShipCarries = GigaStationsPlugin.vesselCapacityMultiplier * 300;
                GameMain.history.logisticDroneCarries = GigaStationsPlugin.droneCapacityMultiplier * 40;
            }
            else if (GameMain.history.TechUnlocked(3502))
            {
                GigaStationsPlugin.logger.LogInfo("\nLogistic carrier capacity Level 2\nSetting Carrier Capacity Multipliers from settings...");
                GigaStationsPlugin.logger.LogInfo($"\nLevel 2\nSetting Vessels Capacity: 200 * {GigaStationsPlugin.vesselCapacityMultiplier} = {200 * GigaStationsPlugin.vesselCapacityMultiplier}\nSetting Drones Capacity: 35 * {GigaStationsPlugin.droneCapacityMultiplier} = {35 * GigaStationsPlugin.droneCapacityMultiplier}");
                GameMain.history.logisticShipCarries = GigaStationsPlugin.vesselCapacityMultiplier * 200;
                GameMain.history.logisticDroneCarries = GigaStationsPlugin.droneCapacityMultiplier * 35;
            }
            else if (GameMain.history.TechUnlocked(3501))
            {
                GigaStationsPlugin.logger.LogInfo("\nLogistic carrier capacity Level 1\nSetting Carrier Capacity Multipliers from settings...");
                GigaStationsPlugin.logger.LogInfo($"\nLevel 1\nSetting Vessels Capacity: 200 * {GigaStationsPlugin.vesselCapacityMultiplier} = {200 * GigaStationsPlugin.vesselCapacityMultiplier}\nSetting Drones Capacity: 30 * {GigaStationsPlugin.droneCapacityMultiplier} = {30 * GigaStationsPlugin.droneCapacityMultiplier}");
                GameMain.history.logisticShipCarries = GigaStationsPlugin.vesselCapacityMultiplier * 200;
                GameMain.history.logisticDroneCarries = GigaStationsPlugin.droneCapacityMultiplier * 30;
            }
            else // still lvl 0
            {
                GigaStationsPlugin.logger.LogInfo("\nLogistic carrier capacity Level 0\nSetting Carrier Capacity Multipliers from settings...");
                GigaStationsPlugin.logger.LogInfo($"\nLevel 0\nSetting Vessels Capacity: 200 * {GigaStationsPlugin.vesselCapacityMultiplier} = {200 * GigaStationsPlugin.vesselCapacityMultiplier}\nSetting Drones Capacity: 25 * {GigaStationsPlugin.droneCapacityMultiplier} = {25 * GigaStationsPlugin.droneCapacityMultiplier}");
                GameMain.history.logisticShipCarries = GigaStationsPlugin.vesselCapacityMultiplier * 200;
                GameMain.history.logisticDroneCarries = GigaStationsPlugin.droneCapacityMultiplier * 25;
            }
            
        }

        public static void History_onTechUnlocked(int techitemID, int techitemLevel)
        {
            /*
            lvl 0	+d:  0	+v:   0	= d:  25 v:  200
            lvl 1	+d:  5	+v:   0	= d:  30 v:  200
            lvl 2	+d:  5	+v:   0	= d:  35 v:  200
            lvl 3	+d:  5	+v: 100	= d:  40 v:  300
            lvl 4	+d: 10	+v: 100 = d:  50 v:  400
            lvl 5	+d: 10	+v: 100 = d:  60 v:  500
            lvl 6	+d: 10	+v: 100 = d:  70 v:  600
            lvl 7	+d: 10	+v: 200 = d:  80 v:  800
            lvl 8	+d: 20	+v: 200 = d: 100 v: 1000
            */

            
            switch (techitemID)
            {
                case 3508:
                    GigaStationsPlugin.logger.LogInfo($"\nUnlocked Logistic carrier capacity Level {techitemLevel}\nSetting Carrier Capacity Multipliers from settings...");
                    GigaStationsPlugin.logger.LogInfo($"\nLevel {techitemLevel}\nSetting Vessels Capacity: 1000 * {GigaStationsPlugin.vesselCapacityMultiplier} = {1000 * GigaStationsPlugin.vesselCapacityMultiplier}\nSetting Drones Capacity: 100 * {GigaStationsPlugin.droneCapacityMultiplier} = {100 * GigaStationsPlugin.droneCapacityMultiplier}");
                    GameMain.history.logisticShipCarries = GigaStationsPlugin.vesselCapacityMultiplier * 1000;
                    GameMain.history.logisticDroneCarries = GigaStationsPlugin.droneCapacityMultiplier * 100;
                    break;
                case 3507:
                    GigaStationsPlugin.logger.LogInfo($"\nUnlocked Logistic carrier capacity Level {techitemLevel}\nSetting Carrier Capacity Multipliers from settings...");
                    GigaStationsPlugin.logger.LogInfo($"\nLevel {techitemLevel}\nSetting Vessels Capacity: 800 * {GigaStationsPlugin.vesselCapacityMultiplier} = {800 * GigaStationsPlugin.vesselCapacityMultiplier}\nSetting Drones Capacity: 80 * {GigaStationsPlugin.droneCapacityMultiplier} = {80 * GigaStationsPlugin.droneCapacityMultiplier}");
                    GameMain.history.logisticShipCarries = GigaStationsPlugin.vesselCapacityMultiplier * 800;
                    GameMain.history.logisticDroneCarries = GigaStationsPlugin.droneCapacityMultiplier * 80;
                    break;
                case 3506:
                    GigaStationsPlugin.logger.LogInfo($"\nUnlocked Logistic carrier capacity Level {techitemLevel}\nSetting Carrier Capacity Multipliers from settings...");
                    GigaStationsPlugin.logger.LogInfo($"\nLevel {techitemLevel}\nSetting Vessels Capacity: 600 * {GigaStationsPlugin.vesselCapacityMultiplier} = {600 * GigaStationsPlugin.vesselCapacityMultiplier}\nSetting Drones Capacity: 70 * {GigaStationsPlugin.droneCapacityMultiplier} = {70 * GigaStationsPlugin.droneCapacityMultiplier}");
                    GameMain.history.logisticShipCarries = GigaStationsPlugin.vesselCapacityMultiplier * 600;
                    GameMain.history.logisticDroneCarries = GigaStationsPlugin.droneCapacityMultiplier * 70;
                    break;
                case 3505:
                    GigaStationsPlugin.logger.LogInfo($"\nUnlocked Logistic carrier capacity Level {techitemLevel}\nSetting Carrier Capacity Multipliers from settings...");
                    GigaStationsPlugin.logger.LogInfo($"\nLevel {techitemLevel}\nSetting Vessels Capacity: 500 * {GigaStationsPlugin.vesselCapacityMultiplier} = {500 * GigaStationsPlugin.vesselCapacityMultiplier}\nSetting Drones Capacity: 60 * {GigaStationsPlugin.droneCapacityMultiplier} = {60 * GigaStationsPlugin.droneCapacityMultiplier}");
                    GameMain.history.logisticShipCarries = GigaStationsPlugin.vesselCapacityMultiplier * 500;
                    GameMain.history.logisticDroneCarries = GigaStationsPlugin.droneCapacityMultiplier * 60;
                    break;
                case 3504:
                    GigaStationsPlugin.logger.LogInfo($"\nUnlocked Logistic carrier capacity Level {techitemLevel}\nSetting Carrier Capacity Multipliers from settings...");
                    GigaStationsPlugin.logger.LogInfo($"\nLevel {techitemLevel}\nSetting Vessels Capacity: 400 * {GigaStationsPlugin.vesselCapacityMultiplier} = {400 * GigaStationsPlugin.vesselCapacityMultiplier}\nSetting Drones Capacity: 50 * {GigaStationsPlugin.droneCapacityMultiplier} = {50 * GigaStationsPlugin.droneCapacityMultiplier}");
                    GameMain.history.logisticShipCarries = GigaStationsPlugin.vesselCapacityMultiplier * 400;
                    GameMain.history.logisticDroneCarries = GigaStationsPlugin.droneCapacityMultiplier * 50;
                    break;
                case 3503:
                    GigaStationsPlugin.logger.LogInfo($"\nUnlocked Logistic carrier capacity Level {techitemLevel}\nSetting Carrier Capacity Multipliers from settings...");
                    GigaStationsPlugin.logger.LogInfo($"\nLevel {techitemLevel}\nSetting Vessels Capacity: 300 * {GigaStationsPlugin.vesselCapacityMultiplier} = {300 * GigaStationsPlugin.vesselCapacityMultiplier}\nSetting Drones Capacity: 40 * {GigaStationsPlugin.droneCapacityMultiplier} = {40 * GigaStationsPlugin.droneCapacityMultiplier}");
                    GameMain.history.logisticShipCarries = GigaStationsPlugin.vesselCapacityMultiplier * 300;
                    GameMain.history.logisticDroneCarries = GigaStationsPlugin.droneCapacityMultiplier * 40;
                    break;
                case 3502:
                    GigaStationsPlugin.logger.LogInfo($"\nUnlocked Logistic carrier capacity Level {techitemLevel}\nSetting Carrier Capacity Multipliers from settings...");
                    GigaStationsPlugin.logger.LogInfo($"\nLevel {techitemLevel}\nSetting Vessels Capacity: 200 * {GigaStationsPlugin.vesselCapacityMultiplier} = {200 * GigaStationsPlugin.vesselCapacityMultiplier}\nSetting Drones Capacity: 35 * {GigaStationsPlugin.droneCapacityMultiplier} = {35 * GigaStationsPlugin.droneCapacityMultiplier}");
                    GameMain.history.logisticShipCarries = GigaStationsPlugin.vesselCapacityMultiplier * 200;
                    GameMain.history.logisticDroneCarries = GigaStationsPlugin.droneCapacityMultiplier * 35;
                    break;
                case 3501:
                    GigaStationsPlugin.logger.LogInfo($"\nUnlocked Logistic carrier capacity Level {techitemLevel}\nSetting Carrier Capacity Multipliers from settings...");
                    GigaStationsPlugin.logger.LogInfo($"\nLevel {techitemLevel}\nSetting Vessels Capacity: 200 * {GigaStationsPlugin.vesselCapacityMultiplier} = {200 * GigaStationsPlugin.vesselCapacityMultiplier}\nSetting Drones Capacity: 30 * {GigaStationsPlugin.droneCapacityMultiplier} = {30 * GigaStationsPlugin.droneCapacityMultiplier}");
                    GameMain.history.logisticShipCarries = GigaStationsPlugin.vesselCapacityMultiplier * 200;
                    GameMain.history.logisticDroneCarries = GigaStationsPlugin.droneCapacityMultiplier * 30;
                    break;
            }
        }
        

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PowerSystem), "NewConsumerComponent")]
        public static bool NewConsumerComponentPrefix(PowerSystem __instance, ref int entityId, ref long work, ref long idle)
        {
            var x = LDB.items.Select(__instance.factory.entityPool[entityId].protoId).ID;
            if (x != GigaStationsPlugin.ils.ID && x !=GigaStationsPlugin.pls.ID)
            {
                return true;
            }

            work = 1000000;

            return true;

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(StationComponent), "Init")] // maybe swap with normal VFPreload if not supporting modded tesla towers? or later preloadpostpatch LDBTool one again if already done
        public static void StationComponentInitPostfix(StationComponent __instance, ref int _id, ref int _entityId, ref int _pcId, ref PrefabDesc _desc, ref EntityData[] _entityPool) // Do when LDB is done
        {
            __instance.needs = new int[13];

            if (_entityPool[_entityId].protoId != GigaStationsPlugin.ils.ID && _entityPool[_entityId].protoId != GigaStationsPlugin.pls.ID && _entityPool[_entityId].protoId != GigaStationsPlugin.collector.ID) // not my stations
            {
                return;
            }

            if (!__instance.isStellar && !__instance.isCollector) //pls
            {
                _desc.stationMaxEnergyAcc = Convert.ToInt64(GigaStationsPlugin.plsMaxAcuMJ * 1000000);
                __instance.energyMax = GigaStationsPlugin.plsMaxAcuMJ * 1000000;
                __instance.storage = new StationStore[GigaStationsPlugin.plsMaxSlots];
                __instance.energyPerTick = 1000000;
            }
            else if (__instance.isStellar && !__instance.isCollector)
            {
                _desc.stationMaxEnergyAcc = Convert.ToInt64(GigaStationsPlugin.ilsMaxAcuGJ * 1000000000);
                
                __instance.energyMax = GigaStationsPlugin.ilsMaxAcuGJ * 1000000000;
                __instance.warperMaxCount = GigaStationsPlugin.ilsMaxWarps;
                __instance.storage = new StationStore[GigaStationsPlugin.ilsMaxSlots];
                __instance.energyPerTick = 1000000;
            }
        }



        [HarmonyTranspiler]
        [HarmonyPatch(typeof(StationComponent), "Import")]
        public static IEnumerable<CodeInstruction> StationImportTranspiler(IEnumerable<CodeInstruction> instructions)
        {

            return new CodeMatcher(instructions)
                .MatchForward(false, // false = move at the start of the match, true = move at the end of the match
            new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldc_I4_6),
                    new CodeMatch(OpCodes.Newarr),
                    new CodeMatch(i => i.opcode == OpCodes.Stfld && ((FieldInfo)i.operand).Name == nameof(StationComponent.needs)))
                .Advance(1)
                .Set(OpCodes.Ldc_I4, 13)
                .InstructionEnumeration();

        }
        

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StationComponent), "UpdateNeeds")]
        public static bool UpdateNeeds(StationComponent __instance)
        {
            // Do for all, should not matter

            int num = __instance.storage.Length;
            if (num > 12)
            {
                num = 12;
            }
            for (int i = 0; i <= num; i++)
            {
                if (i == num && !__instance.isCollector)
                {
                    __instance.needs[num] = ((!__instance.isStellar || __instance.warperCount >= __instance.warperMaxCount) ? 0 : 1210); // HIDDEN SLOT?!?!
                }
                else if (i < __instance.needs.Length)
                {
                    __instance.needs[i] = ((i >= num || __instance.storage[i].count >= __instance.storage[i].max) ? 0 : __instance.storage[i].itemId);
                }
            }
            return false;
        }


        // Fixing Belt cannot input for item-slots 7-12
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CargoPath), "TryPickItemAtRear", new[] { typeof(int[]), typeof(int), typeof(byte), typeof(byte) }, new[] { ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out })]
        // ReSharper disable once RedundantAssignment
        public static bool TryPickItemAtRear(CargoPath __instance, int[] needs, out int needIdx, out byte stack, out byte inc, ref int __result)
        {
            needIdx = -1;
            stack = 1;
            inc = 0;
            
            int num = __instance.bufferLength - 5 - 1;
            if (__instance.buffer[num] == 250)
            {
                int num2 = __instance.buffer[num + 1] - 1 + (__instance.buffer[num + 2] - 1) * 100 + (__instance.buffer[num + 3] - 1) * 10000 + (__instance.buffer[num + 4] - 1) * 1000000;
                int item = __instance.cargoContainer.cargoPool[num2].item;
                stack = __instance.cargoContainer.cargoPool[num2].stack;
                inc = __instance.cargoContainer.cargoPool[num2].inc;

                for (int i = 0; i < needs.Length; i++)
                {
                    if (item == needs[i])
                    {
                        Array.Clear(__instance.buffer, num - 4, 10);
                        int num3 = num + 5 + 1;
                        if (__instance.updateLen < num3)
                        {
                            __instance.updateLen = num3;
                        }
                        __instance.cargoContainer.RemoveCargo(num2);
                        needIdx = i;
                        __result = item;
                        return false;
                    }
                }
            }
            __result = 0;
            return false;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(StationComponent), "TakeItem", new[] { typeof(int), typeof(int), typeof(int[]), typeof(int) }, new[] { ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Out })]
        public static bool TakeItemPrefix(StationComponent __instance, ref int _itemId, ref int _count, ref int[] _needs, out int _inc)
        {
            _inc = 0;
            bool flag = false;
            if (_needs == null)
            {
                flag = true;
            }
            else
            {
                foreach (var need in _needs)
                {
                    if (need == _itemId)
                    {
                        flag = true;
                    }
                }
            }

            if (_itemId > 0 && _count > 0 && (flag))
            {
                StationStore[] obj = __instance.storage;
                lock (obj)
                {
                    int num = __instance.storage.Length;
                    for (int i = 0; i < num; i++)
                    {
                        if (__instance.storage[i].itemId == _itemId && __instance.storage[i].count > 0)
                        {
                            _count = ((_count >= __instance.storage[i].count) ? __instance.storage[i].count : _count);
                            _itemId = __instance.storage[i].itemId;
                            _inc = (int)(__instance.storage[i].inc / (double)__instance.storage[i].count * _count + 0.5);
                            StationStore[] array = __instance.storage;
                            
                            array[i].count = array[i].count - _count;
                            array[i].inc = array[i].inc - _inc;
                            return false;
                        }
                    }
                }
            }
            _itemId = 0;
            _count = 0;
            _inc = 0;

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StationComponent), "AddItem")]
        // ReSharper disable once RedundantAssignment
        public static bool AddItemPrefix(StationComponent __instance, ref int itemId, ref int count, ref int __result)
        {
            __result = 0;
            if (itemId <= 0) return false;
            
            StationStore[] obj = __instance.storage;
            lock (obj)
            {
                for (int i = 0; i < __instance.storage.Length; i++)
                {
                    if (__instance.storage[i].itemId != itemId) continue;

                    StationStore[] array = __instance.storage;
                    array[i].count = array[i].count + count;
                    __result = count;
                    return false;
                }
            }

            return false;
        }
    }
}