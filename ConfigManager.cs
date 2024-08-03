using BepInEx.Configuration;

namespace CycleRandomizer
{
    internal class ConfigManager
    {
        public static ConfigEntry<bool> isCycleDungeon;
        public static ConfigEntry<bool> isMoonScreenHided;
        public static ConfigEntry<string> moonDefaultExclusions;
        public static ConfigEntry<string> dungeonDefaultExclusions;

        internal static void Load()
        {
            isCycleDungeon = CycleRandomizer.configFile.Bind<bool>("_Global_", "Cycle Dungeon", true, "Activate dungeon cycling.");
            isMoonScreenHided = CycleRandomizer.configFile.Bind<bool>("_Global_", "Hide Infos", true, "Hide information on moon screen.");
            moonDefaultExclusions = CycleRandomizer.configFile.Bind<string>("_Global_", "Default moon exclusion list", null, "Moons assigned to the list by default at the start of the game.");
            dungeonDefaultExclusions = CycleRandomizer.configFile.Bind<string>("_Global_", "Default dungeon exclusion list", null, "Dungeons assigned to the list by default at the start of the game.");
        }
    }
}
