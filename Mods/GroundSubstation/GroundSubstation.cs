using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using kremnev8;

[module: UnverifiableCode]
#pragma warning disable 618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore 618

namespace GroundSubstation
{
    [BepInPlugin("org.kremnev8.plugin.groundsubstation", "Ground Substation", "1.0.4")]
    public class GroundSubstation : BaseUnityPlugin
    {
        public enum ModeType
        {
            SIMPLE,
            ADVANCED,
            REMOVE
        }

        public enum SubstaionType
        {
            GROUND,
            SATELITE
        }


        private static readonly int Multiplier = Shader.PropertyToID("_Multiplier");
        private static readonly int UVSpeed = Shader.PropertyToID("_UVSpeed");
        private static readonly int InvFade = Shader.PropertyToID("_InvFade");

        public static int firstSubstationId = 230;

        public static ConfigEntry<ModeType> mode;
        public static ConfigEntry<SubstaionType> type;

        public static ManualLogSource logger;

        void Awake()
        {
            logger = Logger;

            mode = Config.Bind("General", "Mode", ModeType.SIMPLE, 
                "Which mode to use:\n" + 
                "Simple means only one model for substation, and fully safe to use(Removing this mod will not harm saves).\n" +
                "Advanced means that all substation types will be available at once.\n" +
                "Downside is that it is not save safe(Removing this mod will cause you to lose all built substations)\n" +
                "Remove mode is intended to prepare your save to removal of this mod. In it all placed substations will revert back to original when save is LOADED");

            type = Config.Bind("General", "SubstationType", SubstaionType.GROUND, "Which model to use(Has no effect in advanced mode!)");
            

            Registry.Init("substationbundle", "custommachines", true, true);

            Material mainMat = Registry.CreateMaterial("VF Shaders/Forward/PBR Standard Substation", "ground-substation",
                "#FF7070FF",
                new[]
                {
                    "assets/custommachines/texture2d/orbital-substation-a",
                    "assets/custommachines/texture2d/orbital-substation-n",
                    "assets/custommachines/texture2d/orbital-substation-s",
                    "assets/custommachines/texture2d/orbital-substation-e"
                });

            //VF Shaders/Forward/Unlit Additive Substation
            Material effectMat = Registry.CreateMaterial("VF Shaders/Forward/Unlit Additive Substation", "substation-effects",
                "#00000000",
                new[]
                {
                    "assets/custommachines/texture2d/orbital-substation-effect",
                    "assets/custommachines/texture2d/orbital-substation-effect-mask"
                }, null,
                new[]
                {
                    Shader.PropertyToID("_MainTex"),
                    Shader.PropertyToID("_MaskTex")
                });
            effectMat.SetFloat(Multiplier, 20);
            effectMat.SetVector(UVSpeed, new Vector4(0, 1, 0, 0));
            effectMat.SetFloat(InvFade, 0.4f);

            Registry.registerString("SubstationModificationWarn", "  - [GroundSubstation] Replaced {0} buildings");
            Registry.registerString("SubstationRestoreWarn", "  - [GroundSubstation] Restored {0} buildings (Save the game! You can now remove this mod safely)");

            if (mode.Value == ModeType.SIMPLE)
            {
                Logger.LogInfo("Initialising in SIMPLE mode");
                switch (type.Value)
                {
                    case SubstaionType.GROUND:
                        Registry.modelMats.Add("assets/custommachines/prefabs/ground-substation", new []{mainMat, effectMat});
                        break;
                    case SubstaionType.SATELITE:
                        Registry.modelMats.Add("assets/custommachines/prefabs/orbital-substation", new []{mainMat, effectMat});
                        break;
                }
                Harmony.CreateAndPatchAll(typeof(VFPreloadPatch), "groundsubstation");
                Harmony.CreateAndPatchAll(typeof(BuildTool_ClickPatch), "groundsubstation");
                
            }
            else if (mode.Value == ModeType.ADVANCED)
            {
                Logger.LogInfo("Initialising in ADVANCED mode");
                Registry.registerModel(firstSubstationId, "Entities/Prefabs/orbital-substation", new[] {mainMat, effectMat});
                Registry.registerModel(firstSubstationId + 1, "assets/custommachines/prefabs/ground-substation", new[] {mainMat, effectMat});
                Registry.registerModel(firstSubstationId + 2, "assets/custommachines/prefabs/orbital-substation", new[] {mainMat, effectMat});
                Harmony.CreateAndPatchAll(typeof(BuildTool_ClickPatch), "groundsubstation");
                
                Harmony.CreateAndPatchAll(typeof(GameDataPatch), "groundsubstation");
                Harmony.CreateAndPatchAll(typeof(EntityDataPatch), "groundsubstation");
                Harmony.CreateAndPatchAll(typeof(PlayerControlGizmoPatch), "groundsubstation");
                Harmony.CreateAndPatchAll(typeof(PlanetFactoryPatch), "groundsubstation");
                Harmony.CreateAndPatchAll(typeof(GameLoaderPatch), "groundsubstation");
            }
            else
            {
                Logger.LogInfo("Initialising in REMOVE mode");
                
                Harmony.CreateAndPatchAll(typeof(GameDataPatch), "groundsubstation");
                Harmony.CreateAndPatchAll(typeof(EntityDataPatch), "groundsubstation");
                Harmony.CreateAndPatchAll(typeof(GameLoaderPatch), "groundsubstation");
            }
            
            
            Logger.LogInfo("Ground Substation mod is initialized!");

            Registry.onLoadingFinished += afterLoad;
        }

