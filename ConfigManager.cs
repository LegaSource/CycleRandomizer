using BepInEx.Configuration;

namespace CycleRandomizer
{
    internal class ConfigManager
    {
        public static ConfigEntry<bool> isCycleDungeon;
        public static ConfigEntry<bool> isMoonScreenHided;

        internal static void Load()
        {
            isCycleDungeon = CycleRandomizer.configFile.Bind<bool>("_Global_", "Cycle Dungeon", true, "Activate dungeon cycling.");
            isMoonScreenHided = CycleRandomizer.configFile.Bind<bool>("_Global_", "Hide Infos", true, "Hide information on moon screen.");
        }
    }
}
