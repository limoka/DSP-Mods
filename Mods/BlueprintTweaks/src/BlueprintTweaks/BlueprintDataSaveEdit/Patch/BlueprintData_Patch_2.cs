using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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

            bool hasData = __instance.reforms != null && __instance.reforms.Length > 0;
            if (hasData)
            {
                w.Write((byte)1);
                w.Write(__instance.reforms.Length);
                foreach (ReformData data in __instance.reforms)
                {
                    data.Export(w);
                }

                bool hasColors = __instance.customColors != null && __instance.customColors.Length > 0;
                w.Write(hasColors);
                if (hasColors)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        w.Write(__instance.customColors[i].r);
                        w.Write(__instance.customColors[i].g);
                        w.Write(__instance.customColors[i].b);
                        w.Write(__instance.customColors[i].a);
                    }
                }
            }
            else
            {
                w.Write((byte)0);
            }

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
                        int len = r.ReadInt32();
                        __instance.reforms = new ReformData[len];
                        for (int i = 0; i < len; i++)
                        {
                            __instance.reforms[i] = new ReformData();
                            ReformData data = __instance.reforms[i];
                            data.Import(r);
                        }
                    }

                    if (version >= CUSTOM_COLORS_VERSION && r.ReadBoolean())
                    {
                        __instance.customColors = new Color[16];
                        for (int i = 0; i < 16; i++)
                        {
                            __instance.customColors[i] = new Color(
                                r.ReadSingle(),
                                r.ReadSingle(),
                                r.ReadSingle(),
                                r.ReadSingle());
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