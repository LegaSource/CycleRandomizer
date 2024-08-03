using BepInEx.Configuration;
using BepInEx;
using HarmonyLib;
using CycleRandomizer.Patches;
using System.Collections.Generic;
using BepInEx.Logging;

namespace CycleRandomizer
{
    [BepInPlugin(modGUID, modName, modVersion)]
    internal class CycleRandomizer : BaseUnityPlugin
    {
        private const string modGUID = "Lega.CycleRandomizer";
        private const string modName = "Cycle Randomizer";
        private const string modVersion = "1.0.3";

        private readonly Harmony harmony = new Harmony(modGUID);

        internal static ManualLogSource mls;

        public static ConfigFile configFile;
        public static List<string> cycleMoons = new List<string>();
        public static List<string> cycleDungeons = new List<string>();
        public static List<Dictionary<string, int>> planetWeights = new List<Dictionary<string, int>>();

        void Awake()
        {
            mls = BepInEx.Logging.Logger.CreateLogSource("CycleRandomizer");
            configFile = Config;
            ConfigManager.Load();

            harmony.PatchAll(typeof(CycleRandomizer));
            harmony.PatchAll(typeof(StartOfRoundPatch));
            harmony.PatchAll(typeof(TerminalPatch));
            harmony.PatchAll(typeof(MoonPatch));
            harmony.PatchAll(typeof(DungeonPatch));
        }
    }
}