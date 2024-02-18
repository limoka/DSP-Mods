### v1.6.4
- Fixed mod not working with Nebula Multiplayer Mod API version 2.0.0 or higher

### v1.6.3
- Updated to work with game version 0.10.29.21904 or higher

### v1.6.2
- Fixed that Station input and output belts broke whenever blueprint with them was mirrored

### v1.6.1
- Fixed Blueprint width and height not applying immediately to blueprint dragging logic
- Fixed Chemical Plank Mk2 mirroring offset
- Fixed issue where belt ports were not mirrored properly (Affects Logistic stations, splitters, etc.)
- Fixed issue where in certain situations blueprint foundations would not be shown
- Fixed drag remove button not showing hover and highlight states

<details>
<summary>Full changelog</summary>

### v1.6.0
- Added feature that allows foundations to be auto generated for any of your blueprints. The feature has a few modes that allow customization of the foundation patterns.
- Added ability to reassign recipes of machines that didn't have any.
- Added a hint section to blueprint browser that contains key combinations relevant to Blueprint Tweaks
- Added ability for other mods to include custom serialized data within blueprints. Use `BlueprintTweaksPlugin.RegisterCustomBlueprintDataSerializer()` API method if your mod needs this functionality.
- Fixed issue where user would not be notified that they don't have enough soil pile to place blueprint with foundations
- Fixed issue that sandbox mode "infinite soil pile" button was not respected by Blueprint Tweaks.


<details>
<summary>v1.5</summary>

### v1.5.11
- Added option to disable "undo cleared" message in the config
- Fixed blueprints not centered when mirrored and rotated at the same time
- Fixed foundation blueprints disappearing with some rotations
- Fixed foundation blueprints being offset in latitude axis in some sections of the planet. This particular bug affects previously saved blueprints, so you will need to recreate the blueprint if you encountered this bug.

### v1.5.10
- Updated to work with game version 0.10.28.20729 or higher

### v1.5.9
- Fixed duplicate belt hints from showing. Also now invalid belt hints are ignored

### v1.5.8
- Fixed compatibility issues with Genesis Book mod

### v1.5.7
- Updated to work with game version 0.9.27.14546 or higher
- Fixed NRE in OnCameraPostRender
### v1.5.5-6
- Fixed issues with undo feature when playing with Nebula Multiplayer mod.
### v1.5.4
- Potentially fixed error when pasting blueprint with foundations in some spots with Galactic Scale 2
- Fixed NRE when drag dismantling previews with default dismantle implementation
- Fixed blueprint browser belt hints UI broken. It also now supports setting hint value
### v1.5.3
- Fixed issues when blueprinting only foundations
- Added save anchors feature
- Changed extra blueprint data format, previous versions of BlueprintTweaks won't be able to load blueprints saved with 1.5.3 and higher
### v1.5.2
- Fixed issues when playing game version 0.9.25.11996 or higher
- Blueprint force paste feature now is vanilla, the only addition now is you can `Shift+Click` to immidiately force paste
### v1.5.1
- Added ability to exclude stations from undo
- Fixed that Blueprint clipboard is cleared after undo
- Undo keybinds now use on pressed detection
### v1.5.0
- Added Factory Undo feature
- Drag remove tool now uses Raptor's fast remove algorithm. If you encounter any issues it can be disabled.
- Drag remove now won't remove Logistic stations by default, to help with errors.
- Foundation blueprints now will take only items actually used. Also amount of items consumed will now be displayed.
- Fixed checkbox for enable foundation blueprints visially appearing checked, when it's not.

</details>

<details>
<summary>v1.4</summary>

### v1.4.8
- Fixed Index out of range error when dismantling prebuilds with drag tool
### v1.4.7
- Fixed NRE when some items have null Upgrade list
### v1.4.5-6
- Fixed mod archive containing old mod version
### v1.4.4
- Fixed working machies having locked recipe message despite recipe being unlocked.
### v1.4.3
- Fixed inability to disable new features
### v1.4.2
- Fixed machines with locked recipes working after loading save.
- Internal refactor of `Axis lock` and `Grid snapping` to improve compatibility with other mods
- Added preserve open path feature
- Added move blueprints using drag and drop feature
### v1.4.1
- Fixed errors when force pasting inserters with one connection missing.
- Fixed again missing connections when force pasting inserters with belts onto belts 
### v1.4.0
- Updated to work with game version 0.9.24.11182 or higher
- Added ability to keep icons and description of a blueprint when pasting string into it.
- Now assemblers with recipes that are not unlocked will keep their recipe setting, but will not work until recipe is unlocked.
- Fixed missing connections when force pasting inserters with belts onto belts 

</details>

<details>
<summary>v1.3</summary>

### v1.3.4
- Added plugin catergories on Thunderstore page.
### v1.3.3
- Fixed potential errors if keybinds are pressed while player is not on a planet
### v1.3.2
- Fixed discription being: "Example mod description"
### v1.3.1
- Added Belt hints change feature
- Added Paste button to blueprint Browser window
- Fixed unablity to open drag remove tool
- Fixed CommonAPI module not loaded errors
### v1.3.0
**Important Note: Installation HAS changed. If you are installing manually, make sure to read installation instructions again!**
- Migrated to CommonAPI
- Updated to work with game version 0.8.23.9832 or higher

</details>

<details>
<summary>v1.2</summary>

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

</details>

<details>
<summary>v1.1</summary>

### v1.1.2
- Allowed copying Custom foundation colors with blueprints
- Fixed issues when opening Blueprint windows on new planets
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

</details>

<details>
<summary>v1.0</summary>

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
- Fixed error in readme
### v1.0.0
- Initial Release

</details>
</details>