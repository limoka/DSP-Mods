using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BlueprintTweaks
{
    [RegisterPatch(BlueprintTweaksPlugin.BLUEPRINT_FOUNDATIONS)]
    public static class BlueprintCopyExtension
    {
        public static Dictionary<int, ReformData> reformPreSelection = new Dictionary<int, ReformData>();
        public static Dictionary<int, ReformData> reformSelection = new Dictionary<int, ReformData>();
        
        public static List<ReformData> tmpReformList = new List<ReformData>();

        public static bool isEnabled = true;
        public static bool copyColors = true;
        
        [HarmonyPatch(typeof(BuildTool_BlueprintCopy), "ClearPreSelection")]
        [HarmonyPostfix]
        public static void ClearPre(BuildTool_BlueprintCopy __instance)
        {
            reformPreSelection.Clear();
        }

        public static void CopyToTempArray(BuildTool_BlueprintCopy __instance)
        {
            Array.Clear(__instance._tmp_selected_ids, 0, __instance._tmp_selected_ids.Length);
            if (__instance.selectedObjIds.Count > 0)
            {
                if (__instance.selectedObjIds.Count > __instance._tmp_selected_ids.Length)
                {
                    __instance._tmp_selected_ids = new int[(int) (__instance.selectedObjIds.Count * 1.5)];
                }
                __instance.selectedObjIds.CopyTo(__instance._tmp_selected_ids);
                __instance._tmp_selected_cnt = __instance.selectedObjIds.Count;
            }
            else
            {
                __instance._tmp_selected_cnt = __instance.selectedObjIds.Count;
            }
        }

        public static void CopyReforms()
        {
            tmpReformList.Clear();
            if (reformSelection.Count > 0)
            {
                if (reformSelection.Count > tmpReformList.Capacity)
                    tmpReformList.Capacity = reformSelection.Count;

                foreach (var kv in reformSelection)
                {
                    tmpReformList.Add(kv.Value);
                }
            }
        }

        [HarmonyPatch(typeof(BuildTool_BlueprintCopy), "GenerateGratBox")]
        [HarmonyPrefix]
        public static bool CreateBox(BuildTool_BlueprintCopy __instance)
        {
            __instance.curActiveAreaGratBoxCursor = 0;

            CopyToTempArray(__instance);
            CopyReforms();

            BPGratBox gratbox = ReformBPUtils.GetBoundingRange(__instance.planet, __instance.actionBuild.planetAux, __instance._tmp_selected_ids, __instance._tmp_selected_cnt, tmpReformList, __instance.divideLineRad);

            if (__instance.selectedObjIds.Count > 0 || reformSelection.Count > 0)
            {
                float longitudeRadPerGrid = BlueprintUtils.GetLongitudeRadPerGrid(gratbox.startLatitudeRad, __instance.segment);
                float latitudeRadPerGrid = BlueprintUtils.GetLatitudeRadPerGrid(__instance.segment);
                float extend_lng_grid = Math.Max(3f - Mathf.RoundToInt(gratbox.width / longitudeRadPerGrid) / 2, 1f);
                float extend_lat_grid = Math.Max(3f - Mathf.RoundToInt(gratbox.height / latitudeRadPerGrid) / 2, 1f);
                gratbox = BlueprintUtils.GetExtendedGratBox(gratbox, extend_lng_grid, extend_lat_grid);
                __instance.curActiveAreaGratBoxCursor = BlueprintUtils.SplitGratBoxInTropicAreas(gratbox, __instance.tropicGratBoxRadRangeArr, __instance.displayGratBoxArr, __instance.segment);


                Array.Clear(__instance._tmp_selected_ids, 0, __instance._tmp_selected_ids.Length);
            }

            return false;
        }

        [HarmonyPatch(typeof(BuildTool_BlueprintCopy), "GenerateBlueprintData")]
        [HarmonyPrefix]
        public static bool GenerateBlueprintData(BuildTool_BlueprintCopy __instance, ref bool __result)
        {
            __instance.hasErrorInserterData = false;
            if (__instance.selectedObjIds.Count == 0 && reformSelection.Count == 0)
            {
                __instance.blueprint.ResetContentAsEmpty();
                __result = false;
                return false;
            }
            
            foreach (int objId in __instance.selectedObjIds)
            {
                if (!__instance.CheckInserterDataLegal(objId))
                {
                    __instance.hasErrorInserterData = true;
                    break;
                }
            }

            CopyToTempArray(__instance);

            BlueprintUtils.GenerateBlueprintData(__instance.blueprint, __instance.planet, __instance.actionBuild.planetAux, __instance._tmp_selected_ids, __instance._tmp_selected_cnt, __instance.divideLineRad);
            __instance.blueprint.CheckBuildingData();

            Array.Clear(__instance._tmp_selected_ids, 0, __instance._tmp_selected_ids.Length);
            __result = true;
            return false;
        }

        [HarmonyPatch(typeof(BuildTool_BlueprintCopy), "ClearSelection")]
        [HarmonyPostfix]
        public static void Clear(BuildTool_BlueprintCopy __instance)
        {
            reformSelection.Clear();
        }

        [HarmonyPatch(typeof(BuildTool_BlueprintCopy), "ArrangeSelection")]
        [HarmonyPostfix]
        public static void Arrange(BuildTool_BlueprintCopy __instance)
        {
            if (GameMain.localPlanet == null || GameMain.localPlanet.type == EPlanetType.Gas) return;
            
            foreach (var kv in reformPreSelection)
            {
                if (!reformSelection.ContainsKey(kv.Key))
                {
                    reformSelection.Add(kv.Key, kv.Value);
                }
            }
        }

        [HarmonyPatch(typeof(BuildTool_BlueprintCopy), "DetermineAddPreSelection")]
        [HarmonyPostfix]
        public static void SelectionAdd(BuildTool_BlueprintCopy __instance)
        {
            if (GameMain.localPlanet == null || GameMain.localPlanet.type == EPlanetType.Gas) return;

            reformPreSelection.Clear();
            if (Mathf.Abs(__instance.preSelectArcBox.x - __instance.preSelectArcBox.z) < 0.01f &&
                Mathf.Abs(__instance.preSelectArcBox.y - __instance.preSelectArcBox.w) < 0.01f &&
                __instance.castObjectId != 0) return;

            ReformBPUtils.currentGrid = GameMain.localPlanet.aux.mainGrid;
            
            ReformBPUtils.ClearDisplay();
            
            ReformBPUtils.DisplayPos(__instance.preSelectGratBox.startLatitudeRad, __instance.preSelectGratBox.startLongitudeRad, Color.green);
            ReformBPUtils.DisplayPos(__instance.preSelectGratBox.endLatitudeRad, __instance.preSelectGratBox.endLongitudeRad, Color.yellow);

            bool full = Mathf.Abs(__instance.preSelectArcBox.startLongitudeRad - __instance.preSelectArcBox.endLongitudeRad) > 2 * Mathf.PI - 0.04f;

            BPGratBox[] areas = ReformBPUtils.SplitGratBox(__instance.preSelectGratBox);

            foreach (BPGratBox box in areas)
            {
                ReformBPUtils.ItterateOnReform(__instance, box, (index, data) =>
                {
                    if (!reformPreSelection.ContainsKey(index))
                    {
                        reformPreSelection.Add(index, data);
                    }
                },full);
            }
        }

        [HarmonyPatch(typeof(BuildTool_BlueprintCopy), "DetermineSubPreSelection")]
        [HarmonyPostfix]
        public static void SelectionSub(BuildTool_BlueprintCopy __instance)
        {
            if (GameMain.localPlanet == null || GameMain.localPlanet.type == EPlanetType.Gas) return;
            
            reformPreSelection.Clear();
            if (Mathf.Abs(__instance.preSelectArcBox.x - __instance.preSelectArcBox.z) < 0.01f &&
                Mathf.Abs(__instance.preSelectArcBox.y - __instance.preSelectArcBox.w) < 0.01f &&
                __instance.castObjectId != 0) return;

            ReformBPUtils.currentGrid = GameMain.localPlanet.aux.mainGrid;
            
            ReformBPUtils.ClearDisplay();
            
            ReformBPUtils.DisplayPos(__instance.preSelectGratBox.startLatitudeRad, __instance.preSelectGratBox.startLongitudeRad, Color.green);
            ReformBPUtils.DisplayPos(__instance.preSelectGratBox.endLatitudeRad, __instance.preSelectGratBox.endLongitudeRad, Color.yellow);

            bool full = Mathf.Abs(__instance.preSelectArcBox.startLongitudeRad - __instance.preSelectArcBox.endLongitudeRad) > 2*Mathf.PI - 0.04f;

            BPGratBox[] areas = ReformBPUtils.SplitGratBox(__instance.preSelectGratBox);

            foreach (BPGratBox box in areas)
            {
                ReformBPUtils.ItterateOnReform(__instance, box, (index, data) =>
                {
                    if (reformSelection.ContainsKey(index))
                    {
                        reformSelection.Remove(index);
                    }
                }, full);
            }
        }
        
        
        [HarmonyPatch(typeof(BuildTool_BlueprintCopy), "GenerateBlueprintData")]
        [HarmonyPostfix]
        public static void AddColorData(BuildTool_BlueprintCopy __instance)
        {
            if (GameMain.localPlanet == null || GameMain.localPlanet.type == EPlanetType.Gas) return;

            PlatformSystem system = GameMain.localPlanet.factory.platformSystem;
            
            __instance.blueprint.customColors = new Color[16];
            
            Array.Copy(system.reformCustomColors, __instance.blueprint.customColors, 16);
        }
    }
}