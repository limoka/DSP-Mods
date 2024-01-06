using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CommonAPI;
using HarmonyLib;
using NebulaAPI;
using UnityEngine;
using static System.String;

namespace BlueprintTweaks
{
    [RegisterPatch(BlueprintTweaksPlugin.BLUEPRINT_FOUNDATIONS)]
    public static class BlueprintPasteExtension
    {
        public static List<ReformData> reformPreviews = new List<ReformData>();
        public static List<ReformData> runtimeReforms = new List<ReformData>();
        
        private static int lastAutoEstimate;
        private static AutoReformMode lastReformMode = AutoReformMode.None;

        private static List<Vector3> tmpPoints = new List<Vector3>();
        private static HashSet<Vector3Int> occupied = new HashSet<Vector3Int>();

        private static int tickCounter;
        private static int lastCost;

        public static void InitPreviews(BlueprintData blueprint, int _dotsCursor)
        {
            int reformCount = runtimeReforms.Count;
            int totalCount = (blueprint.areas.Length > 1) ? reformCount : (reformCount * _dotsCursor);

            if (reformPreviews.Capacity < totalCount)
            {
                reformPreviews.Capacity = totalCount;
            }

            for (int i = 0; i < totalCount; i++)
            {
                int subIndex = i % reformCount;
                ReformData subReform = runtimeReforms[subIndex];

                if (i >= reformPreviews.Count)
                {
                    reformPreviews.Add(new ReformData());
                }
                
                reformPreviews[i].areaIndex = subReform.areaIndex;
                reformPreviews[i].localLatitude = subReform.localLatitude;
                reformPreviews[i].localLongitude = subReform.localLongitude;
                reformPreviews[i].type = subReform.type;
                reformPreviews[i].color = subReform.color;
            }

            for (int i = reformPreviews.Count - 1; i >= totalCount; i--)
            {
                reformPreviews.RemoveAt(i);
            }
        }


        private static Vector2 ToVector2(this Vector3 vec)
        {
            return new Vector2(vec.x, vec.z);
        }

        private static Vector2Int Floor(this Vector2 vec)
        {
            return new Vector2Int(Mathf.FloorToInt(vec.x), Mathf.FloorToInt(vec.y));
        }

        private static Vector2Int Ceil(this Vector2 vec)
        {
            return new Vector2Int(Mathf.CeilToInt(vec.x), Mathf.CeilToInt(vec.y));
        }

        public static void RefreshAutomaticReforms(BlueprintData blueprint, AutoReformMode autoReformMode)
        {
            if (autoReformMode == AutoReformMode.None)
            {
                runtimeReforms.Clear();
                runtimeReforms.AddRange(blueprint.reforms);
                return;
            }

            int estimatedCount = blueprint.reforms.Length + blueprint.buildings.Length;

            if (runtimeReforms.Capacity < estimatedCount)
            {
                runtimeReforms.Capacity = estimatedCount;
            }

            int length = 0;

            switch (autoReformMode)
            {
                case AutoReformMode.Sparse:
                case AutoReformMode.UnderBuildings:

                    length = DetermineCoverFoundations(blueprint, autoReformMode);
                    break;
                case AutoReformMode.Filled:
                    
                    length = DetermineAreaCoverFoundations(blueprint);
                    break;
            }

            for (int i = runtimeReforms.Count - 1; i >= length; i--)
            {
                runtimeReforms.RemoveAt(i);
            }
        }

