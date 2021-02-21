# Registry Tool

Registry is a library created to be used with [BepInEx](https://github.com/BepInEx/BepInEx) and [LDBTool](https://dsp.thunderstore.io/package/xiaoye97/LDBTool/) in order to facilitate the creation and loading of new content for Dyson Sphere Program, specialising particularly on Protodata

## How to use to develop your mod

1. Download and install [BepInEx](https://github.com/BepInEx/BepInEx)
2. Download and install [LDBTool](https://dsp.thunderstore.io/package/xiaoye97/LDBTool/)
3. Create development environment. You can find how to do that [here](https://bepinex.github.io/bepinex_docs/master/articles/dev_guide/plugin_tutorial/index.html#sidetoggle)
4. Make sure to add LDBTool dll as dependency
5. Download the source code ["RegistryTool.cs"](https://github.com/kremnev8/DSP-Mods/blob/master/Mods/RegistryTool/Registry.cs) and include it apart of your project.

## Usage

Implementation of the tool is fairly simple with an example listed below. In order to utilise the registry tool, you must achieve this checklist in order (to register new protodata for items and recipes):
- Initialise the registry tool with the appropiate parameters (this can be realised by looking at the source code)
- Register all localised strings for all new items/recipes you are adding
- Register all items 
- Register all recipes

(The process is similiar for creating new technologies, models and so on forth)

If you intent on using custom assets, create and import them to a empty unity project. Make sure that path to assets **contains** your **keyword**. Then create asset bundle containing these resources. You can use [this useful tool](https://github.com/kremnev8/DSP-Mods/blob/master/Unity/Editor/ExportAssetBundles.cs) to do this. 
Additional info on creating custom buildings can be found [here](https://github.com/kremnev8/DSP-Mods/blob/master/README.md])

## Example
```csharp
//Put this code in your awake function
//Initialises the project using the registry tool. This allows custom assetbundles to be loaded
Registry.Init("examplebundle", "example", true, false);

//Creates a custom stringProto for localisation
Registry.registerString("copperWireName", "Copper Wire");
Registry.registerString("copperWireDesc", "By extruding copper we can make a component which allows current to be carried"); 
Registry.registerString("copperWireConc", "You have unlocked production of copper wire. Highly conductive materials are very useful when creating automated devices"); 
copperWireConc

//Registers a new item using set parameters and loads it into the game
ItemProto wire = Registry.registerItem(10001, "copperWireName", "copperWireDesc", "assets/example/copper_wire", 1711);
//Registers a new recipe using set parameters and loads it into the game
RecipeProto recipe = Registry.registerRecipe(10001, ERecipeType.Assemble, 60, new[] { 1104 }, new[] { 2 }, new[] { wire.ID }, new[] { 1 }, "copperWireDesc"); 

//Registers a new technology using set parameters and loads it into the game
TechProto tech = Registry.registerTech(1500, "copperWireName", "copperWireDesc", "copperWireConc", "assets/example/copper_wire", new[] {1},
                new[] {1202}, new[] {30}, 1200, new [] {recipe.ID}, new Vector2(9, -3));

```

## Documentation regarding parameters fed into registration


**Parameters for Init:**
- Name of bundle to load
- UNIQUE keyword of your mod (This has to be in every path of assets you wish to load)
- Do you need to load asset bundles
- Do you need to load verta files
```csharp
Init(string bundleName, string keyword, bool requireBundle, bool requireVerta)
```


**Parameters for registerItem:**
- UNIQUE id of your item
- LocalizedKey of name of the item
- LocalizedKey of description of the item
- Path to icon, starting from assets folder of your unity project
- Index in craft menu, format : PYXX, P - page
- Stack size of the item
```csharp
registerItem(int id, string name, string description, string iconPath, int gridIndex, int stackSize = 100)
```


**Parameters for registerRecipe:**
- UNIQUE id of your recipe
- Recipe type
- Time in ingame ticks. How long item is being made
- Array of input IDs
- Array of input COUNTS
- Array of output IDs
- Array of output COUNTS
- LocalizedKey of description of this item
- Tech id, which unlock this recipe
```csharp
registerRecipe(int id, ERecipeType type, int time, int[] input, int[] inCounts, int[] output, int[] outCounts, string description, int techID = 0)
```


**Parameters for registerTech:**
- UNIQUE ID of the technology. Note that if id > 2000 tech will be on upgrades page.
- LocalizedKey of name of the tech
- LocalizedKey of description of the tech
- LocalizedKey of conclusion of the tech upon completion
- Path to icon, starting from assets folder of your unity project
- Techs which lead to this tech
- Items required to research the tech
- Amount of items per minute required to research the tech
- Number of hashes needed required to research the tech
- Once the technology has completed, what recipes are unlocked
- Vector2 position of the technology on the technology screen
```csharp
registerTech(int id, String name, String description, String conclusion, int[] PreTechs, int[] Jellos, int[] ItemPoints, long HashNeeded, int[] UnlockRecipes, Vector2 position)
```

**Parameters for registerString:**
- UNIQUE key of your localizedKey (This is used by ItemProto/RecipeProto to load that particular string)
- English translation for this key
```csharp
registerString(string key, string enTrans)
```

**Parameters for CreateMaterial:**
- Name of shader to use
- Name of finished material, can be anything
- Tint color (In html format, #RRGGBBAA)
- Array of texture names in this order: albedo, normal, metallic, emission
- Array of keywords to use

```csharp
CreateMaterial(string shaderName, string materialName, string color, string[] textures = null, string[] keywords = null)
```

**Parameters for ModelProto:**
- UNIQUE id of your model
- ItemProto which will be turned into building
- Path to the prefab, starting from asset folder in your unity project
- List of materials to use
- int Array of used description fields
- Index in build Toolbar, FSS, F - first submenu, S - second submenu
- Grade of the building, used to add upgrading
- List of buildings ids, that are upgradable to this one. You need to include all of them here in order. ID of this building should be zero
```csharp
registerModel(int id, ItemProto proto, string prefabPath, Material[] mats, int[] descFields, int buildIndex, int grade = 0, int[] upgradesIDs = null)
```

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to test changes before opening pull request.
