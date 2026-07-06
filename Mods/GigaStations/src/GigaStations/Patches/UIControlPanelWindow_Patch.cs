using System;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GigaStations
{
    [HarmonyPatch]
    public static class UIControlPanelWindow_Patch
    {
        [HarmonyPatch(typeof(UIControlPanelWindow), "_OnInit")]
        [HarmonyPostfix]
        public static void OnInit(UIControlPanelWindow __instance)
        {
            var maxCount = Math.Max(5, Math.Max(GigaStationsPlugin.plsMaxSlots, GigaStationsPlugin.ilsMaxSlots));
            if (maxCount == 5) return;

            var extraSize = (maxCount - 5) * 50 - 100; // 5 initial, 50 width, 100 empty space
            
            var mainTrans = (RectTransform)__instance.transform;
            mainTrans.sizeDelta = new Vector2(1470 + extraSize, mainTrans.sizeDelta.y); // 1470 - 250
            
            var leftTrans = (RectTransform)__instance.leftScrollRect.transform.parent;
            leftTrans.sizeDelta = new Vector2(850 + extraSize, leftTrans.sizeDelta.y); // 850 - 250
        }

        [HarmonyPatch(typeof(UIControlPanelStationEntry), "_OnCreate")]
        [HarmonyPostfix]
        public static void OnStationItemCreate(UIControlPanelStationEntry __instance)
        {
            var maxCount = Math.Max(5, Math.Max(GigaStationsPlugin.plsMaxSlots, GigaStationsPlugin.ilsMaxSlots));
            if (maxCount == 5) return;
            
            var extraSize = (maxCount - 5) * 50 - 100; // 5 initial, 50 width, 100 empty space
            
            var powerGroup = (RectTransform)__instance.powerGroup.transform;
            powerGroup.localPosition += new Vector3(62 + extraSize, 0, 0);

            var deliveryGroup = (RectTransform)__instance.droneGroupGo.transform.parent;
            deliveryGroup.localPosition += new Vector3(62 + extraSize, 0, 0);

            var item4 = __instance.storageItem4.gameObject;
            var itemsTrans =  item4.transform.parent;

            var addCount = maxCount - 5;
            
            for (int i = 0; i < addCount; i++)
            {
                var clone = Object.Instantiate(item4, itemsTrans, true);
                var trans = (RectTransform)clone.transform;
                
                // offset 50
                var storageItem = clone.GetComponent<UIControlPanelStorageItem>();
                __instance.extraStorageItems.Add(storageItem);
                storageItem.index = 5 + i;
                
                trans.localPosition += new Vector3(50 * (i + 1), 0, 0);
            }
        }
        
        [HarmonyPatch(typeof(UIControlPanelAdvancedMinerEntry), "_OnCreate")]
        [HarmonyPostfix]
        public static void OnAdvMinerItemCreate(UIControlPanelAdvancedMinerEntry __instance)
        {
            var maxCount = Math.Max(5, Math.Max(GigaStationsPlugin.plsMaxSlots, GigaStationsPlugin.ilsMaxSlots));
            if (maxCount == 5) return;
            
            var extraSize = (maxCount - 5) * 50 - 100; // 5 initial, 50 width, 100 empty space
            
            var powerGroup = (RectTransform)__instance.powerGroupGo.transform;
            powerGroup.localPosition += new Vector3(62 + extraSize, 0, 0);
        }

        [HarmonyPatch(typeof(UIControlPanelStationEntry), "UpdateItems")]
        [HarmonyPostfix]
        public static void OnUpdateItems(UIControlPanelStationEntry __instance)
        {
            if (!__instance.isTargetDataValid) return;
            var maxCount = Math.Max(5, Math.Max(GigaStationsPlugin.plsMaxSlots, GigaStationsPlugin.ilsMaxSlots));
            if (maxCount == 5) return;
            
            StationStore[] storage = __instance.station.storage;

            for (int i = 0; i < __instance.extraStorageItems.Count; i++)
            {
                var item = __instance.extraStorageItems[i];
                var pos = 5 + i;
                
                item.SetVisible(storage.Length > pos && storage[pos].itemId > 0);
                item._Update();
            }
        }

        [HarmonyPatch(typeof(UIControlPanelStationEntry), "OnSetTarget")]
        [HarmonyPostfix]
        public static void OnSetTarget(UIControlPanelStationEntry __instance)
        {
            for (int i = 0; i < __instance.extraStorageItems.Count; i++)
            {
                var item = __instance.extraStorageItems[i];
                item.station = __instance.station;
            }
        }
        
        [HarmonyPatch(typeof(UIControlPanelStationEntry), "OnUnsetTarget")]
        [HarmonyPostfix]
        public static void OnUnsetTarget(UIControlPanelStationEntry __instance)
        {
            for (int i = 0; i <  __instance.extraStorageItems.Count; i++)
            {
                var item = __instance.extraStorageItems[i];
                item.station = null;
            }
        }

    }
}