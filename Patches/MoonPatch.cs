using HarmonyLib;
using System.Linq;

namespace CycleRandomizer.Patches
{
    internal class MoonPatch
    {
        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.StartGame))]
        [HarmonyPostfix]
        private static void StartGame(ref SelectableLevel ___currentLevel)
        {
            AddCycleMoon(___currentLevel.PlanetName, true);
        }

        internal static bool AddCycleMoon(string planetName, bool autoClear = false)
        {
            if (!CycleRandomizer.cycleMoons.Contains(planetName))
            {
                CycleRandomizer.cycleMoons.Add(planetName);
            }
            else
            {
                return false;
            }
            if (autoClear && CycleRandomizer.cycleMoons.Count == StartOfRound.Instance.levels.Where(l => l.planetHasTime).Count())
            {
                CycleRandomizer.cycleMoons.Clear();
            }
            RefreshTerminalCycleMoons();
            return true;
        }

        internal static bool RemoveCycleMoon(string planetName)
        {
            if (!CycleRandomizer.cycleMoons.Contains(planetName))
            {
                return false;
            }
            CycleRandomizer.cycleMoons.Remove(planetName);
            RefreshTerminalCycleMoons();
            return true;
        }

        internal static void RefreshTerminalCycleMoons()
        {
            string displayMoonsText = "List of moons that cannot be randomly selected:\n\n";
            foreach (string planetName in CycleRandomizer.cycleMoons)
            {
                displayMoonsText += "* " + planetName + "\n";
            }
            displayMoonsText += "\n\n";

            Terminal terminalScript = (Terminal)AccessTools.Field(typeof(HUDManager), "terminalScript").GetValue(HUDManager.Instance);
            terminalScript.terminalNodes.allKeywords.First(k => k.name.Equals("CycleDisplayMoons")).specialKeywordResult.displayText = displayMoonsText;
        }
    }
}
