using System;
using System.Collections.Generic;
using System.Linq;

namespace xiaoye97
{
    public static class ProtoIndex
    {
        private static Dictionary<Type, int> index = new Dictionary<Type, int>();
        private static Type[] protoTypes;
        
        internal static void InitIndex()
        {
            LDBToolPlugin.logger.LogDebug($"Generating Proto type list:");
            protoTypes = (
                from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                from assemblyType in domainAssembly.GetTypes()
                where typeof(Proto).IsAssignableFrom(assemblyType) && assemblyType != typeof(Proto)
                select assemblyType
                ).ToArray();

            for (int i = 0; i < protoTypes.Length; i++)
            {
                index.Add(protoTypes[i], i);
                LDBToolPlugin.logger.LogDebug($"Found Proto type: {protoTypes[i].FullName}");
            }
        }

        public static int GetProtosCount()
        {
            return index.Count;
        }
        
        public static int GetIndex(Proto proto)
        {
            return GetIndex(proto.GetType());
        }

        public static int GetIndex(Type type)
        {
            if (!typeof(Proto).IsAssignableFrom(type))
            {
                throw new ArgumentException($"Can't get index because type {type.FullName} does not extend Proto type.");
            }

            if (index.ContainsKey(type))
            {
                return index[type];
            }
            
            throw new ArgumentException($"Unknown Proto type: {type.FullName}");
        }

        public static string GetProtoName(Proto proto)
        {
            return proto.GetType().Name.Replace("Proto", "");
        }

        internal static Type[] GetAllProtoTypes()
        {
            return protoTypes;
        }
        
    }
}