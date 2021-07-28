using System;
using HarmonyLib;
using UnityEngine;

namespace BlueprintTweaks
{
    public class BlueprintGridData
    {
        public bool shouldShap;
        public Vector2 snapGrid = new Vector2(1,1);
        public Vector2 snapOffset = Vector2.zero;
    }
    
    [HarmonyPatch]
    public static class BlueprintPastePatch2
    {
        public static BuildTool_BlueprintPaste tool;

        public static float lockLongitude;
        public static float lockLatitude;

        public static bool isLockedLongtitude;
        public static bool isLockedLatitude;

        public static BlueprintGridData currentGridData = new BlueprintGridData();
        
        public static void LockLongtitude()
        {
            if (tool == null) return;

            if (!isLockedLongtitude)
            {
                BlueprintUtils.GetLongitudeLatitudeRad(tool.castGroundPos.normalized, ref lockLongitude, ref lockLatitude);
                isLockedLongtitude = true;
                isLockedLatitude = false;
            }
            else
            {
                isLockedLongtitude = false;
            }
        }
        
        public static void LockLatitude()
        {
            if (tool == null) return;

            if (!isLockedLatitude)
            {
                BlueprintUtils.GetLongitudeLatitudeRad(tool.castGroundPos.normalized, ref lockLongitude, ref lockLatitude);
                isLockedLatitude = true;
                isLockedLongtitude = false;
            }
            else
            {
                isLockedLatitude = false;
            }
        }

        public static void SetOffset()
        {
            if (tool == null) return;

            if (currentGridData.shouldShap)
            {
                currentGridData.shouldShap = false;
            }
            else
            {
                if (tool.actionBuild.planetAux == null) return;
                currentGridData.shouldShap = true;
                tool.actionBuild.planetAux.CalculateOffset(tool.castGroundPos, currentGridData);
            }
        }
        
        public static Vector3 SnapModified(this PlanetAuxData auxData, Vector3 pos, bool onTerrain, BlueprintGridData gridData)
        {
            if (auxData.activeGridIndex < auxData.customGrids.Count)
            {
                Vector3 vector3 = auxData.customGrids[auxData.activeGridIndex].SnapModified(pos, gridData);
                float radius = auxData.planet.realRadius + 0.2f;
                if (!onTerrain)
                    radius = Mathf.Max(auxData.planet.radius, Mathf.Floor((float) ((pos.magnitude - (double) auxData.planet.radius) / 1.33333325386047)) * 1.333333f + auxData.planet.radius) + 0.2f;
                pos = vector3 * radius;
            }
            return pos;
        }
        
        public static Vector3 SnapModified(this PlanetGrid grid, Vector3 pos, BlueprintGridData gridData)
        {
            pos.Normalize();
            float latitude = BlueprintUtils.GetLatitudeRad(pos);
            float longtitude = BlueprintUtils.GetLongitudeRad(pos);
            
            float latitudeCount = latitude / 6.28318548202515f * grid.segment;
            float longitudeSegmentCount = PlanetGrid.DetermineLongitudeSegmentCount(Mathf.FloorToInt(Mathf.Max(0.0f, Mathf.Abs(latitudeCount) - 0.1f)), grid.segment);
            
            float longtitudeCount = longtitude / 6.283185f * longitudeSegmentCount;
            
            float snappedLatitude = SnapWithOffset(latitudeCount, gridData, 1);
            float snappedLongtitude = SnapWithOffset(longtitudeCount, gridData, 0);
            
            float latRad = snappedLatitude / grid.segment * 6.28318548202515f;
            float longRad = snappedLongtitude /  longitudeSegmentCount * 6.28318548202515f;
            return BlueprintUtils.GetDir(longRad, latRad);
        }

        public static void CalculateOffset(this PlanetAuxData auxData, Vector3 pos, BlueprintGridData gridData)
        {
            if (auxData.activeGridIndex >= auxData.customGrids.Count) return;
            
            PlanetGrid grid = auxData.customGrids[auxData.activeGridIndex];

            pos.Normalize();
            float latitude = BlueprintUtils.GetLatitudeRad(pos);
            float longtitude = BlueprintUtils.GetLongitudeRad(pos);

            float latitudeCount = latitude / 6.28318548202515f * grid.segment;
            float longitudeSegmentCount =
                PlanetGrid.DetermineLongitudeSegmentCount(Mathf.FloorToInt(Mathf.Max(0.0f, Mathf.Abs(latitudeCount) - 0.1f)), grid.segment);

            float longtitudeCount = longtitude / 6.283185f * longitudeSegmentCount;

            float offsetLat = Snap(latitudeCount) * 5f;
            float offsetLong = Snap(longtitudeCount) * 5f;

            gridData.snapOffset = new Vector2(offsetLong % gridData.snapGrid.x, offsetLat % gridData.snapGrid.y);
        }
        
        public static float Snap(float value)
        {
            return Mathf.Round(value * 5f) / 5f;
        } 

        public static float SnapWithOffset(float value, BlueprintGridData gridData, int index)
        {
            if (!gridData.shouldShap) return Snap(value);
            
            float offset = gridData.snapOffset[index];
            float snapped = Mathf.Round((value * 5f - offset) / gridData.snapGrid[index]);
            return snapped / 5f * gridData.snapGrid[index] + offset / 5f;
        }

        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), "_OnInit")]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        public static void Init(BuildTool_BlueprintPaste __instance)
        {
            tool = __instance;
        }


        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), "UpdateRaycast")]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        public static void ChangeRaycast()
        {
            if (!VFInput.onGUI && VFInput.inScreen && tool.castGround)
            {
                //BlueprintTweaksPlugin.logger.LogInfo("OldPos: " + tool.castGroundPos);
                float x = tool.blueprint.dragBoxSize_x;
                float y = tool.blueprint.dragBoxSize_y;

                currentGridData.snapGrid = new Vector2(x, y);
                
                
                float longitude = 0;
                float latitude = 0;
                BlueprintUtils.GetLongitudeLatitudeRad(tool.castGroundPos.normalized, ref longitude, ref latitude);
                if (isLockedLongtitude)
                    longitude = lockLongitude;
                
                if (isLockedLatitude)
                    latitude = lockLatitude;
                
                //BlueprintTweaksPlugin.logger.LogInfo("longitude: " + longitude);
                //BlueprintTweaksPlugin.logger.LogInfo("latitude: " + latitude);

                tool.castGroundPos = BlueprintUtils.GetDir(longitude, latitude) * tool.castGroundPos.magnitude;
                tool.castGroundPosSnapped = tool.actionBuild.planetAux.SnapModified(tool.castGroundPos, tool.castTerrain, currentGridData);
                if (tool.isDragging)
                {
                    tool.castGroundPosSnapped = tool.castGroundPosSnapped.normalized * tool.startGroundPosSnapped.magnitude;
                }
                tool.controller.cmd.test = tool.castGroundPosSnapped;
                Vector3 normalized = tool.castGroundPosSnapped.normalized;
                if (Physics.Raycast(new Ray(tool.castGroundPosSnapped + normalized * 10f, -normalized), out RaycastHit raycastHit, 20f, 8720, QueryTriggerInteraction.Collide))
                {
                    tool.controller.cmd.test = raycastHit.point;
                }
                tool.cursorTarget = tool.castGroundPosSnapped;
            }
        }
    }
}