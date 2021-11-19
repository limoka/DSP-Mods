using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using CommonAPI;
using HarmonyLib;
using UnityEngine;

namespace BlueprintTweaks
{
    [RegisterPatch(BlueprintTweaksPlugin.BLUEPRINT_FOUNDATIONS)]
    public static class BlueprintUtilsPatch
    {
        public delegate void AddAction(BlueprintData bluprint, BPGratBox box, int i, BPGratBox[] array, float a, float b, int c);

        public delegate void RefreshAction(BlueprintData _blueprintData, PlanetData _planet, int _dotsCursor, IntVector4[] _tropicGratBoxConditionInfo,
            float _yaw, int _segmentCnt, Vector4[] array);
        


        [HarmonyPatch(typeof(BlueprintUtils), "GenerateBlueprintData")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> AddMoreData(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher matcher = new CodeMatcher(instructions, generator)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_S),
                    new CodeMatch(OpCodes.Ldc_I4_0)
                ).Advance(3);

            Label contLabel = (Label) matcher.Operand;

            //allow generation when there is no buildings
            matcher.Advance(-3)
                .SetAndAdvance(OpCodes.Pop, null)
                .InsertAndAdvance(Transpilers.EmitDelegate<Func<bool>>(() => BlueprintCopyExtension.isEnabled && BlueprintCopyExtension.reformSelection.Count > 0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Brtrue, contLabel))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_S, 4))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_0));


            //Replace function call to include foundations
            matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(BlueprintUtils), nameof(BlueprintUtils.GetBoundingRange))))
                .SetInstruction(Transpilers.EmitDelegate<Func<PlanetData, PlanetAuxData, int[], int, float, BPGratBox>>((data, auxData, arg3, i, f) =>
                {
                    BlueprintCopyExtension.CopyReforms();
                    return ReformBPUtils.GetBoundingRange(data, auxData, arg3, i, BlueprintCopyExtension.tmpReformList, f);
                }));

            //Add initialization
            matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Newarr),
                    new CodeMatch(OpCodes.Stfld)
                ).Advance(1)
                .InsertAndAdvance(Transpilers.EmitDelegate<Action<BlueprintData>>(data =>
                {
                    if (BlueprintCopyExtension.isEnabled)
                    {
                        data.reforms = new ReformData[BlueprintCopyExtension.reformSelection.Count];

                        int i = 0;
                        foreach (var kv in BlueprintCopyExtension.reformSelection)
                        {
                            data.reforms[i] = kv.Value;
                            data.reforms[i].areaIndex = -1;
                            i++;
                        }
                    }
                    else
                    {
                        data.reforms = new ReformData[0];
                    }
                }))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0));

            //Just to anchor
            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Call,
                    AccessTools.Method(typeof(BlueprintUtils), nameof(BlueprintUtils.GetLongitudeSegmentCount), new[] {typeof(Vector3), typeof(int)})),
                new CodeMatch(OpCodes.Stloc_S),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Call,
                    AccessTools.Method(typeof(BlueprintUtils), nameof(BlueprintUtils.GetLongitudeRadPerGrid), new[] {typeof(int), typeof(int)})));

            // add my code
            matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Ldc_I4_0),
                    new CodeMatch(OpCodes.Stloc_S))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 18))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 17))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 7))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 20))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 11))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 19))
                .InsertAndAdvance(Transpilers.EmitDelegate<AddAction>(
                    (blueprint, bpgratBox, i, array, longitudeRadPerGrid, latitudeRadPerGrid, longitudeSegmentCount) =>
                    {
                        if (!BlueprintCopyExtension.isEnabled) return;
                        
                        for (int j = 0; j < BlueprintCopyExtension.reformSelection.Count; j++)
                        {
                            if (blueprint.reforms[j].areaIndex >= 0) continue;

                            ReformData data = blueprint.reforms[j];

                            if (!(bpgratBox.y - 1E-05f <= data.latitude) || !(data.latitude <= bpgratBox.w + 1E-05f)) continue;

                            blueprint.reforms[j].areaIndex = i;
                            blueprint.reforms[j].localLongitude = (data.longitude - array[i].x) / longitudeRadPerGrid;
                            blueprint.reforms[j].localLatitude = (data.latitude - array[i].y) / latitudeRadPerGrid;

                            if (blueprint.reforms[j].localLongitude < -0.5001f)
                            {
                                blueprint.reforms[j].localLongitude += longitudeSegmentCount * 5;
                            }
                        }
                    }));

            //Add null check to buildings iteration
            matcher.MatchForward(true,
                    new CodeMatch(OpCodes.Ldc_I4_0),
                    new CodeMatch(OpCodes.Newarr),
                    new CodeMatch(OpCodes.Stfld))
                .Advance(8)
                .CreateLabel(out Label exitLabel)
                .Advance(-2)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Pop))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_2))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse, exitLabel))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 21));


            return matcher.InstructionEnumeration();
        }


        [HarmonyPatch(typeof(BlueprintUtils), "RefreshBuildPreview")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> RefreshPreviews(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher matcher = new CodeMatcher(instructions, generator)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_1),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(BlueprintData), nameof(BlueprintData.buildings))),
                    new CodeMatch(OpCodes.Ldlen)
                ).Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_S, 4))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_S, 5))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_S, 6))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_S, 7))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0))
                .InsertAndAdvance(Transpilers.EmitDelegate<RefreshAction>(RefreshReformPreviews))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1));

            return matcher.InstructionEnumeration();
        }
        
        public static void RefreshReformPreviews(BlueprintData _blueprintData, PlanetData _planet, int _dotsCursor, IntVector4[] _tropicGratBoxConditionInfo,
            float _yaw, int _segmentCnt, Vector4[] array)
        {
            if (!BlueprintCopyExtension.isEnabled) return;

            int yawCount = Mathf.FloorToInt(_yaw / 90f);
            float yawX = yawCount == 1 || yawCount == 2 ? -1f : 1f;
            float yawY = yawCount == 2 || yawCount == 3 ? -1f : 1f;
            float latitudeRadPerGrid = BlueprintUtils.GetLatitudeRadPerGrid(_segmentCnt);

            List<ReformData> reforms = BlueprintPasteExtension.reformPreviews;
            int reformsLength = _blueprintData.reforms.Length;

            for (int i = 0; i < reformsLength; i++)
            {
                ReformData reformData = _blueprintData.reforms[i];
                for (int j = 0; j < _dotsCursor; j++)
                {

                    ReformData reformPreview = reforms[reformsLength * j + i];
                    Vector4 areaData = array[j + reformData.areaIndex];

                    BlueprintUtilsPatch2.MirrorArea(ref areaData, yawX, yawY);

                    float radPerGrid = BlueprintUtils.GetLongitudeRadPerGrid(areaData.y, _segmentCnt);
                    Vector2 vector4 = BlueprintUtils.TransitionWidthAndHeight(_yaw, reformData.localLongitude - 0.5f, reformData.localLatitude - 0.5f);
                    float longitudeRad = areaData.x + vector4.x * radPerGrid * yawX;
                    float finalLatitude = areaData.y + vector4.y * latitudeRadPerGrid * yawY;
                    finalLatitude = Math.Abs(finalLatitude) > 1.5707964f ? 1.5707964f * Math.Sign(finalLatitude) : finalLatitude;

                    reformPreview.latitude = finalLatitude;
                    reformPreview.longitude = longitudeRad;
                }
            }
        }
    }
}