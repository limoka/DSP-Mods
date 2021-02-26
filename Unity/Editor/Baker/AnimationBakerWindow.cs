using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace UnityEditor
{
#if UNITY_EDITOR
    public class AnimationBakerWindow : EditorWindow
    {
        private GameObject bakeObject;
        private MeshFilter[] filters;
        private SkinnedMeshRenderer[] renderers;

        private VertaBuffer buffer;
        private bool bufferReady = false;

        private AnimationClip animationClip;
        private int frameCount = 0;
        private bool readyToBake = false;
        private bool lockSelection = false;
        private bool bakeOnlyMesh = false;

        private float frameRate = 30f;
        private int frameStride;

        private string meshDataPath = "MeshDatas";
        private string vertaPath = "Verta";

        private string modelName = "";

        [MenuItem("Window/DSP Tools/Verta Animation Baker", false)]
        public static void DoWindow()
        {
            var window = GetWindowWithRect<AnimationBakerWindow>(new Rect(0, 0, 300, 100));
            window.Show();
        }

        //Combine all meshes together
        public Mesh CombineMeshes(GameObject gameObject)
        {
            //Temporarily set position to zero to make matrix math easier
            Vector3 position = gameObject.transform.position;
            gameObject.transform.position = Vector3.zero;

            //Get all mesh filters and combine
            CombineInstance[] combine = new CombineInstance[filters.Length + renderers.Length];
            for (int i = 0; i < filters.Length; i++)
            {
                combine[i].mesh = filters[i].sharedMesh;
                combine[i].transform = filters[i].transform.localToWorldMatrix;
                combine[i].subMeshIndex = 0;
            }

            for (int i = filters.Length; i < filters.Length + renderers.Length; i++)
            {
                combine[i].mesh = new Mesh();
                renderers[i].BakeMesh(combine[i].mesh);
                combine[i].transform = renderers[i].transform.localToWorldMatrix;
                combine[i].subMeshIndex = 0;
            }

            Mesh mesh = new Mesh();
            mesh.CombineMeshes(combine, true, true);

            //Return to original position
            gameObject.transform.position = position;

            return mesh;
        }

        private int GetFramesCount(AnimationClip clip)
        {
            return Mathf.CeilToInt(clip.length * frameRate);
        }


        // Has a GameObject been selection?
        public void OnSelectionChange()
        {
            if (!lockSelection)
            {
                bakeObject = Selection.activeGameObject;
                Repaint();
            }
        }

        // Main editor window
        public void OnGUI()
        {
            if (AnimationMode.InAnimationMode())
                AnimationMode.StopAnimationMode();

            // Wait for user to select a GameObject
            if (bakeObject == null)
            {
                EditorGUILayout.HelpBox("Please select a GameObject", MessageType.Info);
                return;
            }

            if (buffer == null)
            {
                buffer = new VertaBuffer();
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Selected object: " + bakeObject.name);

            modelName = EditorGUILayout.TextField("Model Name", modelName);

            bakeOnlyMesh = EditorGUILayout.Toggle("No animations", bakeOnlyMesh);


            if (!bakeOnlyMesh)
            {
                animationClip =
                    EditorGUILayout.ObjectField(animationClip, typeof(AnimationClip), false) as AnimationClip;
                if (animationClip != null)
                {
                    frameCount = GetFramesCount(animationClip);
                    EditorGUILayout.LabelField("Frames to bake: " + frameCount);
                }
            }else
            {
                frameCount = 1;
                animationClip = null;
            }

            readyToBake = (animationClip != null || bakeOnlyMesh) && !EditorApplication.isPlaying &&
                          !modelName.Equals("");

            if (GUILayout.Button("Bake mesh animations.") && readyToBake)
            {
                lockSelection = true;
                BakeMesh();
                lockSelection = false;
            }

            EditorGUILayout.EndVertical();
        }

        private void BakeMesh()
        {
            if (bakeObject == null)
                return;

            if (animationClip == null && !bakeOnlyMesh)
                return;

            // There is a bug in AnimationMode.SampleAnimationClip which crashes
            // Unity if there is no valid controller attached
            Animator animator = bakeObject.GetComponent<Animator>();
            if (animator != null && animator.runtimeAnimatorController == null && !bakeOnlyMesh)
                return;

            //Collect information about gameObject
            List<MeshFilter> tmpFilters = bakeObject.GetComponentsInChildren<MeshFilter>().ToList();
            List<SkinnedMeshRenderer> tmpMeshRenderers = new List<SkinnedMeshRenderer>();

            for (int i = 0; i < tmpFilters.Count; i++)
            {
                MeshFilter filter = tmpFilters[i];
                SkinnedMeshRenderer meshRenderer = filter.GetComponent<SkinnedMeshRenderer>();

                if (meshRenderer != null)
                {
                    tmpFilters.RemoveAt(i);
                    tmpMeshRenderers.Add(meshRenderer);
                    i--;
                }
            }

            filters = tmpFilters.ToArray();
            renderers = tmpMeshRenderers.ToArray();

            Mesh firstFrame = new Mesh();

            EditorUtility.DisplayProgressBar("Mesh Animation Baker", "Baking", 0f);

            //Now bake
            AnimationMode.StartAnimationMode();
            AnimationMode.BeginSampling();

            for (int frame = 0; frame < frameCount; frame++)
            {
                EditorUtility.DisplayProgressBar("Mesh Animation Baker", "Baking mesh animations",
                    1f * frame / frameCount);

                if (!bakeOnlyMesh)
                    AnimationMode.SampleAnimationClip(bakeObject, animationClip, frame / frameRate);
                Mesh bakedMesh = CombineMeshes(bakeObject);

                if (!bufferReady)
                {
                    buffer.Expand(VertType.VNT, bakedMesh.vertexCount, frameCount);
                    frameStride = buffer.vertexSize * buffer.vertexCount;
                    bufferReady = true;
                }

                for (int i = 0; i < bakedMesh.vertexCount; i++)
                {
                    int vertStart = i * buffer.vertexSize;
                    int globalVertStart = frameStride * frame + vertStart;
                    buffer.data[globalVertStart] = bakedMesh.vertices[i].x;
                    buffer.data[globalVertStart + 1] = bakedMesh.vertices[i].y;
                    buffer.data[globalVertStart + 2] = bakedMesh.vertices[i].z;

                    buffer.data[globalVertStart + 3] = bakedMesh.normals[i].x;
                    buffer.data[globalVertStart + 4] = bakedMesh.normals[i].y;
                    buffer.data[globalVertStart + 5] = bakedMesh.normals[i].z;

                    buffer.data[globalVertStart + 6] = bakedMesh.tangents[i].x;
                    buffer.data[globalVertStart + 7] = bakedMesh.tangents[i].y;
                    buffer.data[globalVertStart + 8] = bakedMesh.tangents[i].z;
                }

                if (frame == 0)
                {
                    firstFrame = bakedMesh;
                }
            }

            string filePath = Path.Combine(Application.dataPath, vertaPath);

            FileInfo fileInfo = new FileInfo(filePath);
            if (!Directory.Exists(fileInfo.Directory.FullName))
                Directory.CreateDirectory(fileInfo.Directory.FullName);

            filePath += $"/{modelName}.verta";

            if (!bakeOnlyMesh)
                buffer.SaveToFile(filePath);

            filePath = Path.Combine("Assets", meshDataPath);

            fileInfo = new FileInfo(filePath);
            if (!Directory.Exists(fileInfo.Directory.FullName))
                Directory.CreateDirectory(fileInfo.Directory.FullName);

            filePath += $"/{modelName}.asset";

            byte[] bytes = MeshDataAssetEditor.saveMeshToMeshAsset(firstFrame);

            MeshDataAsset asset = new MeshDataAsset();
            asset.bytes = bytes;

            AssetDatabase.CreateAsset(asset, filePath);

            EditorUtility.ClearProgressBar();

            AnimationMode.EndSampling();
            AnimationMode.StopAnimationMode();
        }
    }
#endif
}