        public void afterLoad()
        {
            ItemProto item = LDB.items.Select(2212);
            item.ModelIndex = firstSubstationId;
            item.ModelCount = 3;
            ModelProto modelProto = LDB.models.modelArray[item.ModelIndex];
            if (modelProto != null)
            {
                item.prefabDesc = modelProto.prefabDesc;
            }
        }
    }

    [HarmonyPatch]
    public static class VFPreloadPatch
    {
        public static ModelProto substation;
        
        [HarmonyPatch(typeof(VFPreload), "InvokeOnLoadWorkEnded")]
        [HarmonyPrefix]
        public static void Prefix1()
        {
            PrefabDesc pdesc = substation.prefabDesc;

            Material[] mats = Registry.modelMats[substation.PrefabPath];
            for (int i = 0; i < pdesc.lodCount; i++)
            {
                for (int j = 0; j < pdesc.lodMaterials[i].Length; j++)
                {
                    if (j >= mats.Length) continue;
                    
                    pdesc.lodMaterials[i][j] = mats[j];
                }
            }
        }

        [HarmonyPatch(typeof(VFPreload), "PreloadThread")]
        [HarmonyPrefix]
        public static void Prefix2()
        {
            switch (GroundSubstation.type.Value)
            {
                case GroundSubstation.SubstaionType.GROUND:
                    
                    LDB.items.Select(2212).IconPath = "assets/custommachines/texture2d/ground-substation";
                    
                    substation = LDB.models.modelArray[68];
                    substation.PrefabPath = "assets/custommachines/prefabs/ground-substation";
                    break;
                case GroundSubstation.SubstaionType.SATELITE:
                    
                    substation = LDB.models.modelArray[68];
                    substation.PrefabPath = "assets/custommachines/prefabs/orbital-substation";
                    break;
            }
        }
    }

    [HarmonyPatch]
    public static class GameDataPatch
    {
        [HarmonyPatch(typeof(GameData), "Import")]
        [HarmonyPrefix]
        public static void Prefix()
        {
            EntityDataPatch.updateCounter = 0;
        }
    }

    [HarmonyPatch]
    public static class EntityDataPatch
    {
        public static int updateCounter = 0;

        [HarmonyPatch(typeof(EntityData), "Import")]
        [HarmonyPostfix]
        public static void Postfix(ref EntityData __instance)
        {
            if (GroundSubstation.mode.Value == GroundSubstation.ModeType.REMOVE)
            {
                if (__instance.modelIndex == GroundSubstation.firstSubstationId || 
                    __instance.modelIndex == GroundSubstation.firstSubstationId + 1 || 
                    __instance.modelIndex == GroundSubstation.firstSubstationId + 2 )
                {
                    __instance.modelIndex = 68;
                    updateCounter++;
                }
                return;
            }
            
            if (__instance.modelIndex == 68)
            {
                __instance.modelIndex = (short) GroundSubstation.firstSubstationId;
                updateCounter++;
            }
        }
    }

