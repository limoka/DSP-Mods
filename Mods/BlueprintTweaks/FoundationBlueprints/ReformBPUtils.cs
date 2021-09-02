using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BlueprintTweaks
{
    public static class ReformBPUtils
    {
        public static PlanetGrid currentGrid;
        public static List<GameObject> markers = new List<GameObject>();
        public static void GetSegmentCount(float latitude, float longitude, out float latitudeCount, out float longitudeCount)
        {
            latitudeCount = latitude / 6.2831855f * currentGrid.segment;
            int latitudeIndex = Mathf.FloorToInt(Mathf.Abs(latitudeCount));
            int segmentCount = PlanetGrid.DetermineLongitudeSegmentCount(latitudeIndex, currentGrid.segment);

            longitudeCount = longitude / 6.2831855f * segmentCount;
        }

        public static void GetSegmentCount(float latitude, float longitude, out float latitudeCount, out float longitudeCount, out int segmentCount)
        {
            latitudeCount = latitude / 6.2831855f * currentGrid.segment;
            int latitudeIndex = Mathf.FloorToInt(Mathf.Abs(latitudeCount));
            segmentCount = PlanetGrid.DetermineLongitudeSegmentCount(latitudeIndex, currentGrid.segment);

            longitudeCount = longitude / 6.2831855f * segmentCount;
        }
        
        public static float GetAverageLongitude(float a, float b)
        {
            if (a > b)
            {
                float temp = a;
                a = b;
                b = temp;
            }
     
            if (b - a > Mathf.PI) 
                b -= 2 * Mathf.PI;
     
            float finalValue = (b + a)/2;
     
            if (finalValue < 0) finalValue += 2 * Mathf.PI;
                
            return finalValue;
        }

        public static Vector2 GetSphericalCenter(float lat1, float long1, float lat2, float long2)
        {
            const float HALF_PI = Mathf.PI / 2;
            if (lat1 < -HALF_PI)
            {
                lat1 += Mathf.PI;
                long1 += Mathf.PI;
            }
            
            if (lat1 > HALF_PI)
            {
                lat1 = Mathf.PI - lat1;
                long1 += Mathf.PI;
            }
            
            if (lat2 < -HALF_PI)
            {
                lat2 += Mathf.PI;
                long2 += Mathf.PI;
            }
            
            if (lat2 > HALF_PI)
            {
                lat2 = Mathf.PI - lat1;
                long2 += Mathf.PI;
            }

            float avrLat = (lat1 + lat2) / 2;
            float avrLong = GetAverageLongitude(long1, long2);

            return new Vector2(avrLat, avrLong);
        }

        public static int SplitGratBoxInTropicAreas(BPGratBox _gratbox, BPGratBox[] _areaBoxes, int _segmentCnt = 200)
        {
            float startLong = _gratbox.x;
            float startLat = _gratbox.y;
            float endLong = _gratbox.z;
            float endLat = _gratbox.w;
            if (endLong < startLong - 1E-05f)
            {
                endLong += 6.2831855f;
            }

            int endIdx = BlueprintUtils.GetSnappedLatitudeGridIdx(endLat, _segmentCnt);
            int startIdx = BlueprintUtils.GetSnappedLatitudeGridIdx(startLat, _segmentCnt);
            float radPerGrid = BlueprintUtils.GetLatitudeRadPerGrid(_segmentCnt);
            int index = 0;
            int index1 = -1;
            float curLat = _gratbox.y;
            _areaBoxes[index] = BPGratBox.zero;
            _areaBoxes[index].y = _areaBoxes[index].w = curLat;
            int startSegCount = BlueprintUtils.GetLongitudeSegmentCount(startIdx, _segmentCnt);
            for (int i = startIdx; i <= endIdx; i++)
            {
                int curIdx = i + 1 > endIdx ? endIdx : i + 1;
                curLat = curIdx * radPerGrid;
                int curSegCount = startSegCount;
                int segmentCount = BlueprintUtils.GetLongitudeSegmentCount(curIdx, _segmentCnt);
                startSegCount = segmentCount;
                bool different = curSegCount != segmentCount;
                if (index1 < index)
                {
                    float radPerGrid2 = BlueprintUtils.GetLongitudeRadPerGrid(curSegCount, _segmentCnt);
                    float startCount = startLong / radPerGrid2;
                    float endCount = endLong / radPerGrid2;
                    float roundStartCount = BlueprintUtils._round(startCount);
                    endCount = BlueprintUtils._round(endCount);
                    float startLong2 = roundStartCount * radPerGrid2;
                    float endLong2 = endCount * radPerGrid2;
                    if (endLong2 > startLong2 + 6.2831855f - 1E-05f - 4E-06f)
                    {
                        endLong2 = startLong2 + 6.2831855f - radPerGrid2;
                    }

                    if (startLong2 < -3.1415927f)
                    {
                        startLong2 += 6.2831855f;
                    }

                    if (startLong2 > 3.1415927f)
                    {
                        startLong2 -= 6.2831855f;
                    }

                    if (endLong2 < -3.1415927f)
                    {
                        endLong2 += 6.2831855f;
                    }

                    if (endLong2 > 3.1415927f)
                    {
                        endLong2 -= 6.2831855f;
                    }

                    _areaBoxes[index].x = startLong2;
                    _areaBoxes[index].z = endLong2;
                    index1 = index;
                }

                if (different)
                {
                    if (curIdx > 0)
                    {
                        curIdx = i > endIdx ? endIdx : i;
                        curLat = curIdx * radPerGrid;
                    }

                    index++;
                    _areaBoxes[index] = BPGratBox.zero;
                    _areaBoxes[index].y = _areaBoxes[index].w = curLat;
                }
                else
                {
                    if (curIdx < 0)
                    {
                        curIdx = i + 2 > endIdx ? endIdx : i + 2;
                        curLat = curIdx * radPerGrid;
                    }

                    _areaBoxes[index].w = curLat;
                }
            }

            return index + 1;
        }

        public static BPGratBox[] SplitGratBox(BPGratBox box)
        {
            int areaCount = BlueprintUtils.GetAreaCount(box.y, box.w, currentGrid.segment);
            BPGratBox[] areas = new BPGratBox[areaCount];
            SplitGratBoxInTropicAreas(box, areas, currentGrid.segment);
            return areas;
        }

        public static void ItterateOnReform(BuildTool_BlueprintCopy __instance, BPGratBox box, Action<int, ReformData> action)
        {
            if (Mathf.Abs(box.y - box.w) < 0.005f) return;
            
            PlatformSystem platform = __instance.factory.platformSystem;

            GetSegmentCount(box.startLatitudeRad, box.startLongitudeRad, out float startLatCount, out float startLongCount, out int startCount);
            GetSegmentCount(box.endLatitudeRad, box.endLongitudeRad, out float endLatCount, out float endLongCount, out int endCount);

            startLatCount = GridSnappingPatches.Snap(startLatCount);
            startLongCount = GridSnappingPatches.Snap(startLongCount);
            endLatCount = GridSnappingPatches.Snap(endLatCount);
            endLongCount = GridSnappingPatches.Snap(endLongCount);

            startLatCount = Mathf.Round(startLatCount * 10f);
            endLatCount = Mathf.Round(endLatCount * 10f);
            startLongCount = Mathf.Round(startLongCount * 10f);
            endLongCount = Mathf.Round(endLongCount * 10f);

            float latDelta = endLatCount - startLatCount;
            int segmentCount;

            float longDelta;

            if (startCount != endCount)
            {
                Vector2 center = GetSphericalCenter(box.startLatitudeRad, box.startLongitudeRad, box.endLatitudeRad, box.endLongitudeRad);

                GetSegmentCount(center.x, center.y, out float _, out float _, out int midCount);
                segmentCount = midCount;
                if (startCount == midCount)
                {
                    GetSegmentCount(box.startLatitudeRad, box.endLongitudeRad, out float _, out float nlongCount);
                    nlongCount = Mathf.Round(nlongCount * 10f);
                    longDelta = nlongCount - startLongCount;
                }
                else
                {
                    GetSegmentCount(box.endLatitudeRad, box.startLongitudeRad, out float _, out float nlongCount);
                    nlongCount = Mathf.Round(nlongCount * 10f);
                    longDelta = endLongCount - nlongCount;
                    startLongCount = nlongCount;
                }
            }
            else
            {
                segmentCount = startCount;
                longDelta = endLongCount - startLongCount;
            }

            if (longDelta < 0)
            {
                longDelta = segmentCount * 10 + longDelta;
            }

            int latSize = Mathf.RoundToInt(latDelta) / 2;
            int longSize = Mathf.RoundToInt(longDelta) / 2;
            if (latSize == 0)
                latSize = 1;
            if (longSize == 0)
                longSize = 1;

            startLatCount += 1;
            startLongCount += 1;

            int latOffset = 0;
            int longOffset = 0;
            int longCounter = 0;

            float latCount = platform.latitudeCount / 10f;

            for (int i = 0; i < longSize * latSize; i++)
            {
                longCounter++;
                float currentLat = (startLatCount + latOffset) / 10f;
                float currentLong = (startLongCount + longOffset) / 10f;

                float latRad = (currentLat + 0.1f) / currentGrid.segment * 6.2831855f;
                float longRad = (currentLong + 0.1f) / segmentCount * 6.2831855f;


                longOffset += 2;
                if (longCounter % longSize == 0)
                {
                    longOffset = 0;
                    latOffset += 2;
                }

                if (currentLat >= latCount || currentLat <= -latCount) continue;

                int reformIndex = platform.GetReformIndexForSegment(currentLat, currentLong);

                int reformType = platform.GetReformType(reformIndex);
                int reformColor = platform.GetReformColor(reformIndex);

                if (!platform.IsTerrainReformed(reformType)) continue;

                ReformData reform = new ReformData
                {
                    latitude = latRad,
                    longitude = longRad,
                    type = reformType,
                    color = reformColor
                };

                action(reformIndex, reform);
            }
        }

        public static void DisplayPos(Vector3 pos, Color color)
        {
#if DEBUG
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.Destroy(sphere.GetComponent<SphereCollider>());
            sphere.GetComponent<MeshRenderer>().material.color = color;
            markers.Add(sphere);
            sphere.transform.position = pos;
            sphere.transform.localScale *= 0.4f;
#endif
        }

        public static void DisplayPos(float latitude, float longitude, Color color)
        {
#if DEBUG
            Vector3 pos = BlueprintUtils.GetDir(longitude, latitude);
            pos *= GameMain.localPlanet.realRadius + 0.4f;

            DisplayPos(pos, color);
#endif
        }

        public static void ClearDisplay()
        {
#if DEBUG
            foreach (GameObject marker in markers)
            {
                if (marker != null)
                {
                    Object.Destroy(marker);
                }
            }

            markers.Clear();
#endif
        }

        public static BPGratBox GetBoundingRange(PlanetData _planet, PlanetAuxData aux, int[] _objIds, int _objCount, List<ReformData> reforms, float _divideLongitude)
        {
            if (reforms.Count == 0 && _objCount == 0)
            {
                return BPGratBox.zero;
            }

            float startlong = float.MaxValue;
            float startLat = float.MaxValue;
            float endLong = float.MinValue;
            float endLat = float.MinValue;

            bool isPole = true;

            EntityData[] entityPool = _planet.factory.entityPool;
            PrebuildData[] prebuildPool = _planet.factory.prebuildPool;
            for (int i = 0; i < _objCount; i++)
            {
                int id = _objIds[i];
                Vector3 pos = (id > 0) ? entityPool[id].pos.normalized : prebuildPool[-id].pos.normalized;
                Vector3 normalized = aux.Snap(pos, true).normalized;
                float num6 = Mathf.Asin(normalized.y);
                float num7 = Mathf.Atan2(normalized.x, -normalized.z);
                if (normalized.x * normalized.x + normalized.z * normalized.z >= 5E-07f)
                {
                    isPole = false;
                    float correctLong = num7 - _divideLongitude;
                    if (correctLong <= -1.8E-05f)
                    {
                        correctLong += 6.2831855f;
                    }
                    if (correctLong < 0f)
                    {
                        correctLong = 0f;
                    }
                    startlong = ((startlong < correctLong) ? startlong : correctLong);
                    endLong = ((endLong > correctLong) ? endLong : correctLong);
                }
                startLat = ((startLat < num6) ? startLat : num6);
                endLat = ((endLat > num6) ? endLat : num6);
            }

            if (BlueprintCopyExtension.isEnabled)
            {
                foreach (var data in reforms)
                {
                    if (data.longitude != 0)
                    {
                        isPole = false;
                        float correctLong = data.longitude - _divideLongitude;
                        if (correctLong <= -1.8E-05f)
                        {
                            correctLong += 6.2831855f;
                        }

                        if (correctLong < 0f)
                        {
                            correctLong = 0f;
                        }

                        startlong = startlong < correctLong ? startlong : correctLong;
                        endLong = endLong > correctLong ? endLong : correctLong;
                    }

                    startLat = startLat < data.latitude ? startLat : data.latitude;
                    endLat = endLat > data.latitude ? endLat : data.latitude;
                }
            }

            if (startlong < 0f)
            {
                startlong = 0f;
            }
            else if (startlong > 6.2831674f)
            {
                startlong = 6.2831674f;
            }

            if (endLong < 0f)
            {
                endLong = 0f;
            }
            else if (endLong > 6.2831674f)
            {
                endLong = 6.2831674f;
            }

            if (isPole)
            {
                startlong = 0f;
                endLong = 6.2831674f;
            }

            startlong += _divideLongitude;
            endLong += _divideLongitude;
            if (startlong < -3.1415927f)
            {
                startlong += 6.2831855f;
            }
            else if (startlong > 3.1415927f)
            {
                startlong -= 6.2831855f;
            }

            if (endLong < -3.1415927f)
            {
                endLong += 6.2831855f;
            }
            else if (endLong > 3.1415927f)
            {
                endLong -= 6.2831855f;
            }

            return new BPGratBox(startlong, startLat, endLong, endLat);
        }
        
        public static Dictionary<int, int> tmpLevelChanges = new Dictionary<int, int>();
        
        public static int ComputeFlattenTerrainReform(PlanetFactory factory, List<Vector3> points, Vector3 center, float fade0 = 3f)
        {
            PlanetRawData data = factory.planet.data;
            tmpLevelChanges.Clear();

            float realRadius = factory.planet.realRadius;
            ushort[] heightData = data.heightData;

            float heightDiff = (heightData[data.QueryIndex(center)] - factory.planet.realRadius * 100f + 20f) * 0.01f * 2f;
            heightDiff = Mathf.Min(9f, Mathf.Abs(heightDiff));
            fade0 += heightDiff;
            float steps = realRadius * 3.1415927f / (factory.planet.precision * 2f);
            int extent = Mathf.CeilToInt(fade0 * 1.414f / steps * 1.5f + 0.5f);

            int cost = 0;
            foreach (Vector3 vpos in points)
            {
                float longitude = BlueprintUtils.GetLongitudeRad(vpos.normalized);
                float longCount = 2 * longitude / Mathf.PI;
                float angleDistance = Mathf.Abs(longCount - Mathf.Round(longCount));

                if (angleDistance <= 0.04)
                {
                    cost += ScanTerrainDetailed(data, realRadius, vpos, extent, fade0);
                }
                else
                {
                    cost += ScanTerrain(data, realRadius, vpos, extent, fade0);
                }
            }

            return cost;
        }
        
        private static int ScanTerrainDetailed(PlanetRawData data, float realRadius, Vector3 vpos, int extent, float fade0)
        {
            int stride = data.stride;
            int dataLength = data.dataLength;
            const float minimum = 8f;
            
            Vector3[] vertices = data.vertices;
            ushort[] heightData = data.heightData;
            int cost = 0;

            Quaternion rotation = Maths.SphericalRotation(vpos, 22.5f);

            Vector3[] offsets = {
                Vector3.zero,
                rotation * (new Vector3(0f, 0f, 1.414f) * fade0),
                rotation * (new Vector3(0f, 0f, -1.414f) * fade0),
                rotation * (new Vector3(1.414f, 0f, 0f) * fade0),
                rotation * (new Vector3(-1.414f, 0f, 0f) * fade0),
            };

            foreach (Vector3 offset in offsets)
            {
                int start = data.QueryIndex(vpos + offset);

                for (int i = -extent; i <= extent; i++)
                {
                    int index = start + i * stride;
                    if (index < 0 || index >= dataLength) continue;


                    for (int j = -extent; j <= extent; j++)
                    {
                        int finalIndex = index + j;
                        if ((ulong) finalIndex >= (ulong) dataLength) continue;

                        Vector3 groundPos = vertices[finalIndex] * realRadius;

                        float closestDist = (vpos - groundPos).sqrMagnitude;

                        int currentLevel = data.GetModLevel(finalIndex);

                        if (tmpLevelChanges.ContainsKey(finalIndex))
                        {
                            currentLevel = tmpLevelChanges[finalIndex];
                            if (currentLevel == 3) continue;
                        }

                        int fillLevel;
                        if (closestDist <= minimum)
                        {
                            fillLevel = 3;
                        }
                        else
                        {
                            closestDist -= minimum;
                            if (closestDist > fade0 * fade0) continue;

                            float dist = closestDist / (fade0 * fade0);
                            if (dist <= 0.1111111f)
                            {
                                fillLevel = 2;
                            }
                            else if (dist <= 0.4444444f)
                            {
                                fillLevel = 1;
                            }
                            else
                            {
                                if (dist >= 1f)
                                {
                                    continue;
                                }

                                fillLevel = 0;
                            }
                        }

                        int fillDiff = fillLevel - currentLevel;
                        if (fillLevel >= currentLevel && fillDiff != 0)
                        {
                            tmpLevelChanges[finalIndex] = fillLevel;
                            float height = heightData[finalIndex] * 0.01f;
                            float fillHeight = realRadius + 0.2f - height;
                            if (fillHeight < 0f)
                            {
                                fillHeight *= 2f;
                            }

                            float tileCost = 100f * fillDiff * fillHeight * 0.3333333f;
                            cost += Mathf.FloorToInt(tileCost);
                        }
                    }
                }
            }

            return cost;
        }

        private static int ScanTerrain(PlanetRawData data, float realRadius, Vector3 vpos, int extent, float fade0)
        {
            int stride = data.stride;
            int dataLength = data.dataLength;
            const float minimum = 8f;
            
            Vector3[] vertices = data.vertices;
            ushort[] heightData = data.heightData;
            
            int start = data.QueryIndex(vpos);

            int cost = 0;

            for (int i = -extent; i <= extent; i++)
            {
                int index = start + i * stride;
                if (index < 0 || index >= dataLength) continue;


                for (int j = -extent; j <= extent; j++)
                {
                    int finalIndex = index + j;
                    if ((ulong) finalIndex >= (ulong) dataLength) continue;

                    Vector3 groundPos = vertices[finalIndex] * realRadius;

                    float closestDist = (vpos - groundPos).sqrMagnitude;

                    int currentLevel = data.GetModLevel(finalIndex);

                    if (tmpLevelChanges.ContainsKey(finalIndex))
                    {
                        currentLevel = tmpLevelChanges[finalIndex];
                        if (currentLevel == 3) continue;
                    }

                    int fillLevel;
                    if (closestDist <= minimum)
                    {
                        fillLevel = 3;
                    }
                    else
                    {
                        closestDist -= minimum;
                        if (closestDist > fade0 * fade0) continue;

                        float dist = closestDist / (fade0 * fade0);
                        if (dist <= 0.1111111f)
                        {
                            fillLevel = 2;
                        }
                        else if (dist <= 0.4444444f)
                        {
                            fillLevel = 1;
                        }
                        else
                        {
                            if (dist >= 1f)
                            {
                                continue;
                            }

                            fillLevel = 0;
                        }
                    }

                    int fillDiff = fillLevel - currentLevel;
                    if (fillLevel >= currentLevel && fillDiff != 0)
                    {
                        tmpLevelChanges[finalIndex] = fillLevel;
                        float height = heightData[finalIndex] * 0.01f;
                        float fillHeight = realRadius + 0.2f - height;
                        if (fillHeight < 0f)
                        {
                            fillHeight *= 2f;
                        }

                        float tileCost = 100f * fillDiff * fillHeight * 0.3333333f;
                        cost += Mathf.FloorToInt(tileCost);
                    }
                }
            }

            return cost;
        }

        public static void FlattenTerrainReform(PlanetFactory factory, List<Vector3> points, Vector3 center)
        {
            if (factory.tmp_ids == null)
            {
                factory.tmp_ids = new int[1024];
            }

            if (factory.tmp_entity_ids == null)
            {
                factory.tmp_entity_ids = new int[1024];
            }

            Array.Clear(factory.tmp_ids, 0, factory.tmp_ids.Length);
            Array.Clear(factory.tmp_entity_ids, 0, factory.tmp_entity_ids.Length);

            PlanetRawData data = factory.planet.data;
            ushort[] heightData = data.heightData;
            short h = (short) (factory.planet.realRadius * 100f + 20f);
            bool levelized = factory.planet.levelized;
            int step = Mathf.RoundToInt((center.magnitude - 0.2f - factory.planet.realRadius) / 1.3333333f);
            int heightT = step * 133 + h - 60;
            float heightT2 = factory.planet.radius * 100f - 20f;
            foreach (KeyValuePair<int, int> keyValuePair in tmpLevelChanges)
            {
                if (keyValuePair.Value > 0)
                {
                    ushort height = heightData[keyValuePair.Key];
                    if (levelized)
                    {
                        if (height >= heightT)
                        {
                            if (data.GetModLevel(keyValuePair.Key) < 3)
                            {
                                data.SetModPlane(keyValuePair.Key, step);
                            }

                            factory.planet.AddHeightMapModLevel(keyValuePair.Key, keyValuePair.Value);
                        }
                    }
                    else
                    {
                        factory.planet.AddHeightMapModLevel(keyValuePair.Key, keyValuePair.Value);
                    }

                    if (height < heightT2)
                    {
                        factory.planet.landPercentDirty = true;
                    }
                }
            }

            if (factory.planet.UpdateDirtyMeshes())
            {
                factory.RenderLocalPlanetHeightmap();
            }

            foreach (Vector3 point in points)
            {
                NearColliderLogic nearColliderLogic = factory.planet.physics.nearColliderLogic;
                int num7 = nearColliderLogic.GetVegetablesInAreaNonAlloc(point, 1, factory.tmp_ids);
                for (int i = 0; i < num7; i++)
                {
                    int id = factory.tmp_ids[i];
                    Vector3 position = factory.vegePool[id].pos;
                    position -= point;
                    if (position.sqrMagnitude <= 9f)
                    {
                        factory.RemoveVegeWithComponents(id);
                    }
                    else
                    {
                        float d = data.QueryModifiedHeight(factory.vegePool[id].pos) - 0.03f;
                        factory.vegePool[id].pos = factory.vegePool[id].pos.normalized * d;
                        GameMain.gpuiManager.AlterModel(factory.vegePool[id].modelIndex, factory.vegePool[id].modelId, id,
                            factory.vegePool[id].pos, factory.vegePool[id].rot, false);
                    }
                }
            }


            tmpLevelChanges.Clear();
            Array.Clear(factory.tmp_ids, 0, factory.tmp_ids.Length);
            Array.Clear(factory.tmp_ids, 0, factory.tmp_entity_ids.Length);
            GameMain.gpuiManager.SyncAllGPUBuffer();
        }
    }
}