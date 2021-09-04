using System;
using HarmonyLib;
using UnityEngine;

namespace BlueprintTweaks
{
    [HarmonyPatch]
    public static class UIBuildingGridPatch
    {
        public static Color selectColor = Color.magenta;
        public static Color displayColor = Color.blue;

        private static readonly int tintColor = Shader.PropertyToID("_TintColor");
        private static readonly int reformMode = Shader.PropertyToID("_ReformMode");
        private static readonly int zMin = Shader.PropertyToID("_ZMin");
        private static readonly int cursorGratBox = Shader.PropertyToID("_CursorGratBox");
        private static readonly int cursorBuffer = Shader.PropertyToID("_CursorBuffer");
        private static readonly int cursorColor = Shader.PropertyToID("_CursorColor");

        private static PlanetFactory currentFactory;
        private static byte[] fakeReformData;
        private static readonly int dataBuffer = Shader.PropertyToID("_DataBuffer");

        [HarmonyPatch(typeof(PlatformSystem), "SetReformType")]
        [HarmonyPatch(typeof(PlatformSystem), "SetReformColor")]
        [HarmonyPostfix]
        public static void UpdateFakeData(PlatformSystem __instance, int index)
        {
            if (currentFactory != null && currentFactory.index == __instance.factory.index)
            {
                fakeReformData[index] = __instance.reformData[index];
            }
        }


        [HarmonyPatch(typeof(UIBuildingGrid), "Update")]
        [HarmonyPostfix]
        public static void Update(UIBuildingGrid __instance)
        {
            if (!BlueprintCopyExtension.isEnabled) return;

            Player mainPlayer = GameMain.mainPlayer;

            PlanetFactory planetFactory = GameMain.localPlanet?.factory;
            if (planetFactory == null) return;
            
            if (GameMain.localPlanet.type == EPlanetType.Gas) return;

            PlayerAction_Build actionBuild = mainPlayer?.controller.actionBuild;
            if (actionBuild == null) return;

            if (__instance.reformCursorMap == null) return;

            int maxLen = __instance.reformCursorMap.Length;
            PlatformSystem system = planetFactory.platformSystem;
            
            if (system == null) return;
            system.EnsureReformData();

            float realRadius = GameMain.localPlanet.realRadius;
            __instance.displayScale = (realRadius + 0.2f) * 2f;

            if (actionBuild.blueprintMode == EBlueprintMode.None)
            {
                __instance.material.SetColor(cursorColor, Color.white);
                return;
            }

            if (currentFactory == null || currentFactory.index != planetFactory.index)
            {
                currentFactory = planetFactory;

                fakeReformData = new byte[system.maxReformCount];
                Array.Copy(system.reformData, fakeReformData, system.maxReformCount);
            }

            if (actionBuild.blueprintMode == EBlueprintMode.Copy)
            {
                bool any = false;
                if (BlueprintCopyExtension.reformSelection.Count > 0)
                {
                    __instance.material.SetColor(cursorColor, displayColor);
                    foreach (var kv in BlueprintCopyExtension.reformSelection)
                    {
                        int index = kv.Key;
                        if (index >= 0 && index < maxLen)
                        {
                            if (BlueprintCopyExtension.reformPreSelection.Count == 0)
                            {
                                fakeReformData[index] = 0;
                            }

                            __instance.reformCursorMap[index] = 1;
                        }
                    }
                    any = true;
                }

                if (BlueprintCopyExtension.reformPreSelection.Count > 0)
                {
                    __instance.material.SetColor(cursorColor, selectColor);

                    foreach (var kv in BlueprintCopyExtension.reformPreSelection)
                    {
                        int index = kv.Key;
                        if (index >= 0 && index < maxLen)
                        {
                            fakeReformData[index] = 0;
                            __instance.reformCursorMap[index] = 1;
                        }
                    }
                    any = true;
                }

                if (any)
                {
                    __instance.material.SetColor(tintColor, Color.clear);
                    __instance.gridRnd.enabled = true;
                    __instance.material.SetFloat(reformMode, 1f);
                    __instance.material.SetFloat(zMin, -1.5f);
                    __instance.reformCursorBuffer.SetData(__instance.reformCursorMap);
                    __instance.material.SetBuffer(cursorBuffer, __instance.reformCursorBuffer);

                    ComputeBuffer reformDataBuffer = system.reformDataBuffer;
                    reformDataBuffer.SetData(fakeReformData);
                    __instance.material.SetBuffer(dataBuffer, reformDataBuffer);

                    foreach (var kv in BlueprintCopyExtension.reformSelection)
                    {
                        int index = kv.Key;
                        if (index >= 0 && index < maxLen)
                        {
                            if (BlueprintCopyExtension.reformPreSelection.Count == 0)
                            {
                                fakeReformData[index] = system.reformData[index];
                            }

                            __instance.reformCursorMap[index] = 0;
                        }
                    }

                    foreach (var kv in BlueprintCopyExtension.reformPreSelection)
                    {
                        int index = kv.Key;
                        if (index >= 0 && index < maxLen)
                        {
                            fakeReformData[index] = system.reformData[index];
                            __instance.reformCursorMap[index] = 0;
                        }
                    }
                }
            }

            if (actionBuild.blueprintMode == EBlueprintMode.Paste)
            {
                ReformBPUtils.currentGrid = GameMain.localPlanet.aux.mainGrid;
                
                __instance.material.SetColor(cursorColor, displayColor);
                __instance.gridRnd.enabled = true;
                PlatformSystem platformSystem = __instance.reformMapPlanet.factory.platformSystem;
                if (BlueprintPasteExtension.reformPreviews.Count > 0)
                {
                    foreach (ReformData reformPreview in BlueprintPasteExtension.reformPreviews)
                    {
                        ReformBPUtils.GetSegmentCount(reformPreview.latitude, reformPreview.longitude, out float latCount, out float longCount);

                        int index = platformSystem.GetReformIndexForSegment(latCount, longCount);
                        if (index >= 0 && index < maxLen)
                        {
                            __instance.reformCursorMap[index] = 1;
                        }
                    }
                    
                    __instance.material.SetColor(tintColor, Color.clear);
                    __instance.material.SetFloat(reformMode, 1f);
                    __instance.material.SetFloat(zMin, -1.5f);
                    __instance.reformCursorBuffer.SetData(__instance.reformCursorMap);
                    __instance.material.SetBuffer(cursorBuffer, __instance.reformCursorBuffer);

                    foreach (ReformData reformPreview in BlueprintPasteExtension.reformPreviews)
                    {
                        ReformBPUtils.GetSegmentCount(reformPreview.latitude, reformPreview.longitude, out float latCount, out float longCount);
                        int index = platformSystem.GetReformIndexForSegment(latCount, longCount);
                        if (index >= 0 && index < maxLen)
                        {
                            __instance.reformCursorMap[index] = 0;
                        }
                    }
                }
            }
        }
    }
}