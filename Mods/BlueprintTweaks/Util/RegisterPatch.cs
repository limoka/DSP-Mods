using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using NebulaAPI;

namespace BlueprintTweaks
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterPatch : Attribute
    {
        public string typeKey;

        public RegisterPatch(string typeKey)
        {
            this.typeKey = typeKey;
        }
    }

    public static class RegisterExtension
    {
        public static IEnumerable<Type> GetTypesWithAttributeInAssembly<T>(Assembly assembly) where T : Attribute
        {
            return assembly.GetTypes().Where(t => t.GetCustomAttributes(typeof(T), true).Length > 0);
        }
        
        public static void PatchAll(this Harmony harmony, string typeKey)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var types = GetTypesWithAttributeInAssembly<RegisterPatch>(assembly);
            foreach (Type type in types)
            {
                if (type.IsClass)
                {
                    RegisterPatch attribute = type.GetCustomAttribute<RegisterPatch>();
                    if (attribute.typeKey.Equals(typeKey))
                    {
                        harmony.PatchAll(type);
                    }
                }
                else
                {
                    BlueprintTweaksPlugin.logger.LogInfo($"Failed to patch: {type.FullName}.");
                }
            }
        }
    }
}