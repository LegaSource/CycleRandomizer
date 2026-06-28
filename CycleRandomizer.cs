using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CycleRandomizer.Patches;
using HarmonyLib;
using System.Collections.Generic;

namespace CycleRandomizer;

[BepInPlugin(modGUID, modName, modVersion)]
public class CycleRandomizer : BaseUnityPlugin
{
    public const string modGUID = "Lega.CycleRandomizer";
    public const string modName = "Cycle Randomizer";
    public const string modVersion = "1.0.4";

    private readonly Harmony harmony = new Harmony(modGUID);
    internal static ManualLogSource mls;
    public static ConfigFile configFile;

    public static HashSet<string> cycleMoons = [];
    public static HashSet<string> cycleDungeons = [];
    public static HashSet<Dictionary<string, int>> planetWeights = [];

    private void Awake()
    {
        mls = BepInEx.Logging.Logger.CreateLogSource("CycleRandomizer");
        configFile = Config;
        ConfigManager.Load();

        harmony.PatchAll(typeof(StartOfRoundPatch));
        harmony.PatchAll(typeof(TerminalPatch));
        harmony.PatchAll(typeof(MoonPatch));
        harmony.PatchAll(typeof(DungeonPatch));
    }
}