using System.IO;
using CommonAPI;
using HarmonyLib;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable RedundantAssignment

namespace BlueprintTweaks
{
    [RegisterPatch(BlueprintTweaksPlugin.BLUEPRINT_FOUNDATIONS)]
    public static class BlueprintDataPatch
    {
        [HarmonyPatch(typeof(BlueprintData), "IsNullOrEmpty")]
        [HarmonyPrefix]
        public static bool CheckNull(BlueprintData blueprint, ref bool __result)
        {
            __result = blueprint == null || !blueprint.isValid || blueprint.buildings.Length == 0 && blueprint.reforms.Length == 0;
            return false;
        }

        [HarmonyPatch(typeof(BlueprintData), "isValid", MethodType.Getter)]
        [HarmonyPrefix]
        public static bool CheckValid(BlueprintData __instance, ref bool __result)
        {
            if (__instance.reforms != null) return true;
            __result = false;
            return false;
        }

        [HarmonyPatch(typeof(BlueprintData), "isEmpty", MethodType.Getter)]
        [HarmonyPrefix]
        public static bool CheckEmpty(BlueprintData __instance, ref bool __result)
        {
            __result = (__instance.buildings == null || __instance.buildings.Length == 0) && (__instance.reforms == null || __instance.reforms.Length == 0);
            return false;
        }

        [HarmonyPatch(typeof(BlueprintData), "DataRepair")]
        [HarmonyPostfix]
        public static void Repair(BlueprintData __instance)
        {
            if (__instance.reforms == null)
            {
                __instance.reforms = new ReformData[0];
            }
        }

        [HarmonyPatch(typeof(BlueprintData), "ResetAsEmpty")]
        [HarmonyPatch(typeof(BlueprintData), "ResetContentAsEmpty")]
        [HarmonyPostfix]
        public static void Reset(BlueprintData __instance)
        {
            __instance.reforms = new ReformData[0];
        }

        [HarmonyPatch(typeof(BlueprintData), "Reset")]
        [HarmonyPostfix]
        public static void SetNull(BlueprintData __instance)
        {
            __instance.reforms = null;
        }

        public static void Export(this ReformData data, BinaryWriter w)
        {
            w.Write((byte) 0);
            w.Write(data.areaIndex);
            w.Write(data.type);
            w.Write(data.color);
            w.Write(data.localLatitude);
            w.Write(data.localLongitude);
        }

        public static void Import(this ReformData data, BinaryReader r)
        {
            r.ReadByte();
            data.areaIndex = r.ReadInt32();
            data.type = r.ReadInt32();
            data.color = r.ReadInt32();
            data.localLatitude = r.ReadSingle();
            data.localLongitude = r.ReadSingle();
        }

        [HarmonyPatch(typeof(BlueprintData), "Export")]
        [HarmonyPostfix]
        public static void Export(BlueprintData __instance, BinaryWriter w)
        {
            w.Write((byte) 3);
            
            bool hasData = __instance.reforms != null && __instance.reforms.Length > 0;
            if (hasData)
            {
                w.Write((byte) 1);
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
        }

        [HarmonyPatch(typeof(BlueprintData), "Import")]
        [HarmonyPostfix]
        public static void Import(BlueprintData __instance, BinaryReader r)
        {
            if (r.BaseStream.Position != r.BaseStream.Length)
            {
                byte version = r.ReadByte();
                
                bool hasData;
                if (version >= 3)
                {
                    hasData = r.ReadByte() == 1;
                }
                else
                {
                    hasData = version >= 1;
                }

                if (hasData)
                {
                    if (version >= 1)
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

                    if (version >= 2 && r.ReadBoolean())
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

                if (version >= 3)
                {
                    __instance.anchorType = r.ReadByte();
                }
            }
        }
    }
}