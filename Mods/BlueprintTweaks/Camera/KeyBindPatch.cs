using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace BlueprintTweaks
{
    public class CustomKeyBind
    {
        public bool keyValue
        { 
            get
            {
                if (!VFInput.override_keys[defaultBind.id].IsNull())
                {
                    return ReadKey(VFInput.override_keys[defaultBind.id]);
                }
                return ReadDefaultKey();
            }
        }

        public BuiltinKey defaultBind;

        public void Init(BuiltinKey defaultBind)
        {
            this.defaultBind = defaultBind;
        }

        protected virtual bool ReadDefaultKey()
        {
            return ReadKey(defaultBind.key);
        }

        protected virtual bool ReadKey(CombineKey key)
        {
            return key.GetKeyDown();
        }
    }

    public class HoldKeyBind : CustomKeyBind
    {
        
        protected override bool ReadKey(CombineKey key)
        {
            return key.GetKey();
        }
    }
    
    [HarmonyPatch]
    public static class KeyBindPatch
    {
        public static Dictionary<string, CustomKeyBind> customKeys = new Dictionary<string, CustomKeyBind>();
        
        public static void UpdateArray<T>(ref T[] array, int newSize)
        {
            T[] oldArray = array;
            array = new T[newSize];
            Array.Copy(oldArray, array, oldArray.Length);
        }

        public static void RegisterKeyBind<T>(BuiltinKey key)  where T : CustomKeyBind, new()
        {
            T keyBind = new T();
            keyBind.Init(key);
            customKeys.Add("KEY" + key.name, keyBind);
        }

        public static CustomKeyBind GetKeyBind(string id)
        {
            string key = "KEY" + id;
            if (customKeys.ContainsKey(key))
            {
                return customKeys[key];
            }

            return null;
        }

        public static void Init()
        {
            if (BlueprintTweaksPlugin.cameraToggleEnabled)
            {
                RegisterKeyBind<CustomKeyBind>(new BuiltinKey
                {
                    id = 100,
                    key = new CombineKey((int) KeyCode.J, 0, ECombineKeyAction.OnceClick, false),
                    conflictGroup = 3071,
                    name = "ToggleBPGodModeDesc",
                    canOverride = true
                });
            }

            if (BlueprintTweaksPlugin.forcePasteEnabled)
            {
                RegisterKeyBind<HoldKeyBind>(new BuiltinKey
                {
                    id = 101,
                    key = new CombineKey(0, 1, ECombineKeyAction.OnceClick, false),
                    conflictGroup = 2052,
                    name = "ForceBPPlace",
                    canOverride = true
                });
            }
        }
        
        [HarmonyPatch(typeof(UIOptionWindow), "_OnCreate")]
        [HarmonyPrefix]
        public static void AddKeyBind(UIOptionWindow __instance)
        {
            CustomKeyBind[] newKeys = customKeys.Values.ToArray();
            if (newKeys.Length == 0) return;
            
            int index = DSPGame.key.builtinKeys.Length;
            UpdateArray(ref DSPGame.key.builtinKeys, index + customKeys.Count);

            for (int i = 0; i < newKeys.Length; i++)
            {
                DSPGame.key.builtinKeys[index + i] = newKeys[i].defaultBind;
            }
        }
    }
}