using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CommonAPI;
using CommonAPI.Systems;
using HarmonyLib;
using UnityEngine;

[module: UnverifiableCode]
#pragma warning disable 618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore 618

namespace GroundSubstation
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency(CommonAPIPlugin.GUID)]
    [CommonAPISubmoduleDependency(nameof(ProtoRegistry), nameof(UtilSystem))]
    public class GroundSubstation : BaseUnityPlugin
    {
        public const string ID = "groundsubstation";
        public const string GUID = "org.kremnev8.plugin." + ID;
        public const string NAME = "Ground Substation";
        
        public const string VERSION = "1.1.0";
        
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
        public static ResourceData resource;

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
            
            string pluginfolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            resource = new ResourceData(ID, "custommachines", pluginfolder);
            resource.LoadAssetBundle("substationbundle");
            resource.ResolveVertaFolder();
            
            ProtoRegistry.AddResource(resource);


            Material mainMat = ProtoRegistry.CreateMaterial("VF Shaders/Forward/PBR Standard Substation", "ground-substation",
                "#FF7070FF",
                new[]
                {
                    "assets/custommachines/texture2d/orbital-substation-a",
                    "assets/custommachines/texture2d/orbital-substation-n",
                    "assets/custommachines/texture2d/orbital-substation-s",
                    "assets/custommachines/texture2d/orbital-substation-e"
                });

            //VF Shaders/Forward/Unlit Additive Substation
            Material effectMat = ProtoRegistry.CreateMaterial("VF Shaders/Forward/Unlit Additive Substation", "substation-effects",
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

            ProtoRegistry.RegisterString("SubstationModificationWarn", "  - [GroundSubstation] Replaced {0} buildings");
            ProtoRegistry.RegisterString("SubstationRestoreWarn", "  - [GroundSubstation] Restored {0} buildings (Save the game! You can now remove this mod safely)");

            if (mode.Value == ModeType.SIMPLE)
            {
                Logger.LogInfo("Initialising in SIMPLE mode");
                switch (type.Value)
                {
                    case SubstaionType.GROUND:
                        ProtoRegistry.AddLodMaterials("assets/custommachines/prefabs/ground-substation", 0, new []{mainMat, effectMat});
                        break;
                    case SubstaionType.SATELITE:
                        ProtoRegistry.AddLodMaterials("assets/custommachines/prefabs/orbital-substation", 0, new []{mainMat, effectMat});
                        break;
                }
                Harmony.CreateAndPatchAll(typeof(VFPreloadPatch), "groundsubstation");
                Harmony.CreateAndPatchAll(typeof(BuildTool_ClickPatch), "groundsubstation");
                
            }
            else if (mode.Value == ModeType.ADVANCED)
            {
                Logger.LogInfo("Initialising in ADVANCED mode");
                ProtoRegistry.RegisterModel(firstSubstationId, "Entities/Prefabs/orbital-substation", new[] {mainMat, effectMat});
                ProtoRegistry.RegisterModel(firstSubstationId + 1, "assets/custommachines/prefabs/ground-substation", new[] {mainMat, effectMat});
                ProtoRegistry.RegisterModel(firstSubstationId + 2, "assets/custommachines/prefabs/orbital-substation", new[] {mainMat, effectMat});
                Harmony.CreateAndPatchAll(typeof(BuildTool_ClickPatch), "groundsubstation");
                
                Harmony.CreateAndPatchAll(typeof(GameDataPatch), "groundsubstation");
                Harmony.CreateAndPatchAll(typeof(EntityDataPatch), "groundsubstation");
                Harmony.CreateAndPatchAll(typeof(PlayerControlGizmoPatch), "groundsubstation");
            }
            else
            {
                Logger.LogInfo("Initialising in REMOVE mode");
                
                Harmony.CreateAndPatchAll(typeof(GameDataPatch), "groundsubstation");
                Harmony.CreateAndPatchAll(typeof(EntityDataPatch), "groundsubstation");
            }
            
            UtilSystem.AddLoadMessageHandler(GetReplaceMessage);
            
            
            Logger.LogInfo("Ground Substation mod is initialized!");

            ProtoRegistry.onLoadingFinished += AfterLoad;
        }

        private static string GetReplaceMessage()
        {
            bool remove = mode.Value == ModeType.REMOVE;
            if (EntityDataPatch.updateCounter <= 0) return "";
            
            return string.Format((remove ? "SubstationRestoreWarn" : "SubstationModificationWarn").Translate(), EntityDataPatch.updateCounter);
        }
        

        public void AfterLoad()
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
        public static int updateCounter;

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