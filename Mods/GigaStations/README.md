| [Screenshots](#giga-stations) | [ReadMe](#read-me) | [Settings](#settings) | [Installation](#installation) | [Feedback and Bug Reports](#feedback-and-bug-report) | [Changelog](#changelog) |
|---|---|---|---|---|---|

## Important
`As v2 is using its own Items now --> IF YOU WANT TO KEEP YOUR SAVE, PLEASE DELETE OLD GigaStations first with the OLD VERSION (1.0.x) ENABLED BEFORE YOU MOVE TO v2! ALSO VANILLA ONES WITH MORE THAN 5 ITEMSLOTS OCCUPIED`

# Giga-Stations
![GigaStationsV2](https://cdn.discordapp.com/attachments/800964046383349791/812636367561424926/GigaStations.png)
![GigaStationsV2](https://raw.githubusercontent.com/WaGi-Coding/gifs/main/recipes.png)
![GigaStationsV2](https://raw.githubusercontent.com/WaGi-Coding/gifs/main/GS.gif)

## Read me!
This is a updated version of the mod. If you have not loaded your save without this mod, your save should be intact. Mod originally made by [Taki7o7](https://dsp.thunderstore.io/package/Taki7o7/)

Up to 12 Item Slots per Station.
For Stations you can set the Slotcount, max Slot-Storage, max Warp-Cells, max Vessels & Drones, max Accu capacity.

For the Orbital Collector you can set the max Storage & the Collect-Speed-Multiplier.

Also you can set Vessels & Drones Capacity, so they can carry more than usual.

Vanilla Stations Carriers minimum Capacity Slider step will also get changed to 1% steps instead of 10% steps. So you can set it more precise with huge carrier capacities.

You can configure this mod to your likings. However, i highly recommend to backup your important Savegames, this mod COULD cause a lot of issues with updates.

Checkout the config of this mod! (You need to start the Game once with the mod in order to generate the config. Also, always restart the Game when you made changes to the config)

## Settings
### ILS
Setting						|Possible Values			|Default    |Description													|Vanilla-Value
-							|-							|-          |-																|-
`Max. Item Slots`			|Number (Int32)	 5 to 12	|12			|The maximum Item Slots the Station can have.					|5
`Max. Storage`				|Number (Int32)				|30000		|The maximum Storage capacity per Item-Slot.					|10000
`Max. Vessels`				|Number (Int32) 10 to 30 	|30			|The maximum Logistic Vessels amount.							|10
`Max. Drones`				|Number (Int32) 50 to 150 	|150		|The maximum Logistic Drones amount.							|50
`Max. Accu Capacity (GJ)`	|Number (Int32)				|50 GJ		|The Stations maximum Accumulator Capacity in GJ				|12 GJ
`Max. Warps`				|Number (Int32)				|150		|The maximum Warp Cells amount.									|50

### PLS
Setting						|Possible Values			|Default    |Description													|Vanilla-Value
-							|-							|-          |-																|-
`Max. Item Slots`			|Number (Int32)	 3 to 12	|12			|The maximum Item Slots the Station can have.					|3
`Max. Storage`				|Number (Int32)				|15000		|The maximum Storage capacity per Item-Slot.					|5000
`Max. Drones`				|Number (Int32) 50 to 150 	|150		|The maximum Logistic Drones amount.							|50
`Max. Accu Capacity (MJ)`	|Number (Int32)				|500 MJ		|The Stations maximum Accumulator Capacity in GJ				|180 MJ

### Collector
Setting						|Possible Values			|Default    |Description													|Vanilla-Value
-							|-							|-          |-																|-
`Collect Speed Multiplier`	|Number (Int32)			 	|3			|The maximum Logistic Drones amount.							|1
`Max. Storage`				|Number (Int32)				|15000		|The maximum Storage capacity per Item-Slot.					|5000

### Vessel
Setting						|Possible Values			|Default    |Description															|Vanilla-Value
-							|-							|-          |-																		|-
`Capacity Multiplier`		|Number (Int32)			 	|3			|Vessel Capacity Multiplier (Default Vanilla is 1000 at max. Level)		|1

### Drone
Setting						|Possible Values			|Default    |Description															|Vanilla-Value
-							|-							|-          |-																		|-
`Capacity Multiplier`		|Number (Int32)			 	|3			|Drone Capacity Multiplier (Default Vanilla is 100 at max. Level)		|1

## Installation
### With Mod Manager

Simply open the mod manager (if you don't have it install it [here](https://dsp.thunderstore.io/package/ebkr/r2modman/)), select **GigaStationsUpdated by kremnev8**, then **Download**.

If prompted to download with dependencies, select `Yes`.

Then just click **Start modded**, and the game will run with the mod installed.

### Manually
Install BepInEx from [here](https://dsp.thunderstore.io/package/xiaoye97/BepInEx/)

Then unzip all files into `Dyson Sphere Program/BepInEx/plugins/GigaStationsUpdated/`. (Create folder named `GigaStationsUpdated`)

## Feedback and Bug Report
Feel free to contact me via Discord (Kremnev8#3756) for any feedback, bug-reports or suggestions.
Origial author no longer has time to support this mod, so don't message him if this breakes.

## Changelog
### v2.1.0
- Updated to work with version `0.7.18.7275` of the game and up
- Added a scroll bar if amount of slots is higher than 5
- Added color tint to all Giga stations to differentiate them
### v2.0.7
No functionality changed with this patch
- Using CodeMatcher now for updating `needs` array size instead of using hard coded index
### v2.0.6
- Fixed gigastations cover energy exchanger in buildbar
### v2.0.5
- Fixed Collector max storage is accidentally bound to ILS max storage
- Eeducing workenergy of collector
### v2.0.4
- Fixed 1% step Capacity Sliders
- Changed initial charge power to 60MW again (it was not really intentional to set it to 200)
### v2.0.3
- Temporarely removed 1% step for carriers capacity slider
- Making Recipe and Item showing actual max charge power
- Max charge power slider/value is now 200MW by default
### v2.0.2
- Fixed Double clicking in Build-Bar highlights the wrong recipe
- Added some missing Proto IDs
### v2.0.1
- Fixed Max Warp Count did not worked when putting in per Hand
### v2.0.0
If coming from Version 1.X.X please read [THIS](#important)
- Made everything their own Items
- Carrier Capacities will now use a multiplier instead of just the value & it now gets correctly multiplied per Level
- Added Vanilla Checks to Display UI correctly for Vanilla or Giga
- Added own Item Icons
- Added own Item Recipes
- Added 1% step for min Carrier Capacity Sliders, so you can also set them precise for Vanilla Stations when you have huge Carrier Capacities
 
 ### Older versions and full changelog
 Check out original page [here](https://dsp.thunderstore.io/package/Taki7o7/GigaStations_v2/#giga-stations-v2)