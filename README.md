# Remnants


 Why pay corporate for it if you can just find it out in the wild? 
 Let out your inner scavenger and find the tools that were left behind by former, less fortunate employees.
 
This mod spreads items from the store around on the moons for you to find along with your usual scrap. 
Battery levels may vary. For full battery levels, consult the Lethal Company store.

Adds store items as scrap, to be found on moons as scrap in Lethal Company.
Along this, there is a small chance that you will find a corpse along the scrap you find.

## Mechanics:

#### Register store items as scrap, also known as remnant items.
This mod takes the game's store items and registers them as scrap, so you can find these store items on moons alongside other scrap.
Remnant items that use a battery will have a random battery charge to make it seem as if they have been used.
This mod is also compatible with other mods that add more items to the game, which means that you can find modded store items on moons as well.
With that, remnant items can now be stored in the beltbag.

#### Spawn bodies near remnant items.
Now there is a chance that a body will spawn near a remnant item. There are four different types of bodies (default body, cocooned body, spring body, and head burst body). Each of these body types have unique enemies ascribed to them to determine which body is spawned in. As such, depending on the moon and the enemies that spawn there, a different body will spawn. Based on the enemy's spawn rate, their linked body types will also spawn more or less. For example: on your current moon, there is a high chance for a spider to spawn, so, when a body spawns, it is far more likely to be a cocooned body.
The bodies used were part of the original version of Lethal Company, but they are loaded in via an asset bundle for easier access to the bodies.

#### Config

Here's what you can edit and change via the config.

Remnant items:
- The minimum and maximum spawn chance (rarity) on all moons.
- The individual minimum and maximum spawn chance (rarity) for vanilla moons.
- The individual minimum and maximum spawn chance (rarity) for modded moons.
- You can choose if you want a general rarity for all moons or a unique value for each moon.
- The battery charge of remnant items with a minimum and maximum charge.
- A list of banned items, preventing these items from being registered as scrap and becoming remnant items.
- Choosing spawn options per item, using credit cost, choosing your own spawn rarity or disabling spawning entirely.
- Makes you able to edit the minimum and maximum scrap cost of remnant items via the config.
- A list of scrap items that you can add in acting as remnant items for body spawning and randomizing batteries.
- Disable or enable the remnant items to be stored in the beltbag item.
- Disable or enable end of round stats fix.

Spawning remnant items:
- The minimum and maximum remnant items spawned on moons.
- A modifier that increases the amount of remnant items that spawns on moons, depending on the risk level of that moon.
- The maximum duplicates of the same remnant item that can spawn on a moon.
- The minimum and maximum remnant items found on a body.

Spawning remnant items: (legacy spawning)
- Use legacy spawning, spawning like normal scrap. This will prevent the other way of spawning remnant items.
- Increasing the maximum spawning pool of scrap so there will be enough normal scrap to find.

Body spawning:
- To make them grabbbable with a scrap value or have them just as a prop and non interactable.
- The minimum and maximum scrap value of a body.
- The spawn chance of bodies.
- The body spawn modifier in relation to the moon risk level.
- Banning suits from being put on bodies.

Save/Load:
- Despawn remnant items on party wipe, do take note that this feature uses the old despawning method and may cause issues.
- A list of banned items, preventing these items from being saved on the ship.

Now also compatible with Lethal Config.

## Installing
This mod is also available on the [thunderstore website](https://thunderstore.io/c/lethal-company/p/KawaiiBone/Remnants/).

If you want to download this mod manually, you should know that it is dependent on [BepInEx](https://github.com/BepInEx) and [Lethal Lib](https://github.com/EvaisaDev/LethalLib).
When the dependencies are added, you can just copy the files in the plugins folder and add them in the BepInEx plugins folder.

## Feedback and issues
If you find any bugs, issues, or have any feedback on how to improve the mod, you are always welcome to share it here on [Github](https://github.com/KawaiiBone/LethalCompanyRemnants/issues), [Unofficial Lethal Company discord](https://discord.com/invite/nYcQFEpXfU) or on the [LC Modding discord](https://discord.com/invite/lcmod). When you report an issue, please be sure to add the error in question and what other mods you were using at the time of the error. This way I can easily find the bug and patch it.

## Known Issues

### Odd body behaviour
The scrap bodies you can find via this mod are at odd angles and might be annoying if you want to carry them. 
This is because the bodies shake too much before setting into position.
If anyone has a good way of fixing this issue, be sure to let me know.

### Possible incompatibilities 
If there are other compatibility issues with other mods, there is a big chance that it can be fixed via the config.

## Credits

I would like to thank the following people for the assistance their work and efforts have provided in creating the Remnants Mod: 
- [Lethal company modding wiki](https://lethal.wiki/) and its authors: For giving a very clear overview on how to create a mod.
- [MrMiinxx](https://www.youtube.com/@iMinx): To their easy-to-follow guides on how to start programming a mod for Lethal Company. 
- [Bepinex](https://github.com/BepInEx/) and [Harmony](https://harmony.pardeike.net/): For providing their framework and library. Without this, modding would be very difficult. 
- [Lethallib](https://github.com/EvaisaDev/LethalLib): This library made it easier to create this mod and taught me a lot about how the game and modding works.

Because of these people and their resources and contributions, I had everything I needed to start making the Remnants Mod and to further develop it.

The following mods were a great inspiration for the creation of the Remnants Mod:
- [BuyableShotgunShells](https://github.com/MegaPiggy/LethalCompanyBuyableShotgunShells): An easy way to find objects in the game.
- [Monsterdrops](https://github.com/fardin2000/MonsterDrops): Making it easier to synchronize scrap values on the network.
- [Batteries](https://github.com/eXish/lc-batteries): For finding a way to make items spawn in a more unique and separate way.

My endless gratitude to [Zaggy1024](https://github.com/Zaggy1024) for discovering the source of the biggest bug in this mod that has been haunting this mod since the beginning.

A special thanks to Sapphy for supporting and aiding me with editing and cleaning up the public texts. If you want to support her or check out her work, you can find her on [Amazon](https://www.amazon.com/stores/Sapphire-Bellatora/author/B0CNHGCP4S?ref=ap_rdr&isDramIntegrated=true&shoppingPortalEnabled=true)! 

And finally, I want to thank all of you for giving lots of feedback and support. I really appreciate it! I couldn't have made this mod into what it is now without all the bug reports, feature requests, and other helpful feedback. 
