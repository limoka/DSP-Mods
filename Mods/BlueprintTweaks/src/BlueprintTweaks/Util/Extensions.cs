using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace BlueprintTweaks
{
    public static class Extensions
    {
        public static bool Approximately(this Quaternion quatA, Quaternion value, float acceptableRange = 0.01f)
        {
            return 1 - Mathf.Abs(Quaternion.Dot(quatA, value)) < acceptableRange;
        }

        public static Vector2 Rotate(this Vector2 v, float degrees) {
            float radians = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);
         
            float tx = v.x;
            float ty = v.y;
 
            return new Vector2(cos * tx - sin * ty, sin * tx + cos * ty);
        }

        public static bool IsOnEdgeOfGratBox(this BPGratBox box, Vector3 pos, int segmentCount)
        {
            pos.Normalize();
            float num = pos.y;
            if (num > 0.999999f)
            {
                return box.w >= 1.5707864f;
            }
            if (num < -0.999999f)
            {
                return box.y <= -1.5707864f;
            }
            float latitudeRad = Mathf.Asin(num);
            float longitudeRad = Mathf.Atan2(pos.x, -pos.z);
            return box.IsOnEdgeOfGratBox(longitudeRad, latitudeRad, segmentCount);
        }
        
        public static bool IsOnEdgeOfGratBox(this BPGratBox box, float longitudeRad, float latitudeRad, int segmentCount)
        {
            float latPerGrid = BlueprintUtils.GetLatitudeRadPerGrid(segmentCount);
            float longPerGrid = BlueprintUtils.GetLongitudeRadPerGrid(latitudeRad, segmentCount);

            BPGratBox smallBox = new BPGratBox(box.x + longPerGrid, box.y + latPerGrid, box.z - longPerGrid, box.w - latPerGrid);

            return !smallBox.InGratBox(longitudeRad, latitudeRad);
        }

    }
}