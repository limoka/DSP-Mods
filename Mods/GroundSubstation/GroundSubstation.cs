using System.Security;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using kremnev8;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace GroundSubstation
{
    [BepInPlugin("org.kremnev8.plugin.groundsubstation", "Ground Substation", "1.0.0")]
    public class GroundSubstation : BaseUnityPlugin
    {
        void Awake()
        {
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

            Registry.modelMats.Add("assets/custommachines/prefabs/ground-substation", new []{mainMat});
            
            Logger.LogInfo("Ground Substation mod is initialized!");
            
        }
    }
    
    [HarmonyPatch]
    static class VFPreloadPatch
    {
        public static ModelProto substation;
        
        [HarmonyPatch(typeof(VFPreload), "InvokeOnLoadWorkEnded")]
        [HarmonyPrefix]
        public static void Prefix1()
        {
            PrefabDesc pdesc = substation.prefabDesc;

            Material[] mats = pdesc.materials;
            for (int i = 0; i < pdesc.lodCount; i++)
            {
                for (int j = 0; j < pdesc.lodMaterials[i].Length; j++)
                {
                    pdesc.lodMaterials[i][j] = mats[j];
                }
            }
        }

        [HarmonyPatch(typeof(VFPreload), "PreloadThread")]
        [HarmonyPrefix]
        public static void Prefix2()
        {
            LDB.items.Select(2212).IconPath = "assets/custommachines/texture2d/ground-substation";
            
            substation = LDB.models.modelArray[68];
            substation.PrefabPath = "assets/custommachines/prefabs/ground-substation";
        }
    }
}