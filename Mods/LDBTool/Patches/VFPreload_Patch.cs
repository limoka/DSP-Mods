using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace xiaoye97.Patches
{
    [HarmonyPatch]
    public static class VFPreload_Patch
    {
        [HarmonyPrefix, HarmonyPatch(typeof(VFPreload), "InvokeOnLoadWorkEnded")]
        private static void VFPreloadPrePatch()
        {
            if (LDBTool.Finshed) return;
            LDBToolPlugin.logger.LogInfo("Pre Loading...");
            if (LDBTool.PreAddDataAction != null)
            {
                LDBTool.PreAddDataAction();
                LDBTool.PreAddDataAction = null;
            }

            LDBTool.AddProtos(LDBTool.PreToAdd);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(VFPreload), "InvokeOnLoadWorkEnded")]
        private static void VFPreloadPostPatch()
        {
            if (LDBTool.Finshed) return;
            LDBToolPlugin.logger.LogInfo("Post Loading...");
            if (LDBTool.PostAddDataAction != null)
            {
                LDBTool.PostAddDataAction();
                LDBTool.PostAddDataAction = null;
            }

            LDBTool.AddProtos(LDBTool.PostToAdd);

            if (LDBTool.EditDataAction != null)
            {
                foreach (PropertyInfo propertyInfo in typeof(LDB).GetProperties())
                {
                    Type setType = propertyInfo.PropertyType;
                    if (!setType.IsConstructedGenericType)
                    {
                        setType = setType.BaseType;
                    }

                    Type protoType = setType.GetGenericArguments()[0];

                    object protoSet = propertyInfo.GetValue(null);

                    MethodInfo method = typeof(VFPreload_Patch).GetMethod(nameof(EditAllProtos), AccessTools.all).MakeGenericMethod(protoType);
                    method.Invoke(null, new[] {protoSet});
                }
            }

            GameMain.iconSet.loaded = false;
            GameMain.iconSet.Create();
            LDBTool.SetBuildBar();
            LDBTool.Finshed = true;
            LDBToolPlugin.logger.LogInfo("Done.");
        }

        private static void EditAllProtos<T>(ProtoSet<T> protoSet)
            where T : Proto
        {
            foreach (T proto in protoSet.dataArray)
            {
                if (proto == null) continue;

                try
                {
                    LDBTool.EditDataAction(proto);
                }
                catch (Exception e)
                {
                    LDBToolPlugin.logger.LogWarning($"Edit Error: ID:{proto.ID} Type:{proto.GetType().Name} {e.Message}");
                }
            }
        }
    }
}