# Blueprint Tweaks
![BlueprintTweaks](https://raw.githubusercontent.com/kremnev8/DSP-Mods/master/Mods/BlueprintTweaks/undo-preview.gif)
![BlueprintTweaks](https://raw.githubusercontent.com/kremnev8/DSP-Mods/master/Mods/BlueprintTweaks/recipe-preview.gif)

This mod improves Blueprint system with QoL changes, new features like force pasting, foundation blueprints and more.<br/>
List of features:<br/>
Interface changes:
- Allow using 3rd person view in Blueprint mode.
- Allow toggling between 3d person and planet view when in Blueprint mode using `J` key
- Add paste button to Blueprint Browser window. Pressing it is equivalent to double clicking currently selected blueprint.
- When pasting blueprint string into existing blueprint you can hold `Shift` key to keep description and icons
- Preserve last open Blueprint Browser directory. Also when creating new blueprints, they will be saved in the last open directory
- Commonly used keybinds are shown in the Blueprint Browser menu.

Change your blueprint on the fly:
- Selectively change recipes on machines during any moment using a new panel in blueprint inspector
- Selectively change cargo requested/provided by logistic stations in a new panel in blueprint inspector
- Allow changing building tiers. Left-click on a building in component panel to use.<br/>
- Allow changing belt hint icons. Left-click on a hint icon in component panel to use.<br/>
- Automatically generate foundations for your blueprint. There are 4 modes for this feature: None, Sparse, Cover and Fill. Their behaviour is described when hovering over mode buttons. Foundation type and color that is set in the reform menu is used when placing blueprints with generated foundations

Features:
- Allow using La**T**itude/Lon**G**itude axis lock. Use `Ctrl + T/G` to toggle
- Allow changing grid snapping. Set blueprint grid size in its settings, then press `Ctrl + B` in desired initial position.
- Allow to save used anchor position. New UI element will appear in the blueprint inspector, which will display current anchor.
- Allow blueprinting on Gas Giants.
- Allow blueprinting Foundations. Also allows copying Custom foundations color palettes. If you blueprint buildings with foundations under them, you can place blueprint where foundations are needed. Blueprint strings with this feature are `compatible` with vanilla strings.
- Allow to use blueprint like selection to Dismantle buildings. You can find its button in dismantle panel.
- Allow mirroring blueprints. Use `Shift + T/G` to toggle mirror in La**T**itude/Lon**G**itude axis.
- Allow pasting assemblers with recipes which have not been unlocked yet. Assemblers with recipes that are not unlocked will not work.
- Allow moving blueprints using drag and drop
- Allow to undo your mistakes. Use `Ctrl+Z` to undo the most recent action. Use `Shift+Z` to redo last undone action. When new action is performed redo history is cleared. When player leaves current planet undo history is cleared. Undo is compatible with Nebula, however please make backups and report any issues encountered.

Axis lock supports: Blueprint, Construction and Reform modes<br/>
Grid snapping supports: Blueprint and Construction modes<br/>

All Keybinds are rebindable<br/>
All features can be disabled in config file located at `Dyson Sphere Program/BepInEx/config/`. By default everything is enabled.

This mod is fully compatible with [Galactic Scale 2](https://dsp.thunderstore.io/package/Galactic_Scale/GalacticScale/)<br/>
This mod is fully compatible with [Nebula Multiplayer Mod](https://dsp.thunderstore.io/package/nebula/NebulaMultiplayerMod/)<br/>
**Important Note: Nebula Multiplayer mod itself is `NOT` required. I only need its API plugin, which is separate.**

More features might come in the future. If you have any feature you would like to see added, [message](#feedback-and-bug-report) me on Discord

## Mod API
Blueprint tweaks features a small API that allows other mods to save data within blueprints.

To use this API within your mod:
1. Add BlueprintTweaks assembly to your mod references
2. Declare a dependency on Blueprint tweaks
3. Implement `ICustomBlueprintDataSerializer` interface in a class

Then add following code to your mod awake call:

```cs
BlueprintTweaksPlugin.RegisterCustomBlueprintDataSerializer<MyModBlueprintSerializer>("my_amazing_mod:bp_serializer");
```

Now your `ICustomBlueprintDataSerializer` methods will be executed when appropriate.

## Feedback and Bug Report
Feel free to contact me via Discord (Kremnev8#3756) for any feedback, bug-reports or suggestions.

## Installation
### With Mod Manager

Simply open the mod manager (if you don't have it install it [here](https://dsp.thunderstore.io/package/ebkr/r2modman/)), select **Blueprint Tweaks by kremnev8**, then **Download**.

If prompted to download with dependencies, select `Yes`.

Then just click **Start modded**, and the game will run with the mod installed.

### Manually
Install BepInEx from [here](https://dsp.thunderstore.io/package/xiaoye97/BepInEx/)<br/>
Install LDBTool from [here](https://dsp.thunderstore.io/package/xiaoye97/LDBTool/)<br/>
Install CommonAPI from [here](https://dsp.thunderstore.io/package/CommonAPI/CommonAPI/)<br/>
Install NebulaMultiplayerModApi from [here](https://dsp.thunderstore.io/package/nebula/NebulaMultiplayerModApi/)<br/>

Unzip folder `patchers` into `Dyson Sphere Program/BepInEx/patchers/BlueprintTweaks/` (Create folder named `BlueprintTweaks`)<br/>
Unzip folder `plugins` into `Dyson Sphere Program/BepInEx/plugins/BlueprintTweaks/`. (Create folder named `BlueprintTweaks`)<br/>