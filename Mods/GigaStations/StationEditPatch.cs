using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using kremnev8;
using UnityEngine;
using UnityEngine.EventSystems;
using xiaoye97;
using Object = UnityEngine.Object;

// ReSharper disable InconsistentNaming

namespace GigaStations
{
    [HarmonyPatch]
    public class StationEditPatch
    {
        static bool alreadyInitialized;

        public static GameObject contentPane;
        public static GameObject scrollPane;


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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LDBTool), "VFPreloadPostPatch")] // maybe swap with normal VFPreload if not supporting modded tesla towers? or later preloadpostpatch LDBTool one again if already done
        public static void LDBVFPreloadPostPostfix() // Do when LDB is done
        {
            if (!alreadyInitialized) // Don't do when loading back into main menu
            {

                //LDB.items.Select(2110).prefabDesc.stationMaxItemCount = GigaStationsPlugin.plsMaxStorage;
                //LDB.items.Select(2110).prefabDesc.stationMaxDroneCount = GigaStationsPlugin.plsMaxDrones;
                //LDB.items.Select(2110).prefabDesc.stationMaxItemKinds = GigaStationsPlugin.plsMaxSlots;
                //LDB.items.Select(2110).prefabDesc.stationMaxEnergyAcc = Convert.ToInt64(GigaStationsPlugin.plsMaxAcuMJ * 1000000);
                ////LDB.items.Select(2110).name = "Planetary Giga Station";




                //LDB.items.Select(2111).prefabDesc.stationMaxItemCount = GigaStationsPlugin.ilsMaxStorage;
                //LDB.items.Select(2111).prefabDesc.stationMaxItemKinds = GigaStationsPlugin.ilsMaxSlots;
                //// Set MaxWarpers in station init!!!!!
                //LDB.items.Select(2111).prefabDesc.stationMaxDroneCount = GigaStationsPlugin.ilsMaxDrones;
                //LDB.items.Select(2111).prefabDesc.stationMaxShipCount = GigaStationsPlugin.ilsMaxVessels;
                //LDB.items.Select(2111).prefabDesc.stationMaxEnergyAcc = Convert.ToInt64(GigaStationsPlugin.ilsMaxAcuGJ * 1000000000);
                ////LDB.items.Select(2111).name = "Interstellar Giga Station";



                //LDB.items.Select(2105).prefabDesc.stationMaxItemCount = GigaStationsPlugin.colMaxStorage;
                //LDB.items.Select(2105).prefabDesc.stationCollectSpeed *= GigaStationsPlugin.colSpeedMultiplier;
                ////LDB.items.Select(2105).name = "Orbital Giga Collector";

                alreadyInitialized = true;
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

            //GigaStationsPlugin.logger.LogInfo($"protoID: {_entityPool[_entityId].protoId}");

            if (_entityPool[_entityId].protoId != GigaStationsPlugin.ils.ID && _entityPool[_entityId].protoId != GigaStationsPlugin.pls.ID && _entityPool[_entityId].protoId != GigaStationsPlugin.collector.ID) // not my gigastations
            {
                return;
            }

            //Debug.Log("ID: " + __instance.id);
            //string text = ((!__instance.isStellar) ? ("Planetary Giga Station #" + __instance.id.ToString()) : ((__instance.isCollector) ? ("Orbital Giga Collector #" + __instance.gid.ToString()) : ("Interstellar Giga Station #" + __instance.gid.ToString())));
            //__instance.name = text;

            if (!__instance.isStellar && !__instance.isCollector) //pls
            {
                //_desc.stationMaxItemCount = GigaStationsPlugin.plsMaxItems;
                //_desc.stationMaxDroneCount = GigaStationsPlugin.plsMaxDrones;
                
                
                _desc.stationMaxEnergyAcc = Convert.ToInt64(GigaStationsPlugin.plsMaxAcuMJ * 1000000);
                __instance.energyMax = GigaStationsPlugin.plsMaxAcuMJ * 1000000;
                __instance.storage = new StationStore[GigaStationsPlugin.plsMaxSlots];
                __instance.needs = new int[13];
                __instance.energyPerTick = 1000000;
            }
            else if (__instance.isStellar && !__instance.isCollector)
            {
                
                
                _desc.stationMaxEnergyAcc = Convert.ToInt64(GigaStationsPlugin.ilsMaxAcuGJ * 1000000000);
                
                //var x = _entityPool[_entityId].
                __instance.energyMax = GigaStationsPlugin.ilsMaxAcuGJ * 1000000000;
                __instance.warperMaxCount = GigaStationsPlugin.ilsMaxWarps;
                __instance.storage = new StationStore[GigaStationsPlugin.ilsMaxSlots];
                __instance.needs = new int[13];
                __instance.energyPerTick = 1000000;
                //_desc.stationMaxItemCount = GigaStationsPlugin.ilsMaxItems;
                //_desc.stationMaxDroneCount = GigaStationsPlugin.ilsMaxDrones;
                //_desc.stationMaxShipCount = GigaStationsPlugin.ilsMaxVessels;
            }
            else if (__instance.isCollector)
            {
                //_desc.stationMaxItemCount = GigaStationsPlugin.colMaxItems;
                //__instance.collectSpeed *= GigaStationsPlugin.colSpeedMultiplier;
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

            //do for all should not matter
            //List<CodeInstruction> list = instructions.ToList<CodeInstruction>();

            //if (list[326].opcode == System.Reflection.Emit.OpCodes.Ldc_I4_6)
            //{
            //    list[326].opcode = System.Reflection.Emit.OpCodes.Ldc_I4;
            //    list[326].operand = 13;
            //}

            //return list.AsEnumerable<CodeInstruction>();
        }

        //public static int myId = 0;

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(UIStationWindow), "get_stationId")]
        //public static void postgetstationid(UIStationWindow __instance, ref int __result)
        //{
        //    myId = __result;
        //}

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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIStationWindow), "OnMinDeliverVesselValueChange")]
        public static bool OnMinDeliverVesselValueChangePrefix(UIStationWindow __instance, ref float value)
        {
            if (__instance.event_lock)
            {
                return false;
            }
            if (__instance.stationId == 0 || __instance.factory == null)
            {
                return false;
            }
            StationComponent stationComponent = __instance.transport.stationPool[__instance.stationId];
            if (stationComponent == null || stationComponent.id != __instance.stationId)
            {
                return false;
            }
            int num = (int)(value * 1f + 0.5f);
            if (num < 1)
            {
                num = 1;
            }
            stationComponent.deliveryShips = num;
            __instance.minDeliverVesselValue.text = num.ToString("0") + " %";
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIStationWindow), "OnMinDeliverDroneValueChange")]
        public static bool OnMinDeliverDroneValueChangePrefix(UIStationWindow __instance, ref float value)
        {
            if (__instance.event_lock)
            {
                return false;
            }
            if (__instance.stationId == 0 || __instance.factory == null)
            {
                return false;
            }
            StationComponent stationComponent = __instance.transport.stationPool[__instance.stationId];
            if (stationComponent == null || stationComponent.id != __instance.stationId)
            {
                return false;
            }
            int num = (int)(value * 1f + 0.5f);
            if (num < 1)
            {
                num = 1;
            }
            stationComponent.deliveryDrones = num;
            __instance.minDeliverDroneValue.text = num.ToString("0") + " %";
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIStationWindow), "OnStationIdChange")]
        public static void OnStationIdChangePostfix(UIStationWindow __instance)
        {
            if (__instance.stationId == 0 || __instance.factory == null)
            {
                return;
            }
            StationComponent stationComponent = __instance.transport.stationPool[__instance.stationId];
            __instance.minDeliverDroneSlider.value = ((stationComponent.deliveryDrones <= 1) ? 0f : (0.1f * stationComponent.deliveryDrones)) * 10f;
            __instance.minDeliverVesselSlider.value = ((stationComponent.deliveryShips <= 1) ? 0f : (0.1f * stationComponent.deliveryShips)) * 10f;
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIStationWindow), "OnStationIdChange")]
        public static void OnStationIdChangePre(UIStationWindow __instance)
        {
            if (__instance.stationId == 0 || __instance.factory == null)
            {
                return;
            }
            
            StationComponent stationComponent = __instance.transport.stationPool[__instance.stationId];
            ItemProto itemProto = LDB.items.Select(__instance.factory.entityPool[stationComponent.entityId].protoId);

            if (itemProto.ID != GigaStationsPlugin.pls.ID && itemProto.ID != GigaStationsPlugin.ils.ID && itemProto.ID != GigaStationsPlugin.collector.ID)
            {
                return;
            }

            if (!__instance.active) return;
            
            string text = (!string.IsNullOrEmpty(stationComponent.name))
                ? stationComponent.name
                : ((!stationComponent.isStellar)
                    ? ("Planetary Giga Station #" + stationComponent.id)
                    : ((stationComponent.isCollector)
                        ? ("Orbital Giga Collector #" + stationComponent.gid)
                        : ("Interstellar Giga Station #" + stationComponent.gid)));
            __instance.nameInput.text = text;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIStationWindow), "OnStationIdChange")]
        public static void OnStationIdChangePost(UIStationWindow __instance)
        {
            if (__instance.stationId == 0 || __instance.factory == null)
            {
                return;
            }

            StationComponent stationComponent = __instance.transport.stationPool[__instance.stationId];
            ItemProto itemProto = LDB.items.Select(__instance.factory.entityPool[stationComponent.entityId].protoId);

            if (itemProto.ID != GigaStationsPlugin.pls.ID && itemProto.ID != GigaStationsPlugin.ils.ID && itemProto.ID != GigaStationsPlugin.collector.ID)
            {

                ((RectTransform) contentPane.transform).sizeDelta = new Vector2(520, 76 * (stationComponent.isStellar ? 5 : 3));
                ((RectTransform) scrollPane.transform).sizeDelta = new Vector2(520, stationComponent.isStellar ? 380 : 228);
                return;
            }

            if (__instance.active)
            {
                int storageCount = (!stationComponent.isCollector) ? stationComponent.storage.Length : stationComponent.collectionIds.Length;

                ((RectTransform) contentPane.transform).sizeDelta = new Vector2(520, 76 * storageCount);
                ((RectTransform) scrollPane.transform).sizeDelta = new Vector2(520, 380);
                
                for (int i = 0; i < __instance.storageUIs.Length; i++)
                {
                    if (i < storageCount)
                    {
                        __instance.storageUIs[i].station = stationComponent;
                        __instance.storageUIs[i].index = i;
                        __instance.storageUIs[i]._Open();
                    }
                    else
                    {
                        __instance.storageUIs[i].station = null;
                        __instance.storageUIs[i].index = 0;
                        __instance.storageUIs[i]._Close();
                    }
                    __instance.storageUIs[i].ClosePopMenu();
                }

                float newSize = stationComponent.isStellar ? 756 : 696;
                if (stationComponent.isCollector)
                {
                    int count = storageCount > 5 ? 5 : storageCount;
                    newSize = 136 + 76 * count;
                }
                
                __instance.windowTrans.sizeDelta = new Vector2(600f, newSize);
            }
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIStationWindow), "OnWarperIconClick")]
        public static bool OnWarperIconClickPrefix(UIStationWindow __instance, ref int obj)
        {
            if ((__instance.stationId == 0 || __instance.factory == null))
            {
                __instance._Close();
                return false;
            }

            StationComponent stationComponent = __instance.transport.stationPool[__instance.stationId];

            ItemProto gigaProto = LDB.items.Select(__instance.factory.entityPool[stationComponent.entityId].protoId);

            //GigaStationsPlugin.logger.LogInfo($"gigaProtoID: {gigaProto.ID} --- gigaProtoSID: {gigaProto.SID}");

            if (gigaProto.ID != GigaStationsPlugin.pls.ID && gigaProto.ID != GigaStationsPlugin.ils.ID && gigaProto.ID != GigaStationsPlugin.collector.ID)
            {
                return true; // not my giga ILS, return to original code
            }


            if (__instance.stationId == 0 || __instance.factory == null)
            {
                return false;
            }

            if (stationComponent == null || stationComponent.id != __instance.stationId)
            {
                return false;
            }
            if (!stationComponent.isStellar)
            {
                return false;
            }
            if (__instance.player.inhandItemId > 0 && __instance.player.inhandItemCount == 0)
            {
                __instance.player.SetHandItems(0, 0);
            }
            else if (__instance.player.inhandItemId > 0 && __instance.player.inhandItemCount > 0)
            {
                int num = 1210;
                ItemProto itemProto = LDB.items.Select(num);
                if (__instance.player.inhandItemId != num)
                {
                    UIRealtimeTip.Popup("只能放入".Translate() + itemProto.name);
                    return false;
                }
                int num2 = GigaStationsPlugin.ilsMaxWarps;
                int warperCount = stationComponent.warperCount;
                int num3 = num2 - warperCount;
                if (num3 < 0)
                {
                    num3 = 0;
                }
                int num4 = (__instance.player.inhandItemCount >= num3) ? num3 : __instance.player.inhandItemCount;
                if (num4 <= 0)
                {
                    UIRealtimeTip.Popup("栏位已满".Translate());
                    return false;
                }
                stationComponent.warperCount += num4;
                __instance.player.AddHandItemCount_Unsafe(-num4);
                if (__instance.player.inhandItemCount <= 0)
                {
                    __instance.player.SetHandItemId_Unsafe(0);
                    __instance.player.SetHandItemCount_Unsafe(0);
                }
            }
            else if (__instance.player.inhandItemId == 0 && __instance.player.inhandItemCount == 0)
            {
                int warperCount2 = stationComponent.warperCount;
                int num5 = warperCount2;
                if (num5 <= 0)
                {
                    return false;
                }
                if (VFInput.shift || VFInput.control)
                {
                    num5 = __instance.player.package.AddItemStacked(1210, num5);
                    if (warperCount2 != num5)
                    {
                        UIRealtimeTip.Popup("无法添加物品".Translate());
                    }
                    UIItemup.Up(1210, num5);
                }
                else
                {
                    __instance.player.SetHandItemId_Unsafe(1210);
                    __instance.player.SetHandItemCount_Unsafe(num5);
                }
                stationComponent.warperCount -= num5;
                if (stationComponent.warperCount < 0)
                {
                    Assert.CannotBeReached();
                    stationComponent.warperCount = 0;
                }
            }

            return false;
        }


        // Fixing Belt cannot input for item-slots 7-12
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CargoPath), "TryPickItemAtRear", new[] { typeof(int[]), typeof(int) }, new[] { ArgumentType.Normal, ArgumentType.Out })]
        public static bool TryPickItemAtRear(CargoPath __instance, int[] needs, out int needIdx, ref int __result)
        {
            needIdx = -1;
            int num = __instance.bufferLength - 5 - 1;
            if (__instance.buffer[num] == 250)
            {
                int num2 = __instance.buffer[num + 1] - 1 + (__instance.buffer[num + 2] - 1) * 100 + (__instance.buffer[num + 3] - 1) * 10000 + (__instance.buffer[num + 4] - 1) * 1000000;
                int item = __instance.cargoContainer.cargoPool[num2].item;

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
        [HarmonyPatch(typeof(StationComponent), "TakeItem", new[] { typeof(int), typeof(int), typeof(int[]) }, new[] { ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal })]
        public static bool TakeItemPrefix(StationComponent __instance, ref int _itemId, ref int _count, ref int[] _needs)
        {


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
                int num = __instance.storage.Length;
                for (int i = 0; i < num; i++)
                {
                    if (__instance.storage[i].itemId == _itemId && __instance.storage[i].count > 0)
                    {
                        _count = ((_count >= __instance.storage[i].count) ? __instance.storage[i].count : _count);
                        _itemId = __instance.storage[i].itemId;
                        StationStore[] array = __instance.storage;
                        int num2 = i;
                        array[num2].count = array[num2].count - _count;
                        return false;
                    }
                }
            }
            _itemId = 0;
            _count = 0;

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StationComponent), "AddItem")]
        public static bool AddItemPrefix(StationComponent __instance, ref int itemId, ref int count, ref int __result)
        {
            if (itemId <= 0)
            {
                __result = 0;
                return false;
            }
            int num = __instance.storage.Length;

            if (0 < num && __instance.storage[0].itemId == itemId)
            {
                if (true)
                {

                }
                StationStore[] array = __instance.storage;
                int num2 = 0;
                array[num2].count = array[num2].count + count;
                __result = count;
                return false;
            }
            if (1 < num && __instance.storage[1].itemId == itemId)
            {
                StationStore[] array2 = __instance.storage;
                int num3 = 1;
                array2[num3].count = array2[num3].count + count;
                __result = count;
                return false;
            }
            if (2 < num && __instance.storage[2].itemId == itemId)
            {
                StationStore[] array3 = __instance.storage;
                int num4 = 2;
                array3[num4].count = array3[num4].count + count;
                __result = count;
                return false;
            }
            if (3 < num && __instance.storage[3].itemId == itemId)
            {
                StationStore[] array4 = __instance.storage;
                int num5 = 3;
                array4[num5].count = array4[num5].count + count;
                __result = count;
                return false;
            }
            if (4 < num && __instance.storage[4].itemId == itemId)
            {
                StationStore[] array5 = __instance.storage;
                int num6 = 4;
                array5[num6].count = array5[num6].count + count;
                __result = count;
                return false;
            }
            if (5 < num && __instance.storage[5].itemId == itemId)
            {
                StationStore[] array6 = __instance.storage;
                int num7 = 5;
                array6[num7].count = array6[num7].count + count;
                __result = count;
                return false;
            }
            if (6 < num && __instance.storage[6].itemId == itemId)
            {
                StationStore[] array6 = __instance.storage;
                int num8 = 6;
                array6[num8].count = array6[num8].count + count;
                __result = count;
                return false;
            }
            if (7 < num && __instance.storage[7].itemId == itemId)
            {
                StationStore[] array6 = __instance.storage;
                int num9 = 7;
                array6[num9].count = array6[num9].count + count;
                __result = count;
                return false;
            }
            if (8 < num && __instance.storage[8].itemId == itemId)
            {
                StationStore[] array6 = __instance.storage;
                int num10 = 8;
                array6[num10].count = array6[num10].count + count;
                __result = count;
                return false;
            }
            if (9 < num && __instance.storage[9].itemId == itemId)
            {
                StationStore[] array6 = __instance.storage;
                int num11 = 9;
                array6[num11].count = array6[num11].count + count;
                __result = count;
                return false;
            }
            if (10 < num && __instance.storage[10].itemId == itemId)
            {
                StationStore[] array6 = __instance.storage;
                int num12 = 10;
                array6[num12].count = array6[num12].count + count;
                __result = count;
                return false;
            }
            if (11 < num && __instance.storage[11].itemId == itemId)
            {
                StationStore[] array6 = __instance.storage;
                int num13 = 11;
                array6[num13].count = array6[num13].count + count;
                __result = count;
                return false;
            }
            if (12 < num && __instance.storage[12].itemId == itemId)
            {
                StationStore[] array6 = __instance.storage;
                int num14 = 12;
                array6[num14].count = array6[num14].count + count;
                __result = count;
                return false;
            }
            __result = 0;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIStationWindow), "_OnCreate")]
        public static bool _OnCreatePrefix(UIStationWindow __instance)
        {
            // do always

            //part of 1% sliderstep fix
            __instance.minDeliverDroneSlider.maxValue = 100;
            __instance.minDeliverVesselSlider.maxValue = 100;

            GameObject prefab = Registry.bundle.LoadAsset<GameObject>("assets/gigastations/ui/station-scroll.prefab");

            scrollPane = Object.Instantiate(prefab, __instance.transform, false);
            
            RectTransform mainTrs = (RectTransform) scrollPane.transform;
            
            mainTrs.anchorMin = Vector2.up;
            mainTrs.anchorMax = Vector2.up;
            mainTrs.pivot = Vector2.up;
            mainTrs.anchoredPosition = new Vector2(40, -90);

            contentPane = scrollPane.transform.Find("Viewport/pane").gameObject;
            
            __instance.storageUIs = new UIStationStorage[12];
            for (int i = 0; i < __instance.storageUIs.Length; i++)
            {
                __instance.storageUIs[i] = Object.Instantiate(__instance.storageUIPrefab, contentPane.transform);
                __instance.storageUIs[i].stationWindow = __instance;
                __instance.storageUIs[i]._Create();
            }
            return false;
        }
    }
}