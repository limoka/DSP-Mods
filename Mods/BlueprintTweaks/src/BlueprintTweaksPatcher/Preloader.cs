using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using TypeAttributes = Mono.Cecil.TypeAttributes;


namespace BlueprintTweaks
{
    public static class Preloader
    {
        public static ManualLogSource logSource;

        public static void Initialize()
        {
            logSource = Logger.CreateLogSource("BlueprintTweaks Preloader");
        }

        // List of assemblies to patch
        // ReSharper disable once InconsistentNaming
        public static IEnumerable<string> TargetDLLs { get; } = new[] { "Assembly-CSharp.dll" };

        public static MethodReference DefaultCtorFor(TypeReference type)
        {
            var resolved = type.Resolve();
            if (resolved == null)
                return null;

            var ctor = resolved.Methods.SingleOrDefault(m => m.IsConstructor && m.Parameters.Count == 0 && !m.IsStatic);
            if (ctor == null)
                return DefaultCtorFor(resolved.BaseType);

            return new MethodReference(".ctor", type.Module.TypeSystem.Void, type) { HasThis = true };
        }

        // Patches the assemblies
        public static void Patch(AssemblyDefinition assembly)
        {
            try
            {
                ModuleDefinition gameModule = assembly.MainModule;
                TypeDefinition blueprintData = gameModule.Types.First(t => t.FullName == "BlueprintData");

                //TypeReference color = gameModule.GetTypeReferences().First(t => t.FullName == "UnityEngine.Color");

                TypeDefinition assemblerComponent = gameModule.Types.First(t => t.FullName == "AssemblerComponent");
                TypeDefinition labComponent = gameModule.Types.First(t => t.FullName == "LabComponent");

                //blueprintData.Fields.Add(new FieldDefinition("customColors", FieldAttributes.Public, color.MakeArrayType()));

                blueprintData.Fields.Add(new FieldDefinition("anchorType", FieldAttributes.Public, gameModule.ImportReference(typeof(int))));
                
                blueprintData.Fields.Add(new FieldDefinition("autoReformMode", FieldAttributes.Public, gameModule.ImportReference(typeof(int))));

                assemblerComponent.Fields.Add(new FieldDefinition("recipeIsLocked", FieldAttributes.Public, gameModule.ImportReference(typeof(bool))));
                labComponent.Fields.Add(new FieldDefinition("recipeIsLocked", FieldAttributes.Public, gameModule.ImportReference(typeof(bool))));


                logSource.LogInfo("Preloader patching is successful!");
            }
            catch (Exception)
            {
                logSource.LogError("Preloader patching failed!");
                throw;
            }
        }
    }
}