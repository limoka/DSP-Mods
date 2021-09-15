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
        public static Type configFile;
        public static PropertyInfo OrphanedEntriesProp;

        static Extensions()
        {
            configFile = AccessTools.TypeByName("BepInEx.Configuration.ConfigFile");
            OrphanedEntriesProp = configFile.GetProperty("OrphanedEntries", AccessTools.all);
        }
        
        

        public static void MigrateConfig<T>(this ConfigFile file, string oldSection, string newSection, string[] keyFilter)
        {
            Dictionary<ConfigDefinition, string> oldEntries = (Dictionary<ConfigDefinition, string>)OrphanedEntriesProp.GetValue(file);
            List<ConfigDefinition> keysToRemove = new List<ConfigDefinition>();

            foreach (var kv in oldEntries)
            {
                string key = kv.Key.Key;
                if (kv.Key.Section.Equals(oldSection) && keyFilter.Contains(key))
                {
                    if (!file.TryGetEntry(newSection, key, out ConfigEntry<T> entry)) continue;

                    entry.SetSerializedValue(kv.Value);
                    keysToRemove.Add(kv.Key);
                    BlueprintTweaksPlugin.logger.LogInfo($"Migrating config from {oldSection}:{key} to {newSection}:{key}");
                    
                }
            }

            foreach (var key in keysToRemove)
            {
                oldEntries.Remove(key);
            }
        }

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