    [HarmonyPatch]
    public static class PlayerControlGizmoPatch
    {
        [HarmonyPatch(typeof(PlayerControlGizmo), "SetMouseOverTarget")]
        [HarmonyPrefix]
        public static bool Prefix(PlayerControlGizmo __instance, EObjectType tarType, int tarId)
        {
            if (__instance.mouseOverTargetType == tarType && __instance.mouseOverTargetId == tarId) return true;
            if (tarId == 0 || tarType != EObjectType.Entity) return true;

            PlanetFactory factory = __instance.player.factory;
            EntityData entityData = factory.entityPool[tarId];
            if (entityData.id == 0) return true;

            ItemProto itemProto = LDB.items.Select(entityData.protoId);
            if (itemProto.ModelCount > 1)
            {
                __instance.mouseOverTargetType = tarType;
                __instance.mouseOverTargetId = tarId;
                if (__instance.mouseOverTargetGizmo != null)
                {
                    __instance.mouseOverTargetGizmo.Close();
                    __instance.mouseOverTargetGizmo = null;
                }

                if (__instance.mouseOverBuildingGizmo != null)
                {
                    __instance.mouseOverBuildingGizmo.Close();
                    __instance.mouseOverBuildingGizmo = null;
                }

                if (__instance.mouseOverMinerGizmo != null)
                {
                    __instance.mouseOverMinerGizmo.Close();
                    __instance.mouseOverMinerGizmo = null;
                }

                ModelProto proto = LDB.models.modelArray[entityData.modelIndex];
                __instance.mouseOverBuildingGizmo = BoxGizmo.Create(entityData.pos, entityData.rot, proto.prefabDesc.selectCenter, proto.prefabDesc.selectSize);
                __instance.mouseOverBuildingGizmo.multiplier = 1f;
                __instance.mouseOverBuildingGizmo.alphaMultiplier = proto.prefabDesc.selectAlpha;
                __instance.mouseOverBuildingGizmo.fadeInScale = 1.3f;
                __instance.mouseOverBuildingGizmo.fadeInTime = 0.05f;
                __instance.mouseOverBuildingGizmo.fadeInFalloff = 0.5f;
                __instance.mouseOverBuildingGizmo.fadeOutScale = 1.3f;
                __instance.mouseOverBuildingGizmo.fadeOutTime = 0.05f;
                __instance.mouseOverBuildingGizmo.fadeOutFalloff = 0.5f;
                __instance.mouseOverBuildingGizmo.color = Color.white;
                __instance.mouseOverBuildingGizmo.Open();
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch]
    static class PlanetFactoryPatch
    {
        [HarmonyPatch(typeof(PlanetFactory), "CreateEntityDisplayComponents")]
        [HarmonyPrefix]
        public static void Prefix(PlanetFactory __instance, int entityId, ref PrefabDesc desc, short modelIndex)
        {
            if (modelIndex == 0) return;
            if (modelIndex >= LDB.models.modelArray.Length) return;

            ModelProto proto = LDB.models.modelArray[modelIndex];
            if (proto != null)
            {
                desc = proto.prefabDesc;
            }
        }
    }

    [HarmonyPatch]
    static class GameLoaderPatch
    {
        public delegate void RefAction<T1>(ref T1 arg1);

        [HarmonyPatch(typeof(GameLoader), "FixedUpdate")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> AddModificationWarn(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldloc_0),
                    new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(string), nameof(string.IsNullOrEmpty))),
                    new CodeMatch(OpCodes.Brtrue)
                )
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, 0))
                .InsertAndAdvance(
                    Transpilers.EmitDelegate<RefAction<string>>((ref string text) =>
                    {
                        bool remove = GroundSubstation.mode.Value == GroundSubstation.ModeType.REMOVE;
                        if (EntityDataPatch.updateCounter > 0)
                            text = text + "\r\n" + string.Format((remove ? "SubstationRestoreWarn" : "SubstationModificationWarn").Translate(), EntityDataPatch.updateCounter);
                    }));


            return matcher.InstructionEnumeration();
        }
    }

    [HarmonyPatch]
    static class BuildTool_ClickPatch
    {
        [HarmonyPatch(typeof(BuildTool_Click), "CheckBuildConditions")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> CheckConditions(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(i =>
                        i.opcode == OpCodes.Ldfld && ((FieldInfo) i.operand).Name == "desc"),
                    new CodeMatch(i =>
                        i.opcode == OpCodes.Ldfld && ((FieldInfo) i.operand).Name == "hasBuildCollider"),
                    new CodeMatch(OpCodes.Brfalse),
                    new CodeMatch(OpCodes.Ldloc_S)
                ).Advance(1)
                .SetAndAdvance(OpCodes.Ldarg_0, null)
                .SetInstructionAndAdvance(
                    Transpilers.EmitDelegate<Func<BuildPreview, BuildTool_Click, bool>>(
                        (preview, _this) =>
                        {
                            if (preview.item.ID == 2212)
                            {
                                if (_this.handPrefabDesc != null && _this.handPrefabDesc.modelIndex == GroundSubstation.firstSubstationId + 2)
                                    return false;
                            }

                            return preview.desc.hasBuildCollider;
                        }));


            return matcher.InstructionEnumeration();
        }
    }
}