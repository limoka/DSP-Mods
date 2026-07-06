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


namespace GigaStations
{
    public static class Preloader
    {
        public static ManualLogSource logSource;

        public static void Initialize()
        {
            logSource = Logger.CreateLogSource("GigaStations Preloader");
        }

        // List of assemblies to patch
        // ReSharper disable once InconsistentNaming
        public static IEnumerable<string> TargetDLLs { get; } = new[] { "Assembly-CSharp.dll" };

        // Patches the assemblies
        public static void Patch(AssemblyDefinition assembly)
        {
            try
            {
                ModuleDefinition gameModule = assembly.MainModule;
                
                TypeDefinition stationEntry = gameModule.Types.First(t => t.FullName == "UIControlPanelStationEntry");
                TypeDefinition storageItem = gameModule.Types.First(t => t.FullName == "UIControlPanelStorageItem");
                
                TypeReference listRawType = gameModule.ImportReference(typeof(List<>));
                
                GenericInstanceType listGenericType = new GenericInstanceType(listRawType);
                listGenericType.GenericArguments.Add(storageItem);

                FieldDefinition storageItemsField = new FieldDefinition("extraStorageItems", FieldAttributes.Private, listGenericType);
                stationEntry.Fields.Add(storageItemsField);
                
                // Bind the constructor specifically to List<UIControlPanelStorageItem>
                MethodReference listCtorGeneric = new MethodReference(".ctor", gameModule.TypeSystem.Void, listGenericType)
                {
                    HasThis = true,
                    ExplicitThis = false,
                    CallingConvention = MethodCallingConvention.Default
                };

                // --- Inject into _OnCreate ---
                MethodDefinition onCreateMethod = stationEntry.Methods.FirstOrDefault(m => m.Name == "_OnCreate");

                if (onCreateMethod != null)
                {
                    ILProcessor il = onCreateMethod.Body.GetILProcessor();
                    Instruction firstInstruction = onCreateMethod.Body.Instructions.First();

                    // Create the initialization instructions
                    Instruction ldarg0 = il.Create(OpCodes.Ldarg_0);                     // Load 'this' onto stack
                    Instruction newobj = il.Create(OpCodes.Newobj, listCtorGeneric);     // Call new List<UIControlPanelStorageItem>()
                    Instruction stfld  = il.Create(OpCodes.Stfld, storageItemsField);    // Store instance into this.storageItems

                    // Insert instructions at the very beginning of the method
                    il.InsertBefore(firstInstruction, ldarg0);
                    il.InsertAfter(ldarg0, newobj);
                    il.InsertAfter(newobj, stfld);
                }
                
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