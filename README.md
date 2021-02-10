# DSP_CustomBuildings
An example how to add a custom model with your mesh and animations

In the Editor folder you can find needed scripts to bake MeshDataAsset and Verta file. 
* ExportAssetBundles script used to force unity to build asset bundles. 
* BetterMeshDataAsset makes sure that if you open MeshDataAsset in inspector nothing will break(Also is used to bake mesh data from Mesh object). 
* AnimationBakerWindow is the baker.
* EditorObjExporter can export unity meshes to obj file(Optional)

## Steps on making this all work
1. Create unity project and either put Assembly-CSharp to reference in scripts or use [ThunderKit](https://github.com/PassivePicasso/ThunderKit) to automate that.
2. Extract or create needed assets(Models, animations, prefabs, etc)
3. Fix extracted prefab so that there is no missing scripts and all meshes and materials(Although i'm still unable to compile ingame's shaders) are there
4. If you intent to use prefab ingame you also need to properly setup all needed description scripts(Like BuildConditionConfig, SlotConfig, etc). Game uses these scripts to figure out what is what and where. If you are unsure how to set them up, try using [RuntimeUnityEditor](https://github.com/ManlyMarco/RuntimeUnityEditor) and [LDBTool](https://dsp.thunderstore.io/package/xiaoye97/LDBTool/)'s utility to explore Proto definitions.
5. Make sure that animation clip works, by dragging fixed prefab into preview window(It should animate it)
6. Open baker (Window->DSP Tools->Verta Animation Baker) and select prefabs root, enter name and hit bake. You will get MeshDataAsset and .verta file. Both are needed to make this work
7. Make sure you correctly define names and references to two created files in LODModelDesc script
8. Create new Aseet Bundle and add all needed assets to it(You cannot add .verta file, becuse it won't let you). Then build Asset Bundle (Window->DSP Tools->Build AssetBundle)
9. Write code and import everything in.

Project view:
![Project view](https://i.imgur.com/RULexSP.png)

## Installation of example mod
1. Download and unpack [BepInEx](https://github.com/BepInEx/BepInEx/releases) into game root directory
2. Download and install [LDBTool](https://dsp.thunderstore.io/package/xiaoye97/LDBTool/)
3. Download and install [DSP_CustomBuildings](https://github.com/kremnev8/DSP_CustomBuildings/releases)
