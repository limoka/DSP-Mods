# DSP_CustomBuildings
An example how to add a custom model with your mesh and animations

In the Editor folder you can find needed scripts to bake MeshDataAsset and Verta file. 
Export Asset bundles script used to force unity to build asset bundles. 
BetterMeshDataAsset makes sure that if you open MeshDataAsset in inspector nothing will break(Also is used to bake mesh data from Mesh object). 
And finally AnimationBakerWindow is the baker. You need to have a prefab like ones game loads in unity editor with all its meshRenderers and Filters in there. You will also need animation clip either from game or yours. Then in Window->Verta Animation Baker and select prefabs root, enter name and hit bake. Once it has finished you will get a verta file and MeshDataAsset. Finally this all will only compile if you have Assembly-CSharp in your project(I used ThunderKit to make it all work)

Project view:
![Project view](https://i.imgur.com/RULexSP.png)

## Installation
1. Download and unpack [BepInEx](https://github.com/BepInEx/BepInEx/releases) into game root directory
2. Download and install [LDBTool](https://dsp.thunderstore.io/package/xiaoye97/LDBTool/)
3. Download and install [DSP_CustomBuildings](https://github.com/kremnev8/DSP_CustomBuildings/releases)
