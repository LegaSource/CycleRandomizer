using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.Video;

namespace CycleRandomizer.Patches
{
    internal class StartOfRoundPatch
    {
        [HarmonyPatch(typeof(StartOfRound), "Start")]
        [HarmonyPostfix]
        private static void BindMoonsConfig(ref StartOfRound __instance)
        {
            foreach (string planetName in __instance.levels.Where(l => l.planetHasTime).Select(l => l.PlanetName))
            {
                if (!CycleRandomizer.configFile.ContainsKey(new ConfigDefinition("Moons", planetName + " Weight")))
                {
                    ConfigEntry<int> weight = CycleRandomizer.configFile.Bind<int>("Moons", planetName + " Weight", 1, "Weighting value for " + planetName + " to be randomly selected");
                    CycleRandomizer.planetWeights.Add(new Dictionary<string, int>
                    {
                        { planetName, weight.Value }
                    });
                }
            }
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.SetMapScreenInfoToCurrentLevel))]
        [HarmonyAfter(new string[] { "mrov.WeatherRegistry" })]
        [HarmonyPostfix]
        private static void HideMapScreenInfo(ref StartOfRound __instance, ref VideoPlayer ___screenLevelVideoReel, ref TextMeshProUGUI ___screenLevelDescription)
        {
            if (ConfigManager.isMoonScreenHided.Value && !__instance.currentLevel.name.Equals("CompanyBuildingLevel"))
            {
                ___screenLevelDescription.text = "Orbiting: Unknown\nPOPULATION: Unknown\nCONDITIONS: Unknown\nFAUNA: Unknown\nWeather: Unknown";
                ___screenLevelVideoReel.enabled = false;
                ___screenLevelVideoReel.clip = null;
                ___screenLevelVideoReel.gameObject.SetActive(false);
                ___screenLevelVideoReel.Stop();
            }
        }
    }
}
