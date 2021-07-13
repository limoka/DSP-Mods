using System;
using HarmonyLib;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace GigaStations
{
    [HarmonyPatch]
    public static class StationUpgradePatch
    {
        [HarmonyPatch(typeof(PlanetFactory), "UpgradeEntityWithComponents")]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        public static void Postfix(int entityId, ItemProto newProto, PlanetFactory __instance)
        {
            if (entityId == 0 || __instance.entityPool[entityId].id == 0) return;
            int stationId = __instance.entityPool[entityId].stationId;
            if (stationId <= 0) return;
            
            StationComponent component = __instance.transport.stationPool[stationId];
            //ItemProto itemProto = LDB.items.Select(__instance.entityPool[component.entityId].protoId);
            
            if (component.isCollector) return;

            int newSize = component.isStellar ? GigaStationsPlugin.ilsMaxSlots : GigaStationsPlugin.plsMaxSlots;
            
            StationStore[] storage = component.storage;
            component.storage = new StationStore[newSize];
            Array.Copy(storage, component.storage, storage.Length);

            if (component.needs.Length != 13)
            {
                int[] oldNeeds = component.needs;
                component.needs = new int[13];
                Array.Copy(oldNeeds, component.needs, oldNeeds.Length);
            }
            component.energyPerTick = 1000000;

            if (component.isStellar)
            {
                component.energyMax = GigaStationsPlugin.ilsMaxAcuGJ * 1000000000;
                component.warperMaxCount = GigaStationsPlugin.ilsMaxWarps;
            }
            else
            {
                component.energyMax = GigaStationsPlugin.plsMaxAcuMJ * 1000000;
            }
        }

        [HarmonyPatch(typeof(PlayerAction_Build), "DoUpgradeObject")]
        [HarmonyPrefix]
        public static bool CheckIfCanUpgrade(PlayerAction_Build __instance, int objId, int grade, int upgrade, ref bool __result)
        {
            ItemProto itemProto = __instance.noneTool.GetItemProto(objId);
            if (itemProto.ID != GigaStationsPlugin.ils.ID && itemProto.ID != GigaStationsPlugin.pls.ID && itemProto.ID != GigaStationsPlugin.collector.ID) // not my stations
            {
                return true;
            }

            if (grade == 1 || upgrade < 0)
            {
                __result = false;
                VFAudio.Create("ui-error", null, Vector3.zero, true, 5);
                if (VFInput._buildConfirm.onDown || GameMain.gameTick % 10L == 0L)
                {
                    UIRealtimeTip.Popup("CantDowngradeWarn".Translate(), false, 0);
                }
                return false;
            }

            return true;
        }

    }
}