# Blueprint Tweaks
![BlueprintTweaks](https://raw.githubusercontent.com/kremnev8/DSP-Mods/master/Mods/BlueprintTweaks/recipe-preview.gif)
![BlueprintTweaks](https://raw.githubusercontent.com/kremnev8/DSP-Mods/master/Mods/BlueprintTweaks/force-preview.gif)

This mod improves Blueprint system with QoL changes, new features like force pasting, foundation blueprints and more.<br/>
List of features:<br/>
Interface changes:
- Allow using 3rd person view in Blueprint mode.
- Allow toggling between 3d person and planet view when in Blueprint mode using `J` key
- Add paste button to Blueprint Browser window. Pressing it is equivalent to double clicking currently selected blueprint.

Change your blueprint on the fly:
- Selectively change recipes on machines during any moment using a new panel in blueprint inspector
- Selectively change cargo requested/provided by logistic stations in a new panel in blueprint inspector
- Allow changing building tiers. Left-click on a building in component panel to use.<br/>
- Allow changing belt hint icons. Left-click on a hint icon in component panel to use.<br/>

Big features:
- Allow forcing Blueprint paste even if some buildings collide or can't be built. Use `Shift` key to use this feature.
- Allow using La**T**itude/Lon**G**itude axis lock. Use `Ctrl + T/G` to toggle
- Allow changing grid snapping. Set blueprint grid size in its settings, then press `Ctrl + B` in desired initial position.
- Allow blueprinting on Gas Giants.
- Allow blueprinting Foundations. Also allows copying Custom foundations color palettes. If you blueprint buildings with foundations under them, you can place blueprint where foundations are needed. Blueprint strings with this feature are `compatible` with vanilla strings.
- Allow to use blueprint like selection to Dismantle buildings. You can find its button in dismantle panel.
- Allow mirroring blueprints. Use `Shift + T/G` to toggle mirror in La**T**itude/Lon**G**itude axis.

Axis lock supports: Blueprint, Construction and Reform modes<br/>
Grid snapping supports: Blueprint and Construction modes<br/>

All Keybinds are rebindable<br/>
All features can be disabled in config file located at `Dyson Sphere Program/BepInEx/config/`. By default everything is enabled.

This mod is fully compatible with [Galactic Scale 2](https://dsp.thunderstore.io/package/Galactic_Scale/GalacticScale/)<br/>
This mod is fully compatible with [Nebula Multiplayer Mod](https://dsp.thunderstore.io/package/nebula/NebulaMultiplayerMod/)<br/>
**Important Note: Nebula Multiplayer mod itself is `NOT` required. I only need its API plugin, which is separate.**

More features might come in the future. If you have any feature you would like to see added, [message](#feedback-and-bug-report) me on Discord

## How can I support this mod
If you like what I do and would like to support me you can [donate](https://paypal.me/kremnev8). <br/>
If you want other means to support me, you can [message](#feedback-and-bug-report) me on discord about it.

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

## Changelog
### v1.3.1
- Added Belt hints change feature
- Added Paste button to blueprint Browser window
- Fixed unablity to open drag remove tool
- Fixed CommonAPI module not loaded errors
### v1.3.0
**Important Note: Installation HAS changed. If you are installing manually, make sure to read installation instructions again!**
- Migrated to CommonAPI
- Updated to work with game version 0.8.23.9832 or higher
### v1.2.4
- Updated to work with game version 0.8.22.9331 or higher
### v1.2.3
- Fixed load issues if mod was installed for the first time.
### v1.2.2
**Note: If you would like to see my other mods support Chinese or other languages, you can help. If you can translate strings (You can find them on my github repo) into your language, I can add support for it.**
- Added Chinese language support
### v1.2.1
- Fixed errors when dismantling build previews using drag remove tool
### v1.2.0
- Added Blueprint mirroring
- Added drag remove Dismantle tool
- Changes behavior of Axis/Grid lock and Mirror tools so that when player exits build mode, tools state resets
- Added installation checker. If your installation is incorrect, an ingame message will pop-up explaining what could have gone wrong
- Changed config file sections. (Old settings will auto-migrate)
- Fixed numerous issues with foundation blueprints selection (Especially on poles)
- Fixed issues that some foundations that are in the blueprint did not paste. **Note that blueprints created before this version might still have these issues**
- Fixed compatability issues with `Galactic Scale 2` when using foundation blueprints
### v1.1.2
- Allow copying Custom foundation colors with blueprints
- Fix issues when opening Blueprint windows on new planets
- Minor improvements to UI look
### v1.1.1
**Important Note: Nebula Multiplayer mod itself is `NOT` required. I only need its API plugin, which is separate.**
- Fixed issues blueprinting on Gas Giants
- Fixed compatibility with `Free Foundations mod`.
### v1.1.0
**Important Note: Installation HAS changed. If you are installing manually, make sure to read installation instructions again!**
- Added foundation blueprints feature
- Added logistic cargo change feature
- Improved compatibility with `Nebula Multiplayer mod`
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