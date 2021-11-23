using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace MinerPreloader
{
    public static class Preloader
    {
        public static ManualLogSource logSource;

        public static void Initialize()
        {
            logSource = Logger.CreateLogSource("Advanced Miner Preloader");
        }

        // List of assemblies to patch
        // ReSharper disable once InconsistentNaming
        public static IEnumerable<string> TargetDLLs { get; } = new[] {"Assembly-CSharp.dll"};

        // Patches the assemblies
        public static void Patch(AssemblyDefinition assembly)
        {
            ModuleDefinition gameModule = assembly.MainModule;
            TypeDefinition MinerComponent = gameModule.Types.First(t => t.FullName == "MinerComponent");
            

            bool flag = MinerComponent == null;
            if (flag)
            {
                logSource.LogInfo("Preloader patching failed!");
                return;
            }

            MinerComponent.Fields.Add(new FieldDefinition("insertTarget2", FieldAttributes.Public, gameModule.ImportReference(typeof(int))));
            MinerComponent.Fields.Add(new FieldDefinition("insertTarget3", FieldAttributes.Public, gameModule.ImportReference(typeof(int))));
            MinerComponent.Fields.Add(new FieldDefinition("lastUsedPort", FieldAttributes.Public, gameModule.ImportReference(typeof(int))));

            
            logSource.LogInfo("Preloader patching is successful!");
        }
    }
}