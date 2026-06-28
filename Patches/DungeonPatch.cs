using DunGen;
using HarmonyLib;
using LethalLevelLoader;
using System.Collections.Generic;
using System.Linq;

namespace CycleRandomizer.Patches;

public class DungeonPatch
{
    [HarmonyPatch(typeof(DungeonGenerator), nameof(DungeonGenerator.Generate))]
    [HarmonyPostfix]
    public static void GenerateDungeon() => AddCycleDungeon(DungeonManager.CurrentExtendedDungeonFlow.DungeonName, true);

    [HarmonyPatch(typeof(DungeonManager), nameof(DungeonManager.GetValidExtendedDungeonFlows))]
    [HarmonyPostfix]
    public static void PreventDungeon(ref List<ExtendedDungeonFlowWithRarity> __result)
    {
        if (ConfigManager.isCycleDungeon.Value)
        {
            List<ExtendedDungeonFlowWithRarity> validExtendedDungeonFlowWithRarity = [];
            foreach (ExtendedDungeonFlowWithRarity extendedDungeonFlowWithRarity in __result)
            {
                if (!CycleRandomizer.cycleDungeons.Contains(extendedDungeonFlowWithRarity.extendedDungeonFlow.DungeonName))
                    validExtendedDungeonFlowWithRarity.Add(extendedDungeonFlowWithRarity);
            }

            if (validExtendedDungeonFlowWithRarity.Count > 0)
                __result = validExtendedDungeonFlowWithRarity;
        }
    }

    public static bool AddCycleDungeon(string dungeonName, bool autoClear = false)
    {
        if (CycleRandomizer.cycleDungeons.Contains(dungeonName)) return false;

        _ = CycleRandomizer.cycleDungeons.Add(dungeonName);
        if (autoClear && CycleRandomizer.cycleDungeons.Count == PatchedContent.ExtendedDungeonFlows.Select(d => d.DungeonName).Distinct().Count())
            CycleRandomizer.cycleDungeons.Clear();

        RefreshTerminalCycleDungeons();
        return true;
    }

    public static bool RemoveCycleDungeon(string dungeonName)
    {
        if (CycleRandomizer.cycleDungeons.Contains(dungeonName))
        {
            _ = CycleRandomizer.cycleDungeons.Remove(dungeonName);
            RefreshTerminalCycleDungeons();
            return true;
        }
        return false;
    }

    public static void RefreshTerminalCycleDungeons()
    {
        string displayDungeonsText = "List of dungeons that cannot be randomly selected:\n\n";
        foreach (string dungeonName in CycleRandomizer.cycleDungeons)
            displayDungeonsText += "* " + dungeonName + "\n";
        displayDungeonsText += "\n\n";

        Terminal terminalScript = (Terminal)AccessTools.Field(typeof(HUDManager), "terminalScript").GetValue(HUDManager.Instance);
        terminalScript.terminalNodes.allKeywords.First(k => k.name.Equals("CycleDisplayDungeons")).specialKeywordResult.displayText = displayDungeonsText;
    }
}
