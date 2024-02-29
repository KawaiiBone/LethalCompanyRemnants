# Remnants


 Why pay corporate for it if you can just find it out in the wild? 
 Let out your inner scavenger and find the tools that were left behind by former, less fortunate employees.
 
This mod spreads items from the store around on the moons for you to find along with your usual scrap. 
Battery levels may vary. For full battery levels, consult the Lethal Company store.

Adds store items as scrap, to be found on moons as scrap in Lethal Company.


## Feedback and issues
If you find any bugs, issues, or have any feedback on how to improve the mod, you are always welcome to share it here on [Github](https://github.com/KawaiiBone/LethalCompanyRemnants/issues) or on the [Unofficial Lethal Company discord](https://discord.com/invite/nYcQFEpXfU). When you report an issue, please be sure to add the error in question and what other mods you were using at the time of the error. This way I can easily find the bug and patch it.

## Future Work: 
I intend to focus on the config/UI, so that you can edit the settings for the scrap items.
The plan was to add bodies of past employees to the scrap list, but due to a whole load of technical issues, I've had to temporarily abandon that idea. 
You can expect regular updates for bug fixes as well.

## How to ban certain store scrap items:
To ban an item, you first need to start up the game and join a lobby (Both your own and other's lobbies work!). This way, a Json file is created.
 Then you edit said Json file, which should be named "RemnantsSpawnableList" and can be found in your Lethal Company appdata folder. In the Json file, you should be able to find the names of the scrap items, with a Boolean that mentions whether the item in question is banned or not. Change this Boolean to ban/unban whatever scrap you wish.
When you edit the file, be careful that it is not at the same time when you join a lobby.
Please do note that only the host’s Json file influences the scrap found on the different Moons.

The location of the file is located where all the savefiles are from Lethal Company. Location:C:\Users(username)\AppData\LocalLow\ZeekerssRBLX\Lethal Company

## Known issues:
- The battery patch has been temporarily disabled because there was an issue with getting it to apply to all items with a battery.

## Installing
This mod is also available on the [thunderstore website](https://thunderstore.io/c/lethal-company/p/KawaiiBone/Remnants/).

If you want to download this mod manually, you should know that it is dependent on [BepInEx](https://github.com/BepInEx) and [Lethal Lib](https://github.com/EvaisaDev/LethalLib).
