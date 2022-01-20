# LDBTool

Library that allows mods to add and edit Proto data. Also allows you to see Proto data, config ID's of mod Protos and change localized strings

# List of features
- Add new Protos to game ProtoSets
- Edit existing Protos
- Configure ID, Grid index of created Protos in config file located at `Dyson Sphere Program/BepInEx/config/LDBTool`
- Customize mod localization
- View all Protos and inspect them using [UnityExplorer](https://dsp.thunderstore.io/package/sinai-dev/UnityExplorer/)

## Installation
### With Mod Manager

Simply open the mod manager (if you don't have it install it [here](https://dsp.thunderstore.io/package/ebkr/r2modman/)), select **LDB Tool by xiaoye97**, then **Download**.

If prompted to download with dependencies, select `Yes`.

Then just click **Start modded**, and the game will run with the mod installed.

### Manually
Install BepInEx from [here](https://dsp.thunderstore.io/package/xiaoye97/BepInEx/)<br/>
Unzip mod arhive into `Dyson Sphere Program/BepInEx/plugins/LDBTool/`. (Create folder named `LDBTool`)<br/>

## Feedback and Bug Report
Feel free to contact me via Discord (Kremnev8#3756) for any feedback, bug-reports or suggestions.

## Changelog
### v2.0.1
- Fix README
- All Protos can now be seen in Proto view menu

### v2.0.0
- Types of protos that can be added is now computed at runtime
- Strings are bound in config file by their string key
- Strings ID's now are autoassigned and not bound to config file
- Now mods can override empty strings binding
- Added UnityExplorer support to Proto UI

### v1.8.0
- Added the function of custom translation, players can customize the translated text added by the Mod in the configuration file.

### v1.7.0
- Added the ability to customize the construction shortcut bar

### v1.6.0
- Optimized GUI, use RuntimeUnityEditor's skin when RuntimeUnityEditor is installed
- Added Proto search function, you can search for ID, Name, and translation
- Added a custom GridIndex configuration file, players can define the location of Mod items by themselves.

### v1.5.0
- Added the function of easily querying Proto data in the data display mode (point the mouse at the item, press I to view ItemProto, and press R to view RECEIVEPROTO)
- In the data display mode, the Tip of the item will display the ID later

### v1.4.0
- A profile with a custom ID has been added, and players can define the ID of the Mod item by themselves.

### v1.3.0
- Fixed item sorting issue
- Add object copy method

### v1.2.0
- Split the added data into pre-added and post-added in order to add translation Proto

### v1.1.0
- Support for modifying Proto data
- Add Proto data to view GUI