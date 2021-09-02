# Blueprint Tweaks
![BlueprintTweaks](https://raw.githubusercontent.com/kremnev8/DSP-Mods/master/Mods/BlueprintTweaks/recipe-preview.gif)
![BlueprintTweaks](https://raw.githubusercontent.com/kremnev8/DSP-Mods/master/Mods/BlueprintTweaks/force-preview.gif)

This mod adds some minor tweaks to new Blueprint system. <br/>
Current list of features:
- Allow using 3rd person view in Blueprint mode.
- Allow toggling between 3d person and planet view when in Blueprint mode using `J` key
- Selectively change recipes on machines during any moment using a new panel in blueprint inspector
- Selectively change cargo requested/provided by logistic stations in a new panel in blueprint inspector
- Allow changing building tiers. Left-click on a building in component panel to use.
- Allow forcing Blueprint paste even if some buildings collide or can't be built. Use `Shift` key to use this feature.
- Allow using La**T**itude/Lon**G**titude axis lock. Use `Ctrl + T/G` to toggle
- Allow changing grid snapping. Set blueprint grid size in its settings, then press `Ctrl + B` in desired initial position.
- Allow blueprinting on Gas Giants.
- Allow blueprinting Foundations. If you blueprint buildings with foundations under them, you can place blueprint where foundations are needed. Blueprint strings with this feature are `compatible` with vanilla strings.

Axis lock supports: Blueprint, Construction and Reform modes<br/>
Grid snapping supports: Blueprint and Construction modes<br/>

All Keybinds are rebindable<br/>
All features can be disabled in config file located at `Dyson Sphere Program/BepInEx/config/`. By default everything is enabled.

This mod is fully compatible with [Nebula Multiplayer Mod](https://dsp.thunderstore.io/package/nebula/NebulaMultiplayerMod/)

More features might come in the future. If you have any feature you would like to see added, [message](#feedback-and-bug-report) me on Discord

## Installation
### With Mod Manager

Simply open the mod manager (if you don't have it install it [here](https://dsp.thunderstore.io/package/ebkr/r2modman/)), select **Blueprint Tweaks by kremnev8**, then **Download**.

If prompted to download with dependencies, select `Yes`.

Then just click **Start modded**, and the game will run with the mod installed.

### Manually
Install BepInEx from [here](https://dsp.thunderstore.io/package/xiaoye97/BepInEx/)<br/>
Install LDBTool from [here](https://dsp.thunderstore.io/package/xiaoye97/LDBTool/)<br/>
Install NebulaMultiplayerModApi from [here](https://dsp.thunderstore.io/package/nebula/NebulaMultiplayerModApi/)<br/>

Unzip folder `patchers` into `Dyson Sphere Program/BepInEx/patchers/BlueprintTweaks/` (Create folder named `BlueprintTweaks`)<br/>
Unzip folder `plugins` into `Dyson Sphere Program/BepInEx/plugins/BlueprintTweaks/`. (Create folder named `BlueprintTweaks`)<br/>

## Feedback and Bug Report
Feel free to contact me via Discord (Kremnev8#3756) for any feedback, bug-reports or suggestions.

## Changelog
### v1.1.0
**Important Note: Installation HAS changed. If you are installing manually, make sure to read installation instructions again!**
- Added foundation blueprints feature
- Added logistic cargo change feature
- Improved compatibility with Nebula Multiplayer mod
### v1.0.8
- Fixed errors if axis lock or grid lock buttons were pressed outside blueprint mode.
- Fixed again inability to force build overlapping `Power poles`.
- Fixed again `belt` connection issues when using force paste.
### v1.0.7
- Fixed Blueprint inspector UI size
- Fixed `Icon select` dropdown being overlapped by size and recipe panels
- Fixed `belt` connection issues when using force paste
- Fixed inability to force build overlapping `Power poles`
- Added `Axis lock` and `Grid snapping` features to normal building and reform modes.
### v1.0.6
- Updated to work with game version 0.8.19.7757 or higher
### v1.0.5
- Added Scroll bar to Blueprint inspector
- Added `Axis lock` feature
- Added `Grid snapping` feature
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