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
        public static IEnumerable<string> TargetDLLs { get; } = new[] {"Assembly-CSharp.dll"};
        
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

        public static TypeDefinition CreateMyType(AssemblyDefinition assembly)
        {
            TypeDefinition dataType = new TypeDefinition("BlueprintTweaks", "ReformData", TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.Public, assembly.MainModule.TypeSystem.Object);
            assembly.MainModule.Types.Add(dataType);
            dataType.BaseType = assembly.MainModule.TypeSystem.Object;
            
            FieldDefinition fld_ReformData_Latitude = new FieldDefinition("latitude", FieldAttributes.Public, assembly.MainModule.TypeSystem.Single);
            dataType.Fields.Add(fld_ReformData_Latitude);
            FieldDefinition fld_ReformData_Longitude = new FieldDefinition("longitude", FieldAttributes.Public, assembly.MainModule.TypeSystem.Single);
            dataType.Fields.Add(fld_ReformData_Longitude);
            var fld_ReformData_LocalLatitude = new FieldDefinition("localLatitude", FieldAttributes.Public, assembly.MainModule.TypeSystem.Single);
            dataType.Fields.Add(fld_ReformData_LocalLatitude);
            var fld_ReformData_LocalLongitude = new FieldDefinition("localLongitude", FieldAttributes.Public, assembly.MainModule.TypeSystem.Single);
            dataType.Fields.Add(fld_ReformData_LocalLongitude);
            FieldDefinition fld_ReformData_Type = new FieldDefinition("type", FieldAttributes.Public, assembly.MainModule.TypeSystem.Int32);
            dataType.Fields.Add(fld_ReformData_Type);
            FieldDefinition fld_ReformData_Color = new FieldDefinition("color", FieldAttributes.Public, assembly.MainModule.TypeSystem.Int32);
            dataType.Fields.Add(fld_ReformData_Color);
            var fld_ReformData_AreaIndex = new FieldDefinition("areaIndex", FieldAttributes.Public, assembly.MainModule.TypeSystem.Int32);
            dataType.Fields.Add(fld_ReformData_AreaIndex);

            //** Constructor: ReformData() **
            MethodDefinition ReformData_ctor_ = new MethodDefinition(".ctor", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName | MethodAttributes.SpecialName, assembly.MainModule.TypeSystem.Void);
            dataType.Methods.Add(ReformData_ctor_);
            ILProcessor il1 = ReformData_ctor_.Body.GetILProcessor();
            Instruction Ldarg_02 = il1.Create(OpCodes.Ldarg_0);
            il1.Append(Ldarg_02);
            Instruction Call3 = il1.Create(OpCodes.Call, assembly.MainModule.ImportReference(DefaultCtorFor(dataType.BaseType)));
            il1.Append(Call3);
            Instruction Ret4 = il1.Create(OpCodes.Ret);
            il1.Append(Ret4);

            return dataType;
        }
        
        // Patches the assemblies
        public static void Patch(AssemblyDefinition assembly)
        {
            ModuleDefinition gameModule = assembly.MainModule;
            TypeDefinition blueprintData = gameModule.Types.First (t => t.FullName == "BlueprintData");

            TypeReference color = gameModule.GetTypeReferences().First(t => t.FullName == "UnityEngine.Color");
            
            bool flag = blueprintData == null;
            if (flag)
            {
                logSource.LogInfo("Preloader patching failed!");
                return;
            }

            TypeDefinition reformData = CreateMyType(assembly);

            blueprintData.Fields.Add(new FieldDefinition("reforms", FieldAttributes.Public, reformData.MakeArrayType()));
            
            blueprintData.Fields.Add(new FieldDefinition("customColors", FieldAttributes.Public, color.MakeArrayType()));

            logSource.LogInfo("Preloader patching is successful!");
        }
    }
}