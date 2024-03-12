#1.0.0
- Release

#1.1.0
- Increased the chance of store scrap items appearing from 1%-10% to 5%-25%.
- Fixed the battery scrap bug. This bug caused only the host to receive partially charged items, and whenever the host would buy an item from the store, the item wouldn't be fully charged.
     Now only the scrap items you find around the map have a random battery charge, which is consistent for all players currently present in the game.
- Fixed a bug where if multiple mods tried to register the same item as scrap, the game returned error prompts.

#1.1.1
- Added a new feature where you can determine which scrap items spawn and which don’t by editing a Json file. Explenation is in the README.
- Fixed a bug where the mod would only register mod items as scrap, instead of both mod and vanilla items

#1.1.2
- Fixed a bug where the item values were not displayed well or not displayed at all.

#1.1.3
- Added a config to use in the config files or in your mod manager, which means the appdata json save file has become obsolete. For now it can change, general rarity, battery of remnant items and a banning list of items to not be registered as scrap. There is already a list of all the remnant items in the config loaded when you join a lobby, but it does not work yet. (It does not save correctly)
- Removed the appdata save. If you still have it you can remove it. 
- Fixed the battery bug that caused some items to not have a random charge due to network issues.

#1.1.4
- The remnant items in config work now, which means you can ban remnant items from spawning before loading the next moon.
- Fixed the battery bug that did not load the correct config data.

#1.1.5
- Fixed the compatibility problem with the mod LC Office.

#1.1.6
- Fixed the occlude error patch, now the patch triggers from when the networkmanager is shutting down rather than the player pressing the exit button which was in the ui.

#1.1.7
- Fixed the save item bug where registered items (remnants) were not present in the ship on loading the lobby again.

#1.2.0
- Added a config feature that gives option for separate rarity for each moon
- Added the bodies feature which now one of the four dead bodies can be spawned on top of a remnant item. The chance of the body spawning can be changed in the config.
- An asset bundle (remnants) that contains 4 bodies prefabs with dependencies from Lethal Company: default body, head burst body, coil head body and en-webbed body.
- Fixed the bug on party wipe, remnant items are still loaded in.
- Fixed the bug that items near the ship will be seen as items in the ship and be saved.