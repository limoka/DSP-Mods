| [Screenshots](#giga-stations) | [ReadMe](#read-me) | [Settings](#settings) | [Installation](#installation) | [Feedback and Bug Reports](#feedback-and-bug-report) | [Changelog](#changelog) |
|---|---|---|---|---|---|

## Important
`As v2 is using its own Items now --> IF YOU WANT TO KEEP YOUR SAVE, PLEASE DELETE OLD GigaStations first with the OLD VERSION (1.0.x) ENABLED BEFORE YOU MOVE TO v2! ALSO VANILLA ONES WITH MORE THAN 5 ITEMSLOTS OCCUPIED`

# Giga-Stations
![GigaStations](https://raw.githubusercontent.com/kremnev8/DSP-Mods/master/Mods/GigaStations/GigaStations.png)
![GigaStations](https://raw.githubusercontent.com/WaGi-Coding/gifs/main/recipes.png)
![GigaStations](https://raw.githubusercontent.com/WaGi-Coding/gifs/main/GS.gif)

## Read me!
This is a updated version of the mod. If you have not loaded your save without this mod, your save can be safely loaded. Mod originally made by [Taki7o7](https://dsp.thunderstore.io/package/Taki7o7/)

Up to 12 Item Slots per Station.
You can change station UI layout. You can change how many slots are visible vertically and horizontally. Grid mode is also available.

For Stations you can set the Slotcount, max Slot-Storage, max Warp-Cells, max Vessels & Drones, max Accu capacity.

For the Orbital Collector you can set the max Storage & the Collect-Speed-Multiplier.

Also you can set Vessels & Drones Capacity, so they can carry more than usual.

Vanilla Stations Carriers minimum Capacity Slider step will also get changed to 1% steps instead of 10% steps. So you can set it more precise with huge carrier capacities.

You can configure this mod to your likings. However, i highly recommend to backup your important Savegames, this mod COULD cause a lot of issues with updates.

Checkout the config of this mod! (You need to start the Game once with the mod in order to generate the config. Also, always restart the Game when you made changes to the config)

## Settings
### General
Setting						|Possible Values			|Default    |Description
----                        | ----                      | ----      | ----
`Grid X Max. Count`			|Number (Int32)	 1 to 3 	|1			|Amount of slots visible horizontally		
`Grid Y Max. Count`			|Number (Int32)	 3 to 12	|5  		|Amount of slots visible vertically
`Station Color`				|Color	 					|5FCCFFFF  	|Color tint of giga stations

### ILS
Setting						|Possible Values			|Default    |Description													|Vanilla-Value
----						|----						|----       |----															|----
`Max. Item Slots`			|Number (Int32)	 5 to 12	|12			|The maximum Item Slots the Station can have.					|5
`Max. Storage`				|Number (Int32)				|30000		|The maximum Storage capacity per Item-Slot.					|10000
`Max. Vessels`				|Number (Int32) 10 to 30 	|30			|The maximum Logistic Vessels amount.							|10
`Max. Drones`				|Number (Int32) 50 to 150 	|150		|The maximum Logistic Drones amount.							|50
`Max. Accu Capacity (GJ)`	|Number (Int32)				|50 GJ		|The Stations maximum Accumulator Capacity in GJ				|12 GJ
`Max. Warps`				|Number (Int32)				|150		|The maximum Warp Cells amount.									|50

### PLS
Setting						|Possible Values			|Default    |Description													|Vanilla-Value
----						|----						|----       |----															|----
`Max. Item Slots`			|Number (Int32)	 3 to 12	|12			|The maximum Item Slots the Station can have.					|3
`Max. Storage`				|Number (Int32)				|15000		|The maximum Storage capacity per Item-Slot.					|5000
`Max. Drones`				|Number (Int32) 50 to 150 	|150		|The maximum Logistic Drones amount.							|50
`Max. Accu Capacity (MJ)`	|Number (Int32)				|500 MJ		|The Stations maximum Accumulator Capacity in GJ				|180 MJ

### Collector
Setting						|Possible Values			|Default    |Description													|Vanilla-Value
----						|----						|----       |----															|----
`Collect Speed Multiplier`	|Number (Int32)			 	|3			|The maximum Logistic Drones amount.							|1
`Max. Storage`				|Number (Int32)				|15000		|The maximum Storage capacity per Item-Slot.					|5000

### Vessel
Setting						|Possible Values			|Default    |Description															|Vanilla-Value
----						|----						|----       |----																	|----
`Capacity Multiplier`		|Number (Int32)			 	|3			|Vessel Capacity Multiplier (Default Vanilla is 1000 at max. Level)		|1

### Drone
Setting						|Possible Values			|Default    |Description															|Vanilla-Value
----						|----						|----       |----																	|----
`Capacity Multiplier`		|Number (Int32)			 	|3			|Drone Capacity Multiplier (Default Vanilla is 100 at max. Level)		|1

## Installation
### With Mod Manager

Simply open the mod manager (if you don't have it install it [here](https://dsp.thunderstore.io/package/ebkr/r2modman/)), select **GigaStationsUpdated by kremnev8**, then **Download**.

If prompted to download with dependencies, select `Yes`.

Then just click **Start modded**, and the game will run with the mod installed.

### Manually
Install BepInEx from [here](https://dsp.thunderstore.io/package/xiaoye97/BepInEx/)<br/>
Install LDBTool from [here](https://dsp.thunderstore.io/package/xiaoye97/LDBTool/)<br/>
Install CommonAPI from [here](https://dsp.thunderstore.io/package/CommonAPI/CommonAPI/)<br/>

Then unzip all files into `Dyson Sphere Program/BepInEx/plugins/GigaStationsUpdated/`. (Create folder named `GigaStationsUpdated`)

## Feedback and Bug Report
Feel free to contact me via Discord (Kremnev8#3756) for any feedback, bug-reports or suggestions.
Original author no longer has time to support this mod, so don't message him if this breaks.