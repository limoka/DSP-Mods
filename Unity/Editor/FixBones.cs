using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityEditor
{
#if UNITY_EDITOR
    public class FixPrefabMeshes
    {
        
        private static void handleBone(GameObject gameObject)
        {
            MeshFilter filter = gameObject.AddComponent<MeshFilter>();
            if (filter == null) filter = gameObject.GetComponent<MeshFilter>();
            string[] found = AssetDatabase.FindAssets("Bone t:mesh");
            string[] found1 = AssetDatabase.FindAssets("Bone t:material");
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(AssetDatabase.GUIDToAssetPath(found[0]));
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(found1[0]));
            filter.sharedMesh = mesh;
            MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
            if (renderer == null) renderer = gameObject.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = mat;
            

        }
        
        private static void ItterateOverBones(GameObject target)
        {

            Queue<GameObject> queue = new Queue<GameObject>();
            queue.Enqueue(target);

            while (queue.Count > 0)
            {
                GameObject gameObject = queue.Dequeue();
                handleBone(gameObject);
                foreach (Transform child in gameObject.transform)
                {
                    queue.Enqueue(child.gameObject);
                }
            }
            
        }
        
        [MenuItem("Window/DSP Tools/Fix bones")]
        public static void FixPrefabs()
        {
            GameObject gameObject = Selection.activeGameObject;

            if (gameObject == null)
            {
                Debug.Log("You have nothing selected!");
                return;
            }
            
            ItterateOverBones(gameObject);
        }
    }
    #endif
}