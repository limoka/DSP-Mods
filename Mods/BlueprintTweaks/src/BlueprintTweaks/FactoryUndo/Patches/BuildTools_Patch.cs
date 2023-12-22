using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using CommonAPI;
using HarmonyLib;
using NebulaAPI;
using UnityEngine;

namespace BlueprintTweaks.FactoryUndo
{
    [RegisterPatch(BlueprintTweaksPlugin.FACTORY_UNDO)]
    public static class BuildTools_Patch
    {
        
        [HarmonyPatch(typeof(BuildTool_Addon), nameof(BuildTool_Addon.CreatePrebuilds))]
        [HarmonyPostfix]
        public static void OnPathAddonBuilt(BuildTool_Addon __instance)
        {
            if (UndoManager.IgnoreAllEvents.Value) return;
            if (NebulaModAPI.IsMultiplayerActive)
            {
                if (NebulaModAPI.MultiplayerSession.LocalPlayer.IsClient) return;
            }

            IEnumerable<int> objectIds = __instance.buildPreviews.Select(preview => preview.objId);
            
            PlayerUndo data = UndoManager.GetCurrentPlayerData();
            data.AddUndoItem(new UndoAddon(data, objectIds));
        }
        
        [HarmonyPatch(typeof(BuildTool_Click), nameof(BuildTool_Click.CreatePrebuilds))]
        [HarmonyPostfix]
        public static void OnClickBuilt(BuildTool_Click __instance)
        {
            if (UndoManager.IgnoreAllEvents.Value) return;
            if (NebulaModAPI.IsMultiplayerActive)
            {
                if (NebulaModAPI.MultiplayerSession.LocalPlayer.IsClient) return;
            }

            IEnumerable<int> objectIds = __instance.buildPreviews.Select(preview => preview.objId);
            
            PlayerUndo data = UndoManager.GetCurrentPlayerData();
            data.AddUndoItem(new UndoBuild(data, objectIds));
        }

        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), nameof(BuildTool_BlueprintPaste.CreatePrebuilds))]
        [HarmonyPostfix]
        public static void OnPaste(BuildTool_BlueprintPaste __instance)
        {
            if (UndoManager.IgnoreAllEvents.Value) return;
            if (NebulaModAPI.IsMultiplayerActive)
            {
                if (NebulaModAPI.MultiplayerSession.LocalPlayer.IsClient) return;
            }

            List<int> objectIds = new List<int>(__instance.bpCursor);

            for (int l = 0; l < __instance.bpCursor; l++)
            {
                BuildPreview preview = __instance.bpPool[l];
                objectIds.Add(preview.objId);
            }

            Vector3[] dots = new Vector3[__instance.dotsCursor];
            Array.Copy(__instance.dotsSnapped, dots, __instance.dotsCursor);

            PlayerUndo data = UndoManager.GetCurrentPlayerData();
            
            UndoBlueprint undo = new UndoBlueprint(data, objectIds, __instance.blueprint, dots, __instance.yaw);
            data.AddUndoItem(undo);
        }

        [HarmonyPatch(typeof(BuildTool_Inserter), nameof(BuildTool_Inserter.CreatePrebuilds))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .End().MatchBack(false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Call, AccessTools.PropertyGetter(typeof(BuildTool), nameof(BuildTool.buildPreviews))),
                    new CodeMatch(OpCodes.Callvirt))
                .Advance(1)
                .InsertAndAdvance(Transpilers.EmitDelegate<Action<BuildTool_Inserter>>(tool =>
                {
                    if (UndoManager.IgnoreAllEvents.Value) return;
                    if (NebulaModAPI.IsMultiplayerActive)
                    {
                        if (NebulaModAPI.MultiplayerSession.LocalPlayer.IsClient) return;
                    }

                    IEnumerable<int> objectIds = tool.buildPreviews.Select(preview => preview.objId);
                    
                    PlayerUndo data = UndoManager.GetCurrentPlayerData();
                    data.AddUndoItem(new UndoBuild(data, objectIds));
                }))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(BuildTool_Path), nameof(BuildTool_Path.CreatePrebuilds))]
        [HarmonyPostfix]
        public static void OnClickBuildPath(BuildTool __instance)
        {
            if (UndoManager.IgnoreAllEvents.Value) return;
            if (NebulaModAPI.IsMultiplayerActive)
            {
                if (NebulaModAPI.MultiplayerSession.LocalPlayer.IsClient) return;
            }

            IEnumerable<int> objectIds = __instance.buildPreviews.Select(preview => preview.objId);
            
            PlayerUndo data = UndoManager.GetCurrentPlayerData();
            data.AddUndoItem(new UndoBuild(data, objectIds));
        }
        
    }
}