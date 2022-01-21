using System;
using System.Collections.Generic;
using CommonAPI;
using HarmonyLib;
using NebulaAPI;
using UnityEngine;

namespace BlueprintTweaks
{
    [RegisterPatch(BlueprintTweaksPlugin.BLUEPRINT_FOUNDATIONS)]
    public static class BlueprintPasteExtension
    {
        public static List<ReformData> reformPreviews = new List<ReformData>();
        public static List<Vector3> tmpPoints = new List<Vector3>();

        public static int tickCounter;
        public static int lastCost;
        
        public static void InitPreviews(BlueprintData _blueprintData, int _dotsCursor)
        {
            int num = _blueprintData.reforms.Length;
            int num2 = (_blueprintData.areas.Length > 1) ? num : (num * _dotsCursor);

            if (reformPreviews.Capacity < num2)
            {
                reformPreviews.Capacity = num2;
            }

            for (int i = 0; i < num2; i++)
            {
                if (i >= reformPreviews.Count)
                {
                    reformPreviews.Add(new ReformData());
                }

                int num4 = i % num;
                reformPreviews[i].areaIndex = _blueprintData.reforms[num4].areaIndex;
                reformPreviews[i].localLatitude = _blueprintData.reforms[num4].localLatitude;
                reformPreviews[i].localLongitude = _blueprintData.reforms[num4].localLongitude;
                reformPreviews[i].type = _blueprintData.reforms[num4].type;
                reformPreviews[i].color = _blueprintData.reforms[num4].color;
            }

            for (int i = reformPreviews.Count - 1; i >= num2; i--)
            {
                reformPreviews.RemoveAt(i);
            }
        }


        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), "ResetBuildPreviews")]
        [HarmonyPrefix]
        public static void ResetBuildPreviews(BuildTool_BlueprintPaste __instance)
        {
            reformPreviews.Clear();
        }

        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), "DeterminePreviewsPrestage")]
        [HarmonyPrefix]
        public static void DeterminePrestage(BuildTool_BlueprintPaste __instance, bool _forceRefreshBP = false)
        {
            int reformsLength = __instance.blueprint.reforms.Length;
            int totalNeeded = (__instance.blueprint.areas.Length > 1) ? reformsLength : (reformsLength * __instance.dotsCursor);
            if (__instance.bpPool == null || reformPreviews.Count != totalNeeded || __instance.drag_box_size_changed || _forceRefreshBP)
            {
                InitPreviews(__instance.blueprint, __instance.dotsCursor);
            }
        }

        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), "_OnTick")]
        [HarmonyPostfix]
        public static void OnUpdate(BuildTool_BlueprintPaste __instance)
        {
            if (!BlueprintCopyExtension.isEnabled) return;
            if (reformPreviews.Count <= 0) return;
            if (__instance.cannotBuild) return;

            tickCounter++;
            if (tickCounter >= 30)
            {
                tickCounter = 0;
                Vector3 center = Vector3.zero;
                tmpPoints.Clear();
            
                foreach (ReformData preview in reformPreviews)
                {
                    Vector3 pos = BlueprintUtils.GetDir(preview.longitude, preview.latitude);
                    pos *= GameMain.localPlanet.realRadius + 0.2f;
                    tmpPoints.Add(pos);
                    center += pos;
                }

                lastCost = ReformBPUtils.ComputeFlattenTerrainReform(__instance.factory, tmpPoints, center);
            }
            

            if (__instance.cursorValid && !VFInput.onGUIOperate)
            {
                if (lastCost > 0)
                {
                    __instance.actionBuild.model.cursorText = $"{"沙土消耗".Translate()} {lastCost} {"个沙土".Translate()}";
                }
                else if (lastCost == 0)
                {
                    __instance.actionBuild.model.cursorText = "改造大小".Translate();
                }
                else
                {
                    int num2 = -lastCost;
                    __instance.actionBuild.model.cursorText = $"{"沙土获得".Translate()} {num2} {"个沙土".Translate()}";
                }
            }
        }
        

        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), "CheckBuildConditions")]
        [HarmonyPrefix]
        public static void PasteReformsPrefix(BuildTool_BlueprintPaste __instance, ref bool __state)
        {
            __state = false;
            if (!BlueprintCopyExtension.isEnabled) return;
            if (NebulaModAPI.IsMultiplayerActive && NebulaModAPI.MultiplayerSession.Factories.IsIncomingRequest.Value) return;

            Color[] colors = null;
            
            if (BlueprintCopyExtension.copyColors && __instance.blueprint.customColors != null && __instance.blueprint.customColors.Length > 0)
            {
                colors = __instance.blueprint.customColors;
            }
            
            __state = CalculatePositions(__instance, reformPreviews, colors);
        }

        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), "CheckBuildConditions")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.First)]
        public static void CheckAfter(BuildTool_BlueprintPaste __instance, ref bool __result, ref bool __state)
        {
            if (__result) return;
            if (!BlueprintCopyExtension.isEnabled || !__state) return;
            if (!__instance.cannotBuild || reformPreviews.Count <= 0) return;
            
            if (NebulaModAPI.IsMultiplayerActive && NebulaModAPI.MultiplayerSession.Factories.IsIncomingRequest.Value) return;

            BPGratBox box = ReformBPUtils.GetBoundingRange(__instance.planet, __instance.actionBuild.planetAux, new int[0], 0, reformPreviews, reformPreviews[0].longitude);

            bool allOk = true; 

            for (int i = 0; i < __instance.bpCursor; i++)
            {
                BuildPreview preview = __instance.bpPool[i];
                if (preview.condition == EBuildCondition.NeedGround)
                {
                    Vector3 pos = (preview.lpos + preview.lpos2) * 0.5f;

                    if (box.InGratBox(pos))
                    {
                        preview.condition = EBuildCondition.Ok;
                    }
                }
                
                if (preview.condition != EBuildCondition.Ok && preview.condition != EBuildCondition.NotEnoughItem) allOk = false;
            }

            if (allOk)
            {
                __result = true;
            }
        }

        public static void ApplyColors(BuildTool_BlueprintPaste tool, Color[] colors)
        {
            PlatformSystem system = tool.factory.platformSystem;
            Array.Copy(colors, system.reformCustomColors, 16);
            system.RefreshColorsTexture();
        }
        

        public static bool CalculatePositions(BuildTool_BlueprintPaste tool, List<ReformData> reforms, Color[] colors)
        {
            ReformBPUtils.currentGrid = tool.factory.planet.aux.mainGrid;
            
            Vector3 center = Vector3.zero;
            tmpPoints.Clear();

            foreach (ReformData preview in reforms)
            {
                Vector3 pos = BlueprintUtils.GetDir(preview.longitude, preview.latitude);
                pos *= GameMain.localPlanet.realRadius + 0.2f;
                tmpPoints.Add(pos);
                center += pos;
            }
            
            int cost = ReformBPUtils.ComputeFlattenTerrainReform(tool.factory, tmpPoints, center);

            if (NebulaModAPI.IsMultiplayerActive)
            {
                IMultiplayerSession session = NebulaModAPI.MultiplayerSession;
                if (!session.Factories.IsIncomingRequest.Value && !CheckItems(tool, cost, tmpPoints.Count)) return false;

                if (session.LocalPlayer.IsHost)
                {
                    int planetId = session.Factories.EventFactory?.planetId ?? GameMain.localPlanet?.id ?? -1;
                    session.Network.SendPacketToStar(new ReformPasteEventPacket(planetId, reforms, colors, session.Factories.PacketAuthor == NebulaModAPI.AUTHOR_NONE ? session.LocalPlayer.Id : session.Factories.PacketAuthor), GameMain.galaxy.PlanetById(planetId).star.id);
                }

                //If client builds, he need to first send request to the host and wait for reply
                if (!session.LocalPlayer.IsHost && !session.Factories.IsIncomingRequest.Value)
                {
                    session.Network.SendPacket(new ReformPasteEventPacket(GameMain.localPlanet?.id ?? -1, reforms, colors, session.Factories.PacketAuthor == NebulaModAPI.AUTHOR_NONE ? session.LocalPlayer.Id : session.Factories.PacketAuthor));
                    return true;
                }
            }
            else
            {
                if (!CheckItems(tool, cost, tmpPoints.Count)) return false;
            }

            if (colors != null && colors.Length > 0)
            {
                ApplyColors(tool, colors);
            }

            ReformBPUtils.FlattenTerrainReform(tool.factory, tmpPoints, center);
            VFAudio.Create("reform-terrain", null, center, true, 4);
            PlatformSystem platformSystem = tool.factory.platformSystem;

            foreach (ReformData preview in reforms)
            {
                ReformBPUtils.GetSegmentCount(preview.latitude, preview.longitude, out float latCount, out float longCount, out int segmentCount);
                longCount = Mathf.Repeat(longCount, segmentCount);
                
                int reformIndex = platformSystem.GetReformIndexForSegment(latCount, longCount);

                if (reformIndex < 0) continue;
                
                int reformType = platformSystem.GetReformType(reformIndex);
                int reformColor = platformSystem.GetReformColor(reformIndex);
                if (reformType == preview.type && reformColor == preview.color) continue;
                
                platformSystem.SetReformType(reformIndex, preview.type);
                platformSystem.SetReformColor(reformIndex, preview.color);
            }

            return true;
        }

        public static bool CheckItems(BuildTool_BlueprintPaste tool, int cost, int reformCount)
        {
            if (BlueprintTweaksPlugin.freeFoundationsIsInstalled) return true;
            
            if (tool.player.package.GetItemCount(PlatformSystem.REFORM_ID) < reformCount) return false;

            int result = tool.player.sandCount - cost;
            if (result <= 0) return false;

            tool.player.package.TakeItem(PlatformSystem.REFORM_ID, reformCount, out int _);
            tool.player.SetSandCount(result);

            return true;
        }
    }
}