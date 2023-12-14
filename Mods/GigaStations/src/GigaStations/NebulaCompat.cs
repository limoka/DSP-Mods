using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using NebulaAPI;
using UnityEngine;

namespace GigaStations
{
    [BepInPlugin(MOD_GUID, MOD_NAME, GigaStationsPlugin.VERSION)]
    [BepInDependency(NebulaModAPI.API_GUID)]
    public class NebulaCompat : BaseUnityPlugin
    {
        public const string MOD_GUID = "org.kremnev8.plugin.GigaStationsNebulaCompat";
        public const string MOD_NAME = "Giga Stations Nebula Compatibility";

        private void Awake()
        {
            if (NebulaModAPI.NebulaIsInstalled)
            {
                Logger.LogInfo("Patching NebulaWorld!");
                Harmony harmony = new Harmony(MOD_GUID);
                MethodInfo method = AccessTools.TypeByName("NebulaWorld.Logistics.StationUIManager").GetMethod("UpdateSettingsUI", AccessTools.all);
                if (method == null)
                {
                    Logger.LogInfo("Error: Not found NebulaWorld.Logistics.StationUIManager. There might be issues!");
                    return;
                }
                MethodInfo transpiler = typeof(NebulaCompatPatches).GetMethod(nameof(NebulaCompatPatches.Replace10With1Transpiler));
                harmony.Patch(method, null, null, new HarmonyMethod(transpiler));
            }
            Logger.LogInfo("Success! Nebula Compat is ready!");
        }
    }

    public static class NebulaCompatPatches
    {
        public static IEnumerable<CodeInstruction> Replace10With1Transpiler(IEnumerable<CodeInstruction> instructions)
        {

            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(StationComponent), nameof(StationComponent.deliveryDrones))))
                .MatchBack(false, new CodeMatch(instr => instr.opcode == OpCodes.Ldc_R4 && Mathf.Approximately((float)instr.operand, 10f)))
                .SetOperandAndAdvance(1f);
            
            matcher
                .MatchForward(false, new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(StationComponent), nameof(StationComponent.deliveryShips))))
                .MatchBack(false, new CodeMatch(instr => instr.opcode == OpCodes.Ldc_R4 && Mathf.Approximately((float)instr.operand, 10f)))
                .SetOperandAndAdvance(1f);

            return matcher.InstructionEnumeration();
        }
    }
}