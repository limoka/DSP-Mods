using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace BlueprintTweaks
{
    [HarmonyPatch]
    public static class UIBlueprintComponentItemPatch
    {
        [HarmonyPatch(typeof(UIBlueprintComponentItem), "_OnRegEvent")]
        [HarmonyPostfix]
        public static void AddEvent(UIBlueprintComponentItem __instance)
        {
            __instance.button.onClick += GetAction(__instance.inspector);
        }
        
        [HarmonyPatch(typeof(UIBlueprintComponentItem), "_OnUnregEvent")]
        [HarmonyPostfix]
        public static void RemoveEvent(UIBlueprintComponentItem __instance)
        {
            __instance.button.onClick -= GetAction(__instance.inspector);
        }

        public static Action<int> GetAction(UIBlueprintInspector inspector)
        {
            void OnClick(int itemId)
            {
                UIItemPickerPatch.Popup(new Vector2(-300, 238), proto =>
                {
                    if (proto == null) return;
                    
                    SetBuildings(inspector, itemId, proto);
                }, proto => proto.Upgrades.Contains(itemId));
            }

            return OnClick;
        }

        public static void SetBuildings(UIBlueprintInspector inspector, int oldItemId, ItemProto newItem)
        {

            foreach (BlueprintBuilding building in inspector.blueprint.buildings)
            {
                if (building.itemId == oldItemId)
                {
                    building.itemId = (short)newItem.ID;
                    building.modelIndex = (short)newItem.ModelIndex;
                }
            }

            if (inspector.usage == UIBlueprintInspector.EUsage.Browser || inspector.usage == UIBlueprintInspector.EUsage.Paste)
            {
                if (inspector.usage == UIBlueprintInspector.EUsage.Paste)
                {
                    inspector.pasteBuildTool.ResetStates();
                }
            }
            else if (inspector.usage == UIBlueprintInspector.EUsage.Copy && inspector.copyBuildTool.active)
            {
            }
            inspector.Refresh(true, true, true);
        }

    }
}