        private static int DetermineCoverFoundations(BlueprintData blueprint, AutoReformMode autoReformMode)
        {
            int index = 0;
            occupied.Clear();

            foreach (ReformData reform in blueprint.reforms)
            {
                if (index >= runtimeReforms.Count)
                {
                    runtimeReforms.Add(new ReformData());
                }

                runtimeReforms[index].areaIndex = reform.areaIndex;
                runtimeReforms[index].localLongitude = reform.localLongitude;
                runtimeReforms[index].localLatitude = reform.localLatitude;
                runtimeReforms[index].type = reform.type;
                runtimeReforms[index].color = reform.color;

                occupied.Add(new Vector3Int(reform.areaIndex, Mathf.RoundToInt(reform.localLongitude), Mathf.RoundToInt(reform.localLatitude)));
                index++;
            }

            int burshType = GameMain.data.preferences.reformBrushType;
            int brushColor = GameMain.data.preferences.reformBrushColor;

            foreach (BlueprintBuilding building in blueprint.buildings)
            {
                if (building.localOffset_z > 0.5f) continue;

                var item = LDB.items.Select(building.itemId);
                if (item?.prefabDesc == null) continue;
                
                if (item.prefabDesc.landPoints.Length == 0 && 
                    autoReformMode == AutoReformMode.Sparse) continue;

                var buildCollider = item.prefabDesc.buildCollider;

                Vector2 extent = buildCollider.ext.ToVector2();
                Vector2 rotatedExtent = BlueprintUtils.TransitionWidthAndHeight(building.yaw, extent.x, extent.y);

                Vector2 min = buildCollider.pos.ToVector2() - rotatedExtent;
                Vector2 max = buildCollider.pos.ToVector2() + rotatedExtent;

                Vector2Int minInt = min.Floor();
                Vector2Int maxInt = max.Ceil();

                for (int x = minInt.x; x < maxInt.x; x++)
                {
                    for (int y = minInt.y; y < maxInt.y; y++)
                    {
                        float reformX = building.localOffset_x + x + 1;
                        float reformY = building.localOffset_y + y + 1;

                        var posKey = new Vector3Int(building.areaIndex, Mathf.RoundToInt(reformX), Mathf.RoundToInt(reformY));
                        if (occupied.Contains(posKey)) continue;

                        if (index >= runtimeReforms.Count)
                        {
                            runtimeReforms.Add(new ReformData());
                        }

                        runtimeReforms[index].areaIndex = building.areaIndex;
                        runtimeReforms[index].localLongitude = reformX;
                        runtimeReforms[index].localLatitude = reformY;
                        runtimeReforms[index].type = burshType;
                        runtimeReforms[index].color = brushColor;

                        occupied.Add(posKey);
                        index++;
                    }
                }
            }

            return index;
        }

