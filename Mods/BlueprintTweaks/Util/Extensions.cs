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
    }
}