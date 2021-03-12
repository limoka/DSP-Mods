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
        private MeshRenderer[] basicRenderers;
        private SkinnedMeshRenderer[] renderers;

        private Material[] materials;

        private VertaBuffer buffer;
        private bool bufferReady;

        private AnimationClip animationClip;
        private int frameCount;
        private bool readyToBake;
        private bool lockSelection;
        private bool bakeOnlyMesh;
        private bool debugMeshOutput;

        private float frameRate = 30f;
        private int frameStride;

        private string meshDataPath = "MeshDatas";
        private string vertaPath = "Verta";

        private string modelName = "";

        [MenuItem("Window/DSP Tools/Verta Animation Baker", false)]
        public static void DoWindow()
        {
            var window = GetWindowWithRect<AnimationBakerWindow>(new Rect(0, 0, 300, 130));
            window.SetBakeObject(Selection.activeGameObject);
            window.Show();
        }

        public AnimationBakerWindow()
        {
            titleContent.text = "Animation Baker";
        }

        //Combine all meshes together
        public Mesh CombineSimpleMeshes(GameObject gameObject)
        {
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

            return mesh;
        }
        
        public Mesh CombineMeshes(GameObject gameObject)
        {
            //Get all mesh filters and combine
            //CombineInstance[] combine = new CombineInstance[filters.Length];

            List<CombineInstance> combine = new List<CombineInstance>();
            List<Mesh> subMeshes = new List<Mesh>();
            Mesh mesh;

            foreach (Material mat in materials)
            {
                for (int i = 0; i < filters.Length; i++)
                {
                    int submesh = -1;
                    for (int j = 0; j < basicRenderers[i].sharedMaterials.Length; j++)
                    {
                        if (basicRenderers[i].sharedMaterials[j] != mat) continue;
                        
                        submesh = j;
                        break;
                    }

                    if (submesh == -1) continue;
                    
                    CombineInstance inst = new CombineInstance
                    {
                        mesh = filters[i].sharedMesh,
                        transform = filters[i].transform.localToWorldMatrix,
                        subMeshIndex = submesh
                    };
                    combine.Add(inst);
                }
                
                foreach (SkinnedMeshRenderer renderer in renderers)
                {
                    int submesh = -1;
                    for (int j = 0; j < renderer.sharedMaterials.Length; j++)
                    {
                        if (renderer.sharedMaterials[j] != mat) continue;
                        
                        submesh = j;
                        break;
                    }

                    if (submesh == -1) continue;

                    Mesh bakedMesh = new Mesh();
                    renderer.BakeMesh(bakedMesh);
                    
                    CombineInstance inst = new CombineInstance
                    {
                        mesh = bakedMesh,
                        transform = renderer.transform.localToWorldMatrix,
                        subMeshIndex = submesh
                    };
                    combine.Add(inst);
                }
                
                mesh = new Mesh();
                mesh.CombineMeshes(combine.ToArray(), true, true);
                subMeshes.Add(mesh);
                combine.Clear();
            }

            if (subMeshes.Count == 1)
                return subMeshes[0];

            foreach (Mesh submesh in subMeshes)
            {
                CombineInstance inst = new CombineInstance
                {
                    mesh = submesh
                };
                combine.Add(inst);

            }

            mesh = new Mesh();
            mesh.CombineMeshes(combine.ToArray(), false, false);

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
                SetBakeObject(Selection.activeGameObject);
            }
        }

        public void SetBakeObject(GameObject newTarget)
        {
            if (modelName.Equals("") || bakeObject != null && modelName.Equals(bakeObject.name))
            {
                modelName = newTarget.name;
            }

            Animation animation = newTarget.GetComponent<Animation>();
            if (animation != null && animation.clip != null)
            {
                animationClip = animation.clip;
            }
            

            bakeObject = newTarget;
            Repaint();
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
            
            debugMeshOutput = EditorGUILayout.Toggle("Output debug mesh", debugMeshOutput);


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
            basicRenderers = filters.Select(filter => filter.GetComponent<MeshRenderer>()).ToArray();
            List<Material> _materials = new List<Material>();
            foreach (MeshRenderer renderer in basicRenderers)
            {
                foreach (Material mat in renderer.sharedMaterials)
                {
                    if (!_materials.Contains(mat))
                    {
                        _materials.Add(mat);
                    }
                }
            }

            materials = _materials.ToArray();
            renderers = tmpMeshRenderers.ToArray();
            
            //Temporarily set position to zero to make matrix math easier
            Vector3 position = bakeObject.transform.position;
            bakeObject.transform.position = Vector3.zero;

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
            
            //Return to original position
            bakeObject.transform.position = position;

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

            if (debugMeshOutput)
                AssetDatabase.CreateAsset(firstFrame, filePath + $"/{modelName}-mesh.asset");

            byte[] bytes = MeshDataAssetEditor.saveMeshToMeshAsset(firstFrame);

            MeshDataAsset asset = new MeshDataAsset();
            asset.bytes = bytes;
            asset.materials = materials.ToArray();

            AssetDatabase.CreateAsset(asset, filePath + $"/{modelName}.asset");

            EditorUtility.ClearProgressBar();

            AnimationMode.EndSampling();
            AnimationMode.StopAnimationMode();
        }
    }
#endif
}