        private static int DetermineAreaCoverFoundations(BlueprintData blueprint)
        {
            int index = 0;
            
            int burshType = GameMain.data.preferences.reformBrushType;
            int brushColor = GameMain.data.preferences.reformBrushColor;

            foreach (BlueprintArea area in blueprint.areas)
            {
                for (int x = 0; x <= area.width; x++)
                {
                    for (int y = 0; y <= area.height; y++)
                    {
                        if (index >= runtimeReforms.Count)
                        {
                            runtimeReforms.Add(new ReformData());
                        }

                        runtimeReforms[index].areaIndex = area.index;
                        runtimeReforms[index].localLongitude = x;
                        runtimeReforms[index].localLatitude = y;
                        runtimeReforms[index].type = burshType;
                        runtimeReforms[index].color = brushColor;
                        index++;
                        
                    }
                }
            }

            return index;
        }
        
        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), "ResetBuildPreviews")]
        [HarmonyPrefix]
        public static void ResetBuildPreviews(BuildTool_BlueprintPaste __instance)
        {
            reformPreviews.Clear();
            runtimeReforms.Clear();
            lastAutoEstimate = 0;
            lastReformMode = AutoReformMode.None;
        }

        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), "DeterminePreviewsPrestage")]
        [HarmonyPrefix]
        public static void DeterminePrestage(BuildTool_BlueprintPaste __instance, bool _forceRefreshBP = false)
        {
            int buildingsLength = __instance.blueprint.buildings.Length;
            AutoReformMode autoReformMode = (AutoReformMode)__instance.blueprint.autoReformMode;
            if (lastReformMode != autoReformMode || lastAutoEstimate != buildingsLength || _forceRefreshBP)
            {
                RefreshAutomaticReforms(__instance.blueprint, autoReformMode);
                lastAutoEstimate = buildingsLength;
                lastReformMode = autoReformMode;
            }
            
            int reformsLength = __instance.blueprint.reforms.Length + runtimeReforms.Count;
            int totalNeeded = (__instance.blueprint.areas.Length > 1) ? reformsLength : (reformsLength * __instance.dotsCursor);
            if (__instance.bpPool == null || reformPreviews.Count != totalNeeded || __instance.drag_box_size_changed || _forceRefreshBP)
            {
                InitPreviews(__instance.blueprint, __instance.dotsCursor);
            }
        }


        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), "DeterminePreviewsPrestage")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> FixZeroComputeBuffer(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            FieldInfo bpCursor = AccessTools.Field(typeof(BuildTool_BlueprintPaste), nameof(BuildTool_BlueprintPaste.bpCursor));
            FieldInfo bpSignBuffer = AccessTools.Field(typeof(BuildTool_BlueprintPaste), nameof(BuildTool_BlueprintPaste.bpSignBuffer));

            CodeMatcher matcher = new CodeMatcher(instructions, generator)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, bpCursor),
                    new CodeMatch(OpCodes.Ldc_I4_S),
                    new CodeMatch(OpCodes.Ldc_I4_0),
                    new CodeMatch(OpCodes.Newobj)
                )
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldnull))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Stfld, bpSignBuffer))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, bpCursor));

            int jumpPos = matcher.Clone()
                .MatchForward(false, new CodeMatch(OpCodes.Stfld)).Advance(1).Pos;

            matcher.InsertBranchAndAdvance(OpCodes.Brfalse, jumpPos)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0));


            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), "ResetBuildPreviews")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> FixZeroComputeBuffer1(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            FieldInfo signPool = AccessTools.Field(typeof(BuildTool_BlueprintPaste), nameof(BuildTool_BlueprintPaste.signPool));
            FieldInfo bpSignBuffer = AccessTools.Field(typeof(BuildTool_BlueprintPaste), nameof(BuildTool_BlueprintPaste.bpSignBuffer));
            Label? label = default;

            CodeMatcher matcher = new CodeMatcher(instructions, generator)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, signPool),
                    new CodeMatch(x => x.Branches(out label))
                ).Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, bpSignBuffer))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse, label));

            return matcher.InstructionEnumeration();
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

                PlatformSystem platformSystem = __instance.factory.platformSystem;

                foreach (ReformData preview in reformPreviews)
                {
                    ReformBPUtils.GetSegmentCount(preview.latitude, preview.longitude, out float latCount, out float longCount, out int segmentCount);
                    longCount = Mathf.Repeat(longCount, segmentCount);

                    int reformIndex = platformSystem.GetReformIndexForSegment(latCount, longCount);
                    if (reformIndex < 0) continue;

                    int type = platformSystem.GetReformType(reformIndex);
                    if (platformSystem.IsTerrainReformed(type)) continue;

                    Vector3 pos = BlueprintUtils.GetDir(preview.longitude, preview.latitude);
                    pos *= GameMain.localPlanet.realRadius + 0.2f;
                    tmpPoints.Add(pos);
                    center += pos;
                }

                lastCost = ReformBPUtils.ComputeFlattenTerrainReform(__instance.factory, tmpPoints, center);
            }

            string message = "";
            int playerFoundationCount = __instance.player.package.GetItemCount(PlatformSystem.REFORM_ID);
            bool isError = false;

            if (playerFoundationCount < tmpPoints.Count)
            {
                message = Format("NotEnoughFoundationsMessage".Translate(), tmpPoints.Count - playerFoundationCount) + "\n";
                isError = true;
            }
            else
            {
                message = Format("FoundCountMessage".Translate(), tmpPoints.Count) + "\n";
            }


            if (__instance.cursorValid && !VFInput.onGUIOperate)
            {
                if (lastCost > 0)
                {
                    __instance.actionBuild.model.cursorText = $"{message}{"沙土消耗".Translate()} {lastCost} {"个沙土".Translate()}";
                }
                else if (lastCost == 0)
                {
                    __instance.actionBuild.model.cursorText = $"{message}";
                }
                else
                {
                    int num2 = -lastCost;
                    __instance.actionBuild.model.cursorText = $"{message}{"沙土获得".Translate()} {num2} {"个沙土".Translate()}";
                }

                if (isError)
                {
                    __instance.actionBuild.model.cursorState = -1;
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

            BPGratBox box = ReformBPUtils.GetBoundingRange(__instance.planet, __instance.actionBuild.planetAux, Array.Empty<int>(), 0, reformPreviews,
                reformPreviews[0].longitude);

            bool allOk = true;
            bool allGroundOk = true;

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

                if (preview.condition == EBuildCondition.NeedGround) allGroundOk = false;
                if (preview.condition != EBuildCondition.Ok && preview.condition != EBuildCondition.NotEnoughItem) allOk = false;
            }
            
            if (allGroundOk)
            {
                __instance._tmp_error_types.Remove(EBuildCondition.NeedGround);
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
            PlanetData planet = tool.factory.planet;
            PlatformSystem platformSystem = tool.factory.platformSystem;

            Vector3 center = Vector3.zero;
            tmpPoints.Clear();

            foreach (ReformData preview in reforms)
            {
                ReformBPUtils.GetSegmentCount(preview.latitude, preview.longitude, out float latCount, out float longCount, out int segmentCount);
                longCount = Mathf.Repeat(longCount, segmentCount);

                int reformIndex = platformSystem.GetReformIndexForSegment(latCount, longCount);
                if (reformIndex < 0) continue;

                int type = platformSystem.GetReformType(reformIndex);
                if (platformSystem.IsTerrainReformed(type)) continue;

                Vector3 pos = BlueprintUtils.GetDir(preview.longitude, preview.latitude);
                pos *= planet.realRadius + 0.2f;
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
                    session.Network.SendPacketToStar(
                        new ReformPasteEventPacket(planetId, reforms, colors,
                            session.Factories.PacketAuthor == NebulaModAPI.AUTHOR_NONE ? session.LocalPlayer.Id : session.Factories.PacketAuthor),
                        GameMain.galaxy.PlanetById(planetId).star.id);
                }

                //If client builds, he need to first send request to the host and wait for reply
                if (!session.LocalPlayer.IsHost && !session.Factories.IsIncomingRequest.Value)
                {
                    session.Network.SendPacket(new ReformPasteEventPacket(GameMain.localPlanet?.id ?? -1, reforms, colors,
                        session.Factories.PacketAuthor == NebulaModAPI.AUTHOR_NONE ? session.LocalPlayer.Id : session.Factories.PacketAuthor));
                    return true;
                }
            }
            else
            {
                if (!CheckItems(tool, cost, tmpPoints.Count))
                {
                    UIRealtimeTip.Popup("沙土不足".Translate());
                    return false;
                }
            }

            if (colors != null && colors.Length > 0)
            {
                ApplyColors(tool, colors);
            }

            ReformBPUtils.FlattenTerrainReform(tool.factory, tmpPoints, center);
            VFAudio.Create("reform-terrain", null, center, true, 4);

            foreach (ReformData preview in reforms)
            {
                ReformBPUtils.GetSegmentCount(preview.latitude, preview.longitude, out float latCount, out float longCount, out int segmentCount);
                longCount = Mathf.Repeat(longCount, segmentCount);

                int reformIndex = platformSystem.GetReformIndexForSegment(latCount, longCount);

                if (reformIndex >= 0 && reformIndex < platformSystem.reformData.Length)
                {
                    int reformType = platformSystem.GetReformType(reformIndex);
                    int reformColor = platformSystem.GetReformColor(reformIndex);
                    if (reformType == preview.type && reformColor == preview.color) continue;

                    platformSystem.SetReformType(reformIndex, preview.type);
                    platformSystem.SetReformColor(reformIndex, preview.color);
                }
            }

            return true;
        }

        public static bool CheckItems(BuildTool_BlueprintPaste tool, int cost, int reformCount)
        {
            if (BlueprintTweaksPlugin.freeFoundationsIsInstalled) return true;
            if (GameMain.data.history.HasFeatureKey(1100001) && GameMain.sandboxToolsEnabled) return true;
            
            if (tool.player.package.GetItemCount(PlatformSystem.REFORM_ID) < reformCount) return false;

            long result = tool.player.sandCount - cost;
            if (result <= 0) return false;

            tool.player.package.TakeItem(PlatformSystem.REFORM_ID, reformCount, out int _);
            tool.player.SetSandCount(result);

            return true;
        }
    }
}