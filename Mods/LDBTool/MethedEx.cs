using System;
using HarmonyLib;

namespace xiaoye97
{
    public static class MethedEx
    {
        /// <summary>
        /// Copy object
        /// </summary>
        [Obsolete("Please don't use this method. Instead use CommonAPI.")]
        public static T Copy<T>(this T obj) where T : class
        {
            Type TargetType = obj.GetType();
            object targetCopyObj = Activator.CreateInstance(TargetType);
            foreach (var field in TargetType.GetFields())
            {
                if (field.IsLiteral || field.IsStatic)
                {
                    continue;
                }
                else
                {
                    Traverse.Create(targetCopyObj).Field(field.Name).SetValue(Traverse.Create(obj).Field(field.Name).GetValue());
                }
            }
            foreach (var property in TargetType.GetProperties())
            {
                if (property.CanWrite && property.CanRead)
                {
                    Traverse.Create(targetCopyObj).Property(property.Name).SetValue(Traverse.Create(obj).Property(property.Name).GetValue());
                }
            }
            return targetCopyObj as T;
        }
    }
}