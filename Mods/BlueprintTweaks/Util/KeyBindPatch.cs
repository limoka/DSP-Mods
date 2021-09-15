using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace BlueprintTweaks
{
    public class PressKeyBind
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

    public class HoldKeyBind : PressKeyBind
    {
        protected override bool ReadKey(CombineKey key)
        {
            return key.GetKey();
        }
    }

    public class ReleaseKeyBind : PressKeyBind
    {
        protected override bool ReadKey(CombineKey key)
        {
            return key.GetKeyUp();
        }
    }

    [HarmonyPatch]
    public static class KeyBindPatch
    {
        public static Dictionary<string, PressKeyBind> customKeys = new Dictionary<string, PressKeyBind>();

        public static void UpdateArray<T>(ref T[] array, int newSize)
        {
            T[] oldArray = array;
            array = new T[newSize];
            Array.Copy(oldArray, array, oldArray.Length);
        }

        public static void RegisterKeyBind<T>(BuiltinKey key) where T : PressKeyBind, new()
        {
            T keyBind = new T();
            keyBind.Init(key);
            customKeys.Add("KEY" + key.name, keyBind);
        }

        public static bool HasKeyBind(string id)
        {
            string key = "KEY" + id;
            return customKeys.ContainsKey(key);
        }

        public static PressKeyBind GetKeyBind(string id)
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
            if (BlueprintTweaksPlugin.cameraToggleEnabled.Value)
            {
                RegisterKeyBind<PressKeyBind>(new BuiltinKey
                {
                    id = 100,
                    key = new CombineKey((int) KeyCode.J, 0, ECombineKeyAction.OnceClick, false),
                    conflictGroup = 3071,
                    name = "ToggleBPGodModeDesc",
                    canOverride = true
                });
            }

            if (BlueprintTweaksPlugin.forcePasteEnabled.Value)
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

            if (BlueprintTweaksPlugin.axisLockEnabled.Value)
            {
                RegisterKeyBind<ReleaseKeyBind>(new BuiltinKey
                {
                    id = 102,
                    key = new CombineKey((int) KeyCode.G, CombineKey.CTRL_COMB, ECombineKeyAction.OnceClick, false),
                    conflictGroup = 2052,
                    name = "LockLongAxis",
                    canOverride = true
                });

                RegisterKeyBind<ReleaseKeyBind>(new BuiltinKey
                {
                    id = 103,
                    key = new CombineKey((int) KeyCode.T, CombineKey.CTRL_COMB, ECombineKeyAction.OnceClick, false),
                    conflictGroup = 2052,
                    name = "LockLatAxis",
                    canOverride = true
                });
            }

            if (BlueprintTweaksPlugin.gridControlFeature.Value)
            {
                RegisterKeyBind<ReleaseKeyBind>(new BuiltinKey
                {
                    id = 104,
                    key = new CombineKey((int) KeyCode.B, CombineKey.CTRL_COMB, ECombineKeyAction.OnceClick, false),
                    conflictGroup = 2052,
                    name = "SetLocalOffset",
                    canOverride = true
                });
            }

            if (BlueprintTweaksPlugin.blueprintMirroring.Value)
            {
                RegisterKeyBind<ReleaseKeyBind>(new BuiltinKey
                {
                    id = 105,
                    key = new CombineKey((int) KeyCode.G, CombineKey.SHIFT_COMB, ECombineKeyAction.OnceClick, false),
                    conflictGroup = 2052,
                    name = "MirrorLongAxis",
                    canOverride = true
                });

                RegisterKeyBind<ReleaseKeyBind>(new BuiltinKey
                {
                    id = 106,
                    key = new CombineKey((int) KeyCode.T, CombineKey.SHIFT_COMB, ECombineKeyAction.OnceClick, false),
                    conflictGroup = 2052,
                    name = "MirrorLatAxis",
                    canOverride = true
                });
            }
        }

        [HarmonyPatch(typeof(UIOptionWindow), "_OnCreate")]
        [HarmonyPrefix]
        public static void AddKeyBind(UIOptionWindow __instance)
        {
            PressKeyBind[] newKeys = customKeys.Values.ToArray();
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