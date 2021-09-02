using System.IO;
using HarmonyLib;
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantAssignment

namespace BlueprintTweaks
{
    [HarmonyPatch]
    public static class BlueprintDataPatch
    {
        
        [HarmonyPatch(typeof(BlueprintData), "IsNullOrEmpty")]
        [HarmonyPrefix]
        public static bool CheckNull(BlueprintData blueprint, ref bool __result)
        {
            __result = blueprint == null || !blueprint.isValid || blueprint.buildings.Length == 0 && blueprint.reforms.Length == 0;
            return false;
        }

        [HarmonyPatch(typeof(BlueprintData), "get_isValid")]
        [HarmonyPrefix]
        public static bool CheckValid(BlueprintData __instance, ref bool __result)
        {
            if (__instance.reforms != null) return true;
            __result = false;
            return false;
        }
        
        [HarmonyPatch(typeof(BlueprintData), "get_isEmpty")]
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
            w.Write((byte)0);
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
            if (__instance.reforms != null && __instance.reforms.Length > 0)
            {
                w.Write((byte)1);
                w.Write(__instance.reforms.Length);
                foreach (ReformData data in __instance.reforms)
                {
                    data.Export(w);
                }
            }
            else
            {
                w.Write((byte)0);
            }
        }
        
        [HarmonyPatch(typeof(BlueprintData), "Import")]
        [HarmonyPostfix]
        public static void Import(BlueprintData __instance, BinaryReader r)
        {
            if (r.BaseStream.Position != r.BaseStream.Length)
            {
                byte hasReform = r.ReadByte();
                if (hasReform == 1)
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
            }
        }
    }
}