using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace BlueprintTweaks
{
    public class GridData
    {
        public bool shouldShap;
        public Vector2 snapGrid = new Vector2(1,1);
        public Vector2 snapOffset = Vector2.zero;
    }
    
    [HarmonyPatch]
    public static class GridSnappingPatches
    {

        public static float lockLongitude;
        public static float lockLatitude;

        public static bool isLockedLongitude;
        public static bool isLockedLatitude;

        public static GridData currentGridData = new GridData();


        public static Vector3 GetGroundCastPos()
        {
            if (GameMain.mainPlayer == null) return Vector3.zero;

            BuildTool currentTool = GameMain.mainPlayer.controller.actionBuild.activeTool;
            if (currentTool == null || VFInput.onGUI || !VFInput.inScreen) return Vector3.zero;

            int layerMask = 8720;
            bool castGround = Physics.Raycast(currentTool.mouseRay, out RaycastHit raycastHit, 800f, layerMask, QueryTriggerInteraction.Collide);
            
            return castGround ? raycastHit.point : Vector3.zero;
        }

        public static void LockLongitude()
        {
            if (!isLockedLongitude)
            {
                Vector3 pos = GetGroundCastPos();
                if (pos.Equals(Vector3.zero)) return;
                
                BlueprintUtils.GetLongitudeLatitudeRad(pos.normalized, ref lockLongitude, ref lockLatitude);
                isLockedLongitude = true;
                isLockedLatitude = false;
            }
            else
            {
                isLockedLongitude = false;
            }
        }
        
        public static void LockLatitude()
        {
            if (!isLockedLatitude)
            {
                Vector3 pos = GetGroundCastPos();
                if (pos.Equals(Vector3.zero)) return;
                
                BlueprintUtils.GetLongitudeLatitudeRad(pos.normalized, ref lockLongitude, ref lockLatitude);
                isLockedLatitude = true;
                isLockedLongitude = false;
            }
            else
            {
                isLockedLatitude = false;
            }
        }

        public static void SetOffset()
        {
            if (currentGridData.shouldShap)
            {
                currentGridData.shouldShap = false;
            }
            else
            {
                if (GameMain.localPlanet.aux == null) return;
                Vector3 pos = GetGroundCastPos();
                if (pos.Equals(Vector3.zero)) return;
                
                BlueprintUtils.GetLongitudeLatitudeRad(pos.normalized, ref lockLongitude, ref lockLatitude);
                currentGridData.shouldShap = true;
                GameMain.localPlanet.aux.CalculateOffset(pos, currentGridData);
            }
        }
        
        public static Vector3 SnapModified(this PlanetAuxData auxData, Vector3 pos, bool onTerrain)
        {
            if (auxData.activeGridIndex < auxData.customGrids.Count)
            {
                Vector3 vector3 = auxData.customGrids[auxData.activeGridIndex].SnapModifiedInternal(pos);
                float radius = auxData.planet.realRadius + 0.2f;
                if (!onTerrain)
                    radius = Mathf.Max(auxData.planet.radius, Mathf.Floor((float) ((pos.magnitude - (double) auxData.planet.radius) / 1.33333325386047)) * 1.333333f + auxData.planet.radius) + 0.2f;
                pos = vector3 * radius;
            }
            return pos;
        }
        
        public static Vector3 SnapModifiedInternal(this PlanetGrid grid, Vector3 pos)
        {
            pos.Normalize();
            float latitude = BlueprintUtils.GetLatitudeRad(pos);
            float longitude = BlueprintUtils.GetLongitudeRad(pos);
            
            float latitudeCount = latitude / 6.28318548202515f * grid.segment;
            float longitudeSegmentCount = PlanetGrid.DetermineLongitudeSegmentCount(Mathf.FloorToInt(Mathf.Max(0.0f, Mathf.Abs(latitudeCount) - 0.1f)), grid.segment);
            
            float longitudeCount = longitude / 6.283185f * longitudeSegmentCount;
            
            float snappedLatitude = SnapWithOffset(latitudeCount, currentGridData, 1);
            float snappedLongitude = SnapWithOffset(longitudeCount, currentGridData, 0);
            
            float latRad = snappedLatitude / grid.segment * 6.28318548202515f;
            float longRad = snappedLongitude /  longitudeSegmentCount * 6.28318548202515f;
            return BlueprintUtils.GetDir(longRad, latRad);
        }

        public static void CalculateOffset(this PlanetAuxData auxData, Vector3 pos, GridData gridData)
        {
            if (auxData.activeGridIndex >= auxData.customGrids.Count) return;
            
            PlanetGrid grid = auxData.customGrids[auxData.activeGridIndex];

            pos.Normalize();
            float latitude = BlueprintUtils.GetLatitudeRad(pos);
            float longitude = BlueprintUtils.GetLongitudeRad(pos);
            
            float latitudeCount = latitude / 6.28318548202515f * grid.segment;
            float longitudeSegmentCount =
                PlanetGrid.DetermineLongitudeSegmentCount(Mathf.FloorToInt(Mathf.Max(0.0f, Mathf.Abs(latitudeCount) - 0.1f)), grid.segment);

            float longtitudeCount = longitude / 6.283185f * longitudeSegmentCount;

            float offsetLat = Snap(latitudeCount) * 5f;
            float offsetLong = Snap(longtitudeCount) * 5f;

            gridData.snapOffset = new Vector2(offsetLong % gridData.snapGrid.x, offsetLat % gridData.snapGrid.y);
        }
        
        public static float Snap(float value)
        {
            return Mathf.Round(value * 5f) / 5f;
        } 

        public static float SnapWithOffset(float value, GridData gridData, int index)
        {
            if (!gridData.shouldShap) return Snap(value);
            
            float offset = gridData.snapOffset[index];
            float snapped = Mathf.Round((value * 5f - offset) / gridData.snapGrid[index]);
            return snapped / 5f * gridData.snapGrid[index] + offset / 5f;
        }
        
        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), "_OnClose")]
        [HarmonyPrefix]
        public static void Close()
        {
            if (BlueprintTweaksPlugin.resetFunctionsOnMenuExit.Value)
            {
                isLockedLongitude = false;
                isLockedLatitude = false;
                currentGridData.shouldShap = false;
            }
        }

        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), "UpdateRaycast")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PasteSnap(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(true,
            new CodeMatch(x => x.IsLdloc()),
                    new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(BuildTool_BlueprintPaste), nameof(BuildTool_Click.castGroundPos)))
                )   
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(Transpilers.EmitDelegate<Action<BuildTool_BlueprintPaste>>(tool =>
                {
                    float x = tool.blueprint.dragBoxSize_x;
                    float y = tool.blueprint.dragBoxSize_y;

                    currentGridData.snapGrid = new Vector2(x, y);

                    float longitude = 0;
                    float latitude = 0;
                    BlueprintUtils.GetLongitudeLatitudeRad(tool.castGroundPos.normalized, ref longitude, ref latitude);
                    
                    if (isLockedLongitude)
                        longitude = lockLongitude;
                    if (isLockedLatitude)
                        latitude = lockLatitude;

                    tool.castGroundPos = BlueprintUtils.GetDir(longitude, latitude) * tool.castGroundPos.magnitude;
                }));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(PlanetAuxData), nameof(PlanetAuxData.Snap))))
                .SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GridSnappingPatches), nameof(SnapModified))));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(BuildTool_Click), "UpdateRaycast")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> ClickSnap(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(x => x.IsLdloc()),
                    new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(BuildTool_Click), nameof(BuildTool_Click.castGroundPos)))
                )   
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(Transpilers.EmitDelegate<Action<BuildTool_Click>>(tool =>
                {
                    currentGridData.snapGrid = tool.handPrefabDesc.blueprintBoxSize;
                    currentGridData.snapGrid.x = Mathf.Round(currentGridData.snapGrid.x);
                    currentGridData.snapGrid.y = Mathf.Round(currentGridData.snapGrid.y);

                    float longitude = 0;
                    float latitude = 0;
                    BlueprintUtils.GetLongitudeLatitudeRad(tool.castGroundPos.normalized, ref longitude, ref latitude);
                    
                    if (isLockedLongitude)
                        longitude = lockLongitude;
                    if (isLockedLatitude)
                        latitude = lockLatitude;

                    tool.castGroundPos = BlueprintUtils.GetDir(longitude, latitude) * tool.castGroundPos.magnitude;
                }));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(PlanetAuxData), nameof(PlanetAuxData.Snap))))
                .SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GridSnappingPatches), nameof(SnapModified))));

            return matcher.InstructionEnumeration();
        }
        
        [HarmonyPatch(typeof(BuildTool_Reform), "UpdateRaycast")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> ClickReform(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(x => x.IsLdloc()),
                    new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(BuildTool_Reform), nameof(BuildTool_Click.castGroundPos)))
                )   
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(Transpilers.EmitDelegate<Action<BuildTool_Reform>>(tool =>
                {
                    currentGridData.snapGrid = new Vector2(tool.brushSize, tool.brushSize);

                    float longitude = 0;
                    float latitude = 0;
                    BlueprintUtils.GetLongitudeLatitudeRad(tool.castGroundPos.normalized, ref longitude, ref latitude);
                    
                    if (isLockedLongitude)
                        longitude = lockLongitude;
                    if (isLockedLatitude)
                        latitude = lockLatitude;

                    tool.castGroundPos = BlueprintUtils.GetDir(longitude, latitude) * tool.castGroundPos.magnitude;
                }));

            return matcher.InstructionEnumeration();
        }
    }
}