using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace UnityEditor
{
#if UNITY_EDITOR
    public class AnimationBakerWindow : EditorWindow
    {
        private GameObject bakeObject;

        private VertaBuffer buffer;
        private bool bufferReady = false;

        private AnimationClip animationClip;
        private int frameCount = 0;
        private bool readyToBake = false;
        private bool lockSelection = false;

        private float frameRate = 30f;
        private int frameStride;
        
        private string meshDataPath = "MeshDatas";
        private string vertaPath = "Verta";

        private string modelName = "";

        [MenuItem("Window/Verta Animation Baker", false, 2000)]
        public static void DoWindow()
        {
            var window = GetWindowWithRect<AnimationBakerWindow>(new Rect(0, 0, 300, 80));
            window.Show();
        }

        public static Mesh CombineMeshes(GameObject gameObject)
        {
            //Temporarily set position to zero to make matrix math easier
            Vector3 position = gameObject.transform.position;
            gameObject.transform.position = Vector3.zero;

            //Get all mesh filters and combine
            MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
            CombineInstance[] combine = new CombineInstance[meshFilters.Length];
            for (int i = 0; i < meshFilters.Length; i++)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
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
            
            if (AnimationMode.InAnimationMode())
                AnimationMode.StopAnimationMode();


            // Slider to use when Animate has been ticked
            EditorGUILayout.BeginVertical();

            modelName = EditorGUILayout.TextField("Model Name", modelName);
            
            animationClip = EditorGUILayout.ObjectField(animationClip, typeof(AnimationClip), false) as AnimationClip;
            if (animationClip != null)
            {
                frameCount = GetFramesCount(animationClip);
                EditorGUILayout.LabelField("Frames to bake: " + frameCount);

                readyToBake = true;
            }


            if (GUILayout.Button("Bake mesh animations.") && readyToBake && !EditorApplication.isPlaying)
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

            if (animationClip == null)
                return;

            // There is a bug in AnimationMode.SampleAnimationClip which crashes
            // Unity if there is no valid controller attached
            Animator animator = bakeObject.GetComponent<Animator>();
            if (animator != null && animator.runtimeAnimatorController == null)
                return;

            Mesh firstFrame = new Mesh();
            
            EditorUtility.DisplayProgressBar("Mesh Animation Baker", "Baking", 0f);

            AnimationMode.StartAnimationMode();
            AnimationMode.BeginSampling();

            for (int frame = 0; frame < frameCount; frame++)
            {
                EditorUtility.DisplayProgressBar("Mesh Animation Baker", "Baking mesh animations", 1f * frame / frameCount);

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
                    buffer.data[globalVertStart+1] = bakedMesh.vertices[i].y;
                    buffer.data[globalVertStart+2] = bakedMesh.vertices[i].z;
                    
                    buffer.data[globalVertStart+3] = bakedMesh.normals[i].x;
                    buffer.data[globalVertStart+4] = bakedMesh.normals[i].y;
                    buffer.data[globalVertStart+5] = bakedMesh.normals[i].z;
                    
                    buffer.data[globalVertStart+6] = bakedMesh.tangents[i].x;
                    buffer.data[globalVertStart+7] = bakedMesh.tangents[i].y;
                    buffer.data[globalVertStart+8] = bakedMesh.tangents[i].z;
                }

                if (frame == 0)
                {
                    firstFrame = bakedMesh;
                }
            }
            
            string filePath = Path.Combine(Application.dataPath, vertaPath, modelName);
            filePath += ".verta";
            
            FileInfo fileInfo = new FileInfo(filePath);
            if (!Directory.Exists(fileInfo.Directory.FullName))
                Directory.CreateDirectory(fileInfo.Directory.FullName);
            
            buffer.SaveToFile(filePath);
            
            filePath = Path.Combine("Assets", meshDataPath, modelName);
            filePath += ".asset";

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