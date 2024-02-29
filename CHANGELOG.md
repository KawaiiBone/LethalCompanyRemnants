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
- Added a config to use in the config files or in your mod manager. For now it can change, general rarity, battery of remnant items and a banning list of items to not be registered as scrap. There is already a list of all the remnant items in the config loaded when you join a lobby, but it does not work yet. (It does not save correctly)
- Removed the appdata save. If you still have it you can remove it. 
- Fixed the battery bug that caused some items to not have a random charge due to network issues.
