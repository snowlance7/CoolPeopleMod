using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using LethalLib.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace CoolPeopleMod
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency(LethalLib.Plugin.ModGUID)]
    public class Plugin : BaseUnityPlugin
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public static Plugin PluginInstance;
        public static ManualLogSource LoggerInstance;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        
        private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        public static PlayerControllerB localPlayer { get { return GameNetworkManager.Instance.localPlayerController; } }
        public static PlayerControllerB PlayerFromId(ulong id) { return StartOfRound.Instance.allPlayerScripts.Where(x => x.actualClientId == id).First(); }
        public static bool IsServerOrHost { get { return NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost; } }

        public static AssetBundle? ModAssets;

        public const ulong RodrigoSteamID = 76561198164429786;
        public const ulong LizzieSteamID = 76561199094139351;
        public const ulong GlitchSteamID = 76561198984467725;
        public const ulong RatSteamID = 76561199182474292;
        public const ulong XuSteamID = 76561198399127090;
        public const ulong SlayerSteamID = 76561198077184650;
        public const ulong SnowySteamID = 76561198253760639;
        public const ulong FunoSteamID = 76561198993437314;

        // Configs
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        // GlitchPlush Configs
        public static ConfigEntry<bool> configGlitchPlushEnable;
        public static ConfigEntry<string> configGlitchPlushLevelRarities;
        public static ConfigEntry<string> configGlitchPlushCustomLevelRarities;
        public static ConfigEntry<int> configGlitchPlushMinValue;
        public static ConfigEntry<int> configGlitchPlushMaxValue;

        // PinataPlush Configs
        public static ConfigEntry<bool> configPinataPlushEnable;
        public static ConfigEntry<string> configPinataPlushLevelRarities;
        public static ConfigEntry<string> configPinataPlushCustomLevelRarities;
        public static ConfigEntry<int> configPinataPlushMinValue;
        public static ConfigEntry<int> configPinataPlushMaxValue;

        // SCP999Plush Configs
        public static ConfigEntry<bool> configSCP999PlushEnable;
        public static ConfigEntry<string> configSCP999PlushLevelRarities;
        public static ConfigEntry<string> configSCP999PlushCustomLevelRarities;
        public static ConfigEntry<int> configSCP999PlushMinValue;
        public static ConfigEntry<int> configSCP999PlushMaxValue;

        // DiceMimicPlush Configs
        public static ConfigEntry<bool> configDiceMimicPlushEnable;
        public static ConfigEntry<string> configDiceMimicPlushLevelRarities;
        public static ConfigEntry<string> configDiceMimicPlushCustomLevelRarities;
        public static ConfigEntry<int> configDiceMimicPlushMinValue;
        public static ConfigEntry<int> configDiceMimicPlushMaxValue;

        // RatPlush Configs
        public static ConfigEntry<bool> configRatPlushEnable;
        public static ConfigEntry<string> configRatPlushLevelRarities;
        public static ConfigEntry<string> configRatPlushCustomLevelRarities;
        public static ConfigEntry<int> configRatPlushMinValue;
        public static ConfigEntry<int> configRatPlushMaxValue;

        // FunoPlush Configs
        public static ConfigEntry<bool> configFunoPlushEnable;
        public static ConfigEntry<string> configFunoPlushLevelRarities;
        public static ConfigEntry<string> configFunoPlushCustomLevelRarities;
        public static ConfigEntry<int> configFunoPlushMinValue;
        public static ConfigEntry<int> configFunoPlushMaxValue;

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        private void Awake()
        {
            if (PluginInstance == null)
            {
                PluginInstance = this;
            }

            LoggerInstance = PluginInstance.Logger;

            harmony.PatchAll();

            InitializeNetworkBehaviours();

            // Configs

            // GlitchPlush Configs
            configGlitchPlushEnable = Config.Bind("GlitchPlush", "Enable", true, "Set to false to prevent loading assets");
            configGlitchPlushLevelRarities = Config.Bind("GlitchPlush", "Level Rarities", "All: 10", "Rarities for each level. See default for formatting.");
            configGlitchPlushCustomLevelRarities = Config.Bind("GlitchPlush", "Custom Level Rarities", "", "Rarities for modded levels. Same formatting as level rarities.");
            configGlitchPlushMinValue = Config.Bind("GlitchPlush", "Min Value", 150, "Minimum scrap value.");
            configGlitchPlushMaxValue = Config.Bind("GlitchPlush", "Max Value", 200, "Maximum scrap value.");

            // PinataPlush Configs
            configPinataPlushEnable = Config.Bind("PinataPlush", "Enable", true, "Set to false to prevent loading assets");
            configPinataPlushLevelRarities = Config.Bind("PinataPlush", "Level Rarities", "All: 10", "Rarities for each level. See default for formatting.");
            configPinataPlushCustomLevelRarities = Config.Bind("PinataPlush", "Custom Level Rarities", "", "Rarities for modded levels. Same formatting as level rarities.");
            configPinataPlushMinValue = Config.Bind("PinataPlush", "Min Value", 150, "Minimum scrap value.");
            configPinataPlushMaxValue = Config.Bind("PinataPlush", "Max Value", 200, "Maximum scrap value.");

            // SCP999Plush Configs
            configSCP999PlushEnable = Config.Bind("SCP999Plush", "Enable", true, "Set to false to prevent loading assets");
            configSCP999PlushLevelRarities = Config.Bind("SCP999Plush", "Level Rarities", "All: 10", "Rarities for each level. See default for formatting.");
            configSCP999PlushCustomLevelRarities = Config.Bind("SCP999Plush", "Custom Level Rarities", "", "Rarities for modded levels. Same formatting as level rarities.");
            configSCP999PlushMinValue = Config.Bind("SCP999Plush", "Min Value", 150, "Minimum scrap value.");
            configSCP999PlushMaxValue = Config.Bind("SCP999Plush", "Max Value", 200, "Maximum scrap value.");

            // DiceMimicPlush Configs
            configDiceMimicPlushEnable = Config.Bind("DiceMimicPlush", "Enable", true, "Set to false to prevent loading assets");
            configDiceMimicPlushLevelRarities = Config.Bind("DiceMimicPlush", "Level Rarities", "All: 10", "Rarities for each level. See default for formatting.");
            configDiceMimicPlushCustomLevelRarities = Config.Bind("DiceMimicPlush", "Custom Level Rarities", "", "Rarities for modded levels. Same formatting as level rarities.");
            configDiceMimicPlushMinValue = Config.Bind("DiceMimicPlush", "Min Value", 150, "Minimum scrap value.");
            configDiceMimicPlushMaxValue = Config.Bind("DiceMimicPlush", "Max Value", 200, "Maximum scrap value.");

            // RatPlush Configs
            configRatPlushEnable = Config.Bind("RatPlush", "Enable", true, "Set to false to prevent loading assets");
            configRatPlushLevelRarities = Config.Bind("RatPlush", "Level Rarities", "All: 12", "Rarities for each level. See default for formatting.");
            configRatPlushCustomLevelRarities = Config.Bind("RatPlush", "Custom Level Rarities", "", "Rarities for modded levels. Same formatting as level rarities.");
            configRatPlushMinValue = Config.Bind("RatPlush", "Min Value", 150, "Minimum scrap value.");
            configRatPlushMaxValue = Config.Bind("RatPlush", "Max Value", 200, "Maximum scrap value.");

            // FunoPlush Configs
            configFunoPlushEnable = Config.Bind("FunoPlush", "Enable", true, "Set to false to prevent loading assets");
            configFunoPlushLevelRarities = Config.Bind("FunoPlush", "Level Rarities", "All: 8", "Rarities for each level. See default for formatting.");
            configFunoPlushCustomLevelRarities = Config.Bind("FunoPlush", "Custom Level Rarities", "", "Rarities for modded levels. Same formatting as level rarities.");
            configFunoPlushMinValue = Config.Bind("FunoPlush", "Min Value", 150, "Minimum scrap value.");
            configFunoPlushMaxValue = Config.Bind("FunoPlush", "Max Value", 200, "Maximum scrap value.");

            new StatusEffectController();

            // Loading Assets
            string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            ModAssets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Info.Location), "cool_assets"));
            if (ModAssets == null)
            {
                Logger.LogError($"Failed to load custom assets.");
                return;
            }
            LoggerInstance.LogDebug($"Got AssetBundle at: {Path.Combine(sAssemblyLocation, "cool_assets")}");

            RegisterScrap(configGlitchPlushEnable, "Assets/ModAssets/Glitch/GlitchPlushItem.asset", configGlitchPlushLevelRarities, configGlitchPlushCustomLevelRarities, configGlitchPlushMinValue, configGlitchPlushMaxValue);
            RegisterScrap(configPinataPlushEnable, "Assets/ModAssets/Snowy/PinataPlushItem.asset", configPinataPlushLevelRarities, configPinataPlushCustomLevelRarities, configPinataPlushMinValue, configPinataPlushMaxValue);
            RegisterScrap(configSCP999PlushEnable, "Assets/ModAssets/Lizzie/SCP999PlushItem.asset", configSCP999PlushLevelRarities, configSCP999PlushCustomLevelRarities, configSCP999PlushMinValue, configSCP999PlushMaxValue);
            RegisterScrap(configDiceMimicPlushEnable, "Assets/ModAssets/Slayer/DiceMimicItem.asset", configDiceMimicPlushLevelRarities, configDiceMimicPlushCustomLevelRarities, configDiceMimicPlushMinValue, configDiceMimicPlushMaxValue);
            RegisterScrap(configRatPlushEnable, "Assets/ModAssets/Rat/RatPlushItem.asset", configRatPlushLevelRarities, configRatPlushCustomLevelRarities, configRatPlushMinValue, configRatPlushMaxValue);
            RegisterScrap(configFunoPlushEnable, "Assets/ModAssets/Funo/FunoPlushItem.asset", configFunoPlushLevelRarities, configFunoPlushCustomLevelRarities, configFunoPlushMinValue, configFunoPlushMaxValue);


            // Finished
            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }

        public void RegisterScrap(ConfigEntry<bool> enabled, string assetPath, ConfigEntry<string> levelRarities, ConfigEntry<string> customLevelRarities, ConfigEntry<int> minValue, ConfigEntry<int> maxValue)
        {
            if (!enabled.Value) { return; }

            Item item = ModAssets!.LoadAsset<Item>(assetPath);
            if (item == null) { LoggerInstance.LogError("Error: Couldnt get assets from path: " + assetPath); return; }
            LoggerInstance.LogDebug($"Got {item.name} prefab");

            item.minValue = minValue.Value;
            item.maxValue = maxValue.Value;

            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(item.spawnPrefab);
            Utilities.FixMixerGroups(item.spawnPrefab);
            LethalLib.Modules.Items.RegisterScrap(item, GetLevelRarities(levelRarities.Value), GetCustomLevelRarities(customLevelRarities.Value));
        }

        public Dictionary<Levels.LevelTypes, int> GetLevelRarities(string levelsString)
        {
            try
            {
                Dictionary<Levels.LevelTypes, int> levelRaritiesDict = new Dictionary<Levels.LevelTypes, int>();

                if (levelsString != null && levelsString != "")
                {
                    string[] levels = levelsString.Split(',');

                    foreach (string level in levels)
                    {
                        string[] levelSplit = level.Split(':');
                        if (levelSplit.Length != 2) { continue; }
                        string levelType = levelSplit[0].Trim();
                        string levelRarity = levelSplit[1].Trim();

                        if (Enum.TryParse<Levels.LevelTypes>(levelType, out Levels.LevelTypes levelTypeEnum) && int.TryParse(levelRarity, out int levelRarityInt))
                        {
                            levelRaritiesDict.Add(levelTypeEnum, levelRarityInt);
                        }
                        else
                        {
                            LoggerInstance.LogError($"Error: Invalid level rarity: {levelType}:{levelRarity}");
                        }
                    }
                }
                return levelRaritiesDict;
            }
            catch (Exception e)
            {
                Logger.LogError($"Error: {e}");
                return null!;
            }
        }

        public Dictionary<string, int> GetCustomLevelRarities(string levelsString)
        {
            try
            {
                Dictionary<string, int> customLevelRaritiesDict = new Dictionary<string, int>();

                if (levelsString != null)
                {
                    string[] levels = levelsString.Split(',');

                    foreach (string level in levels)
                    {
                        string[] levelSplit = level.Split(':');
                        if (levelSplit.Length != 2) { continue; }
                        string levelType = levelSplit[0].Trim();
                        string levelRarity = levelSplit[1].Trim();

                        if (int.TryParse(levelRarity, out int levelRarityInt))
                        {
                            customLevelRaritiesDict.Add(levelType, levelRarityInt);
                        }
                        else
                        {
                            LoggerInstance.LogError($"Error: Invalid level rarity: {levelType}:{levelRarity}");
                        }
                    }
                }
                return customLevelRaritiesDict;
            }
            catch (Exception e)
            {
                Logger.LogError($"Error: {e}");
                return null!;
            }
        }

        public static void FreezePlayer(PlayerControllerB player, bool value)
        {
            player.disableInteract = value;
            player.disableLookInput = value;
            player.disableMoveInput = value;
        }

        private static void InitializeNetworkBehaviours()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
            LoggerInstance.LogDebug("Finished initializing network behaviours");
        }
    }
}
