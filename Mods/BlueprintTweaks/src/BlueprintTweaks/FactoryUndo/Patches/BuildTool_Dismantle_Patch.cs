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

            List<int> results = DetermineDismantle(__instance).ToList();

            if (results.Count <= 0) return;

            BlueprintData blueprint = UndoUtils.GenerateBlueprint(results, out Vector3 position);
            if (blueprint.buildings.Length > 0 && !position.Equals(Vector3.zero))
            {
                PlayerUndo data = UndoManager.GetCurrentPlayerData();
                data.AddUndoItem(new UndoDismantle(data, results, blueprint, new[] { position }, 0));
            }
        }

        private static HashSet<int> objectIds = new HashSet<int>();

        [HarmonyPatch(typeof(BuildTool_Dismantle), nameof(BuildTool_Dismantle.DismantleAction))]
        [HarmonyReversePatch]
        public static HashSet<int> DetermineDismantle(BuildTool_Dismantle tool)
        {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                CodeMatcher matcher = new CodeMatcher(instructions)

                    // Clear result hashset
                    .Start()
                    .InsertAndAdvance(Transpilers.EmitDelegate<Action>(() => { objectIds.Clear(); }));

                // Remove user queries
                matcher
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(BuildTool_Dismantle), nameof(BuildTool_Dismantle.cursorType))),
                        new CodeMatch(i => i.Branches(out Label? _)),
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(BuildTool_Dismantle), nameof(BuildTool_Dismantle.dismantleQueryObjectIds))))
                    .Repeat(codeMatcher =>
                    {
                        codeMatcher.Advance(4)
                            .InsertAndAdvance(new CodeInstruction(OpCodes.Pop))
                            .InsertAndAdvance(Transpilers.EmitDelegate<Func<HashSet<int>>>(() => objectIds))
                            .InsertAndAdvance(new CodeInstruction(OpCodes.Ret))
                            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0));
                    });

                // Replace DoDismantle calls with delegate
                matcher
                    .Start()
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(PlayerAction_Build), nameof(PlayerAction_Build.DoDismantleObject))))
                    .Repeat(codeMatcher =>
                    {
                        codeMatcher.SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<PlayerAction_Build, int, bool>>((build, objectId) =>
                        {
                            objectIds.Add(objectId);
                            return false;
                        }));
                    });

                // Do not clear buildPreviews
                matcher
                    .Start()
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Call, AccessTools.PropertyGetter(typeof(BuildTool), nameof(BuildTool.buildPreviews))),
                        new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(List<BuildPreview>), nameof(List<BuildPreview>.Clear))))
                    .Advance(1)
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Pop))
                    .RemoveInstructions(2)

                    // Push return value
                    .InsertAndAdvance(Transpilers.EmitDelegate<Func<HashSet<int>>>(() => objectIds))
                    .Advance(1);

                matcher
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Ret))
                    .Repeat(codeMatcher =>
                    {
                        codeMatcher
                            .InsertAndAdvance(Transpilers.EmitDelegate<Func<HashSet<int>>>(() => objectIds))
                            .Advance(1);
                    });

                return matcher.InstructionEnumeration();
            }

            // make compiler happy
            _ = Transpiler(null);
            return new HashSet<int>();
        }
    }
}