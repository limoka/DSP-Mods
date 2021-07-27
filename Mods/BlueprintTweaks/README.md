# Blueprint Tweaks
![BlueprintTweaks](https://raw.githubusercontent.com/kremnev8/DSP-Mods/master/Mods/BlueprintTweaks/recipe-preview.gif)
![BlueprintTweaks](https://raw.githubusercontent.com/kremnev8/DSP-Mods/master/Mods/BlueprintTweaks/force-preview.gif)

This mod adds some minor tweaks to new Blueprint system. 
Current list of features:
- Allow using 3rd person view in Blueprint mode.
- Allow toggling between 3d person and planet view when in Blueprint mode using `J` key
- Selectively change recipes on machines during any moment using a new panel in blueprint inspector
- Allow changing building tiers. Left-click on a building in component panel to use.
- Allow forcing Blueprint paste even if some buildings collide or can't be built. Use `Shift` key to use this feature.
- Allow using La`T`itude/Lon`G`titude axis lock. Use `Ctrl + T/G` to toggle
- Allow changing grid shapping. Set blueprint grid size in its settings, then press `Ctrl + B` in desired initial position.
- Allow blueprinting on Gas Giants.

All Keybinds are rebindable
All features can be disabled in config file located at `Dyson Sphere Program/BepInEx/config/`. By default everything is enabled.

More features might come in the future. If you have any feature you would like to see added, [message](#feedback-and-bug-report) me on Discord

## Installation
### With Mod Manager

Simply open the mod manager (if you don't have it install it [here](https://dsp.thunderstore.io/package/ebkr/r2modman/)), select **Blueprint Tweaks by kremnev8**, then **Download**.

If prompted to download with dependencies, select `Yes`.

Then just click **Start modded**, and the game will run with the mod installed.

### Manually
Install BepInEx from [here](https://dsp.thunderstore.io/package/xiaoye97/BepInEx/)
Install LDBTool from [here](https://dsp.thunderstore.io/package/xiaoye97/LDBTool/)

Then unzip all files into `Dyson Sphere Program/BepInEx/plugins/BlueprintTweaks/`. (Create folder named `BlueprintTweaks`)

## Feedback and Bug Report
Feel free to contact me via Discord (Kremnev8#3756) for any feedback, bug-reports or suggestions.

## Changelog
### v1.0.5
- Added Scroll bar to Blueprint inspector
- Added Axis lock feature
- Added Grid snapping feature
- Added building tier change feature
- Added ability to Blueprint on Gas Giants
- Added ability to try again after Blueprint placement failed
- Fixed some minor issues
### v1.0.4
- Fixed minor conflict with Nebula
### v1.0.3
- Added force paste feature
- Added ability to disable features in config file.
### v1.0.2
- Added recipe change feature
### v1.0.1
- Fix error in readme
### v1.0.0
- Initial Release