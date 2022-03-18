using System.Collections.Generic;
using System.Linq;
using CommonAPI;
using HarmonyLib;
using NebulaAPI;
using UnityEngine;

namespace BlueprintTweaks.FactoryUndo
{
    [RegisterPatch(BlueprintTweaksPlugin.FACTORY_UNDO)]
    public static class BuildTool_Dismantle_Patch
    {
        [HarmonyPatch(typeof(BuildTool_Dismantle), nameof(BuildTool_Dismantle.DismantleAction))]
        [HarmonyPrefix]
        public static void OnDismantle(BuildTool_Dismantle __instance)
        {
            if (UndoManager.IgnoreAllEvents.Value) return;
            if (NebulaModAPI.IsMultiplayerActive)
            {
                if (NebulaModAPI.MultiplayerSession.LocalPlayer.IsClient) return;
            }
            
            List<int> objectIds = __instance.DetermineDismantle().ToList();

            if (objectIds.Count <= 0) return;

            BlueprintData blueprint = UndoUtils.GenerateBlueprint(objectIds, out Vector3 position);
            if (blueprint.buildings.Length > 0 && !position.Equals(Vector3.zero))
            {
                PlayerUndo data = UndoManager.GetCurrentPlayerData();
                data.AddUndoItem(new UndoDismantle(data, objectIds, blueprint, new[] {position}, 0));
            }
        }
    }
}