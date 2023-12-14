using System;
using HarmonyLib;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace GigaStations
{
    [HarmonyPatch]
    public static class StationUpgradePatch
    {
        public static void UpdateArray<T>(ref T[] array, int newSize)
        {
            T[] oldArray = array;
            array = new T[newSize];
            Array.Copy(oldArray, array, oldArray.Length);
        }
        
        [HarmonyPatch(typeof(PlanetFactory), "UpgradeEntityWithComponents")]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        public static void Postfix(int entityId, ItemProto newProto, PlanetFactory __instance)
        {
            if (entityId == 0 || __instance.entityPool[entityId].id == 0) return;
            int stationId = __instance.entityPool[entityId].stationId;
            if (stationId <= 0) return;
            
            StationComponent component = __instance.transport.stationPool[stationId];
            PrefabDesc desc = newProto.prefabDesc;
            
            int newSize = component.isStellar ? GigaStationsPlugin.ilsMaxSlots : GigaStationsPlugin.plsMaxSlots;
            
            UpdateArray(ref component.storage, newSize);

            if (component.needs.Length != 13)
            {
                UpdateArray(ref component.needs, 13);
            }

            if (component.isCollector)
            {
                component.collectSpeed = desc.stationCollectSpeed;
                return;
            }
            
            component.energyPerTick = 1000000;

            UpdateArray(ref component.workDroneDatas, desc.stationMaxDroneCount);
            UpdateArray(ref component.workDroneOrders, desc.stationMaxDroneCount);

            if (component.isStellar)
            {
                UpdateArray(ref component.workShipDatas, desc.stationMaxShipCount);
                UpdateArray(ref component.workShipOrders, desc.stationMaxShipCount);
                UpdateArray(ref component.shipRenderers, desc.stationMaxShipCount);
                UpdateArray(ref component.shipUIRenderers, desc.stationMaxShipCount);
                
                component.shipDiskPos = new Vector3[desc.stationMaxShipCount];
                component.shipDiskRot = new Quaternion[desc.stationMaxShipCount];
                
                int num = component.workShipDatas.Length;
                for (int i = 0; i < num; i++)
                {
                    component.shipDiskRot[i] = Quaternion.Euler(0f, 360f / num * i, 0f);
                    component.shipDiskPos[i] = component.shipDiskRot[i] * new Vector3(0f, 0f, 11.5f);
                }
                for (int j = 0; j < num; j++)
                {
                    component.shipDiskRot[j] = component.shipDockRot * component.shipDiskRot[j];
                    component.shipDiskPos[j] = component.shipDockPos + component.shipDockRot * component.shipDiskPos[j];
                }
                
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
                    UIRealtimeTip.Popup("CantDowngradeWarn".Translate(), false);
                }
                return false;
            }

            return true;
        }

    }
}