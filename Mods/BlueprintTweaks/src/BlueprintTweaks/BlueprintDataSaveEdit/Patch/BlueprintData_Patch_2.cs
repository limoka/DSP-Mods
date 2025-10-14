using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CommonAPI;
using HarmonyLib;
using UnityEngine;

namespace BlueprintTweaks.BlueprintDataSaveEdit
{
    [HarmonyPatch]
    public static class BlueprintData_Patch_2
    {
        private const byte INITIAL_VERSION = 1;
        private const byte CUSTOM_COLORS_VERSION = 2;
        private const byte ANCHOR_TYPE_VERSION = 3;
        
        private const byte CURRENT_DATA_VERSION = 4;

        internal static Dictionary<string, ICustomBlueprintDataSerializer> customSerializers = new Dictionary<string, ICustomBlueprintDataSerializer>();

        [HarmonyPatch(typeof(BlueprintData), "Export")]
        [HarmonyPostfix]
        public static void Export(BlueprintData __instance, BinaryWriter w)
        {
            w.Write(CURRENT_DATA_VERSION);
            w.Write((byte)0);

            w.Write((byte)__instance.anchorType);
            w.Write((byte)__instance.autoReformMode);

            bool hasSerializers = customSerializers.Count > 0;
            w.Write(hasSerializers);

            if (hasSerializers)
            {
                InvokeCustomSerializers(__instance, w);
            }
        }

        [HarmonyPatch(typeof(BlueprintData), "Import")]
        [HarmonyPostfix]
        public static void Import(BlueprintData __instance, BinaryReader r)
        {
            if (r.BaseStream.Position != r.BaseStream.Length)
            {
                byte version = r.ReadByte();

                bool hasData;
                if (version >= ANCHOR_TYPE_VERSION)
                {
                    hasData = r.ReadByte() == 1;
                }
                else
                {
                    hasData = version >= INITIAL_VERSION;
                }

                if (hasData)
                {
                    if (version >= INITIAL_VERSION)
                    {
                        MigrateFoundationData(__instance, r);
                    }

                    if (version >= CUSTOM_COLORS_VERSION && r.ReadBoolean())
                    {
                        for (int i = 0; i < 16; i++)
                        {
                            var color = (Color32) new Color(
                                r.ReadSingle(),
                                r.ReadSingle(),
                                r.ReadSingle(),
                                r.ReadSingle());
                            
                            __instance.reformData.customReformColors[i] = (uint)((color.r << 24) | (color.g << 16) | (color.b << 8) | color.a);
                        }
                    }
                }

                if (version >= ANCHOR_TYPE_VERSION)
                {
                    __instance.anchorType = r.ReadByte();
                }

                if (version >= CURRENT_DATA_VERSION)
                {
                    __instance.autoReformMode = r.ReadByte();

                    bool hasCustomData = r.ReadBoolean();

                    if (hasCustomData)
                    {
                        HandleCustomData(__instance, r);
                    }
                }
            }
        }

        private static void MigrateFoundationData(BlueprintData __instance, BinaryReader r)
        {
            int len = r.ReadInt32();
            BlueprintTweaksPlugin.logger.LogWarning($"Blueprint has {len} old foundations, performing migration.");

            var reformDatas = new Dictionary<int, byte[]>();

            for (int i = 0; i < len; i++)
            {
                r.ReadByte();

                var areaIndex = (byte)r.ReadInt32();
                if (areaIndex >= __instance.areas.Length) continue;

                var area = __instance.areas[areaIndex];
                            
                if (!reformDatas.TryGetValue(areaIndex, out byte[] reforms))
                {
                    reforms = new byte[area.width * area.height];
                    reformDatas[areaIndex] = reforms;
                }

                var type = r.ReadInt32();
                var color = r.ReadInt32();
                            
                var data = (byte)((type << 5) + (color & 31));
                var rawY = r.ReadSingle();
                var rawX = r.ReadSingle();
                            
                var y = Mathf.RoundToInt(rawY);
                var x = Mathf.RoundToInt(rawX);

                var segmentCount = area.areaSegments;
                if (x >= segmentCount * 5)
                    x -= segmentCount * 5;

                var index = y * area.width + x;
                reforms[index] = data;
            }

            var outputList = new List<BPReformRect>(32);
            uint[] openList = null;

            for (int i = 0; i < __instance.areas.Length; i++)
            {
                if (!reformDatas.ContainsKey(i)) continue;
                            
                var area = __instance.areas[i];
                var reforms = reformDatas[i];
                            
                BlueprintUtils.GenerateReformRect(ref outputList, ref openList, reforms, area.width, area.height, (byte)i);
            }
                        
            BlueprintTweaksPlugin.logger.LogWarning($"Optimized reform count: {outputList.Count}");
            __instance.reformData.rects = outputList.ToArray();
        }

        private static void InvokeCustomSerializers(BlueprintData __instance, BinaryWriter w)
        {
            foreach (var pair in customSerializers)
            {
                using MemoryStream ms = new MemoryStream();
                using BinaryWriter externalWriter = new BinaryWriter(ms);

                try
                {
                    pair.Value.Export(__instance, externalWriter);
                }
                catch (Exception e)
                {
                    BlueprintTweaksPlugin.logger.LogWarning($"Exception while executing {pair.Key} custom blueprint data serializer: \n{e}");
                    continue;
                }

                byte[] dataBytes = ms.ToArray();

                if (dataBytes.Length > short.MaxValue)
                {
                    BlueprintTweaksPlugin.logger.LogWarning("Error: Custom blueprint serializer wrote more than 32767 bytes! This is not supported.");
                    continue;
                }

                w.Write(true);
                w.Write(pair.Key);
                w.Write((short)dataBytes.Length);
                w.Write(dataBytes);
            }
            
            w.Write(false);
        }

        private static void HandleCustomData(BlueprintData __instance, BinaryReader r)
        {
            while (true)
            {
                bool hasMore = r.ReadBoolean();
                if (!hasMore) break;

                string key = r.ReadString();
                short length = r.ReadInt16();

                if (length == 0) continue;

                byte[] data = r.ReadBytes(length);

                if (!customSerializers.ContainsKey(key))
                {
                    BlueprintTweaksPlugin.logger.LogWarning($"Blueprint contains serialized data for {key}, but such serializer is not registered!");
                    continue;
                }


                ICustomBlueprintDataSerializer serializer = customSerializers[key];
                using MemoryStream ms = new MemoryStream(data);
                using BinaryReader externalReader = new BinaryReader(ms);
                
                try
                {
                    serializer.Import(__instance, externalReader);
                }
                catch (Exception e)
                {
                    BlueprintTweaksPlugin.logger.LogWarning($"Exception while executing {key} custom blueprint data serializer: \n{e}");
                }
            }
        }
    }
}