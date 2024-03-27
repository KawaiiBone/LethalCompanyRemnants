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
Items that use a battery will have a random battery charge to make it seem as if they have been used.
This mod is also compatible with other mods that add more items to the game, which means that you can find modded store items on moons as well.

#### Spawn bodies near remnant items.
Now there is a chance that a body will spawn near a remnant item. There are four different types of bodies (default body, cocooned body, spring body, and head burst body). Each of these body types have unique enemies ascribed to them to determine which body is spawned in. As such, depending on the moon and the enemies that spawn there, a different body will spawn. Based on the enemy's spawn rate, their linked body types will also spawn more or less. For example: on your current moon, there is a high chance for a spider to spawn, so, when a body spawns, it is far more likely to be a cocooned body.
For now, the body is still a prop. This means you cannot grab it or interact with it.
The bodies used were part of the original version of Lethal Company, but they are loaded in via an asset bundle for easier access to the bodies.

#### Config.
Here's what you can edit and change via the config.
Remnant items:
- The minimum and maximum spawn chance (rarity) on all moons.
- The individual minimum and maximum spawn chance (rarity) for vanilla moons.
- The individual minimum and maximum spawn chance (rarity) for modded moons, does not work for now with [LethalLevelLoader](https://thunderstore.io/c/lethal-company/p/IAmBatby/LethalLevelLoader/).
- You can choose if you want a general rarity for all moons or a unique value for each moon.
- The battery charge of remnant items with a minimum and maximum charge.
- A list of banned items, preventing these items from being registered as scrap and becoming remnant items.
- Banning remnant items from spawning on certain moons.
- Makes you able to edit the scrap cost of remnant items via the config.

Body spawning:
- To make them grabbbable with a scrap value or have them just as a prop and non interactable.
- The scrap value of a body.
- The spawn chance of bodies.
- The body spawn modifier in relation to the moon risk level.

Save/Load:
- Save remnant items.
- Despawn remnant items on party wipe.


## Known Issues:

Currently, it seems that this mod becomes unstable when you rejoin a lobby. This issue can cause the following:
- Some remnant items spawn outside of the map.
- Picking up a remnant item can bug your game, making it so that you're unable to pick up this item.
- When picking up a remnant item, you can sometimes get locked into an animation loop.

To prevent these issues from happening, it is advised to restart the whole application/game rather than rejoining the lobby. (This especially applies when you are the host.)
I'm sorry for the inconvenience.



## Feedback and issues
If you find any bugs, issues, or have any feedback on how to improve the mod, you are always welcome to share it here on [Github](https://github.com/KawaiiBone/LethalCompanyRemnants/issues) or on the [Unofficial Lethal Company discord](https://discord.com/invite/nYcQFEpXfU). When you report an issue, please be sure to add the error in question and what other mods you were using at the time of the error. This way I can easily find the bug and patch it.
 
## Installing
This mod is also available on the [thunderstore website](https://thunderstore.io/c/lethal-company/p/KawaiiBone/Remnants/).

If you want to download this mod manually, you should know that it is dependent on [BepInEx](https://github.com/BepInEx) and [Lethal Lib](https://github.com/EvaisaDev/LethalLib).
When the dependencies are added, you can just copy the files in the plugins folder and add them in the BepInEx plugins folder.

