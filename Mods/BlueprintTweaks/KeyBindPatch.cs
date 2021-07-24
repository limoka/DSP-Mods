using System;
using HarmonyLib;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace BlueprintTweaks
{
    [HarmonyPatch]
    public static class KeyBindPatch
    {
        public static bool toggleBpMode
        { 
            get
            {
                if (!VFInput.override_keys[100].IsNull())
                {
                    return VFInput.override_keys[100].GetKeyDown();
                }
                return Input.GetKeyDown(KeyCode.J);
            }
        }
        
        public static void UpdateArray<T>(ref T[] array, int newSize)
        {
            T[] oldArray = array;
            array = new T[newSize];
            Array.Copy(oldArray, array, oldArray.Length);
        }
        
        [HarmonyPatch(typeof(UIOptionWindow), "_OnCreate")]
        [HarmonyPrefix]
        public static void AddKeyBind(UIOptionWindow __instance)
        {
            int index = DSPGame.key.builtinKeys.Length;

            BuiltinKey key = new BuiltinKey
            {
                id = 100, 
                key = new CombineKey((int) KeyCode.J, 0, ECombineKeyAction.OnceClick, false), 
                conflictGroup = 3071, 
                name = "ToggleBPGodModeDesc",
                canOverride = true
            };

            UpdateArray(ref DSPGame.key.builtinKeys, index + 1);
             DSPGame.key.builtinKeys[index] = key;
        }
    }
}