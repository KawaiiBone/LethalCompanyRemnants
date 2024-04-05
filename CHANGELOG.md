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

#1.2.1
- Fixed directory bug where it could not find the remnants asset bundle via the Thunderstore app.

#1.2.2
- Added a config feature to disable/enable this mod version of saving and despawning, so it can be more compatible with other mods.
- On the remnants github page, you can find the plugins folder to install the game manually. In that folder the dll file and the asset bundle can be found.
- Fixed the bug that crashes the game when unable to load the asset bundle remnants. Now it will signal that it could not load or find the asset bundle and it will cause no more crashes.
- Fixed the bug that too much things disappears on team wipe, now only the remnant items will be removed along with the normal items that would be removed.
- Updated the icon, so the feature bodies can be seen.

#1.2.3
- Added a feature that spawns varying types of bodies, based on which enemies usually spawn on that specific moon. As such, if there is a high chance for a spider to spawn, there will also be a high chance for a cocooned body to spawn. Curious what lurks in the dark this time? Your predecessors will answer that question for you.
- Added a feature that increases the body spawn rate relative to the difficulty of the moon. For example, Titan, a high risk moon, will have a higher chance for bodies to spawn compared to Vow. If you want to modify this value, you can do so in the config.
- Updated the manifest json description as follows: "Adds store items as scrap, to be found on moons as scrap in Lethal Company. You can now also find the bodies of previous crews accompanying their former gear."
- Fixed directory issues with finding the asset bundle named remnants.

#1.2.4
- Fixed a bug that would cause a crash when a custom moon was added.
- Fixed a bug that when using a lot of mods at once, something could break during registering items.

#1.2.5
- Added a feature gives bodies better spawning locations.
- Added a feature that reads costum/modded moons and makes them available to edit the remnant items rarity in the config.
- Added a feature that makes you able to edit the scrap cost of remnant items via the config.
- Update Readme for explaining the features better.

#1.2.6
- Added a feature that makes bodies grabbable and makes it so that you can sell them as scrap. You have the option to make bodies a prop instead, and you change the scrap value in the config.
  Do note that whenever you reload a lobby, the bodies created by this mod will move for a couple of seconds. This is a part of the process for preventing it from just T-posing.
- Added the "body" section in the config, and the config has been rearranged. You're advised to delete the current config and let it be remade.
- Fixed compatibility issues for registering store items as remnant items.
- Fixed bug percentage scrap cost.
- Fixed compatibility issue with custom moons which had illegal characters in its name. Now it skips the name and uses another moon's data in its place for spawning bodies.

#1.2.7
- Fixed bug where there was an audio echo if you re-entered a lobby without quitting the application.
- Fixed missing textures on webbed bodies.

#1.2.8
- Fixed a bug where despawning remnant items did not work as intended.
- Increased compatibility with LethalLib made store items.

#1.2.9
- Fixed bugs around despawn items, now it despawns remnant items accordingly making the occlude error patch obsolete.
  This also should mean that the mod has become stable.
- Updated version so it works with the latest Lethal lib version, making you able to see remnant items in custom levels in mods made with LethalLevelLoader.
  This means you can also edit the rarities seperately via the config now too.

#1.2.10
- Fixed a bug where items in your hand would disappear when lifting off from a moon.
- Fixed a bug where items in your hand would not save when lifting off from a moon or disconnecting.

#1.2.11
- Fixed a bug where items in your hand would disappear when lifting off from a moon for non hosts.
- Fixed a bug where items in your hand would not save when lifting off from a moon or disconnecting for non hosts.