using HarmonyLib;
using LethalLevelLoader;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CycleRandomizer.Patches
{
    internal class TerminalPatch
    {
        [HarmonyPatch(typeof(Terminal), "Awake")]
        [HarmonyPostfix]
        private static void AddCommands(ref Terminal __instance)
        {
            AddCycleRandomCommand(ref __instance);
            AddCycleDisplayCommands(ref __instance);
            AddCycleAddRemoveCommands(ref __instance, "Moon", "cam", "crm", StartOfRound.Instance.levels.Where(l => l.planetHasTime).Select(l => l.PlanetName));
            AddCycleAddRemoveCommands(ref __instance, "Dungeon", "cad", "crd", PatchedContent.ExtendedDungeonFlows.Select(d => d.DungeonName).Distinct());
        }

        [HarmonyPatch(typeof(Terminal), "ParsePlayerSentence")]
        [HarmonyPostfix]
        private static void RunCommands(ref TerminalNode __result, ref Terminal __instance)
        {
            if (__result == null)
            {
                return;
            }
            if (!GameNetworkManager.Instance.localPlayerController.IsServer
                && !GameNetworkManager.Instance.localPlayerController.IsHost
                && (__result.name.Contains("cycle") || __result.name.Contains("Cycle")))
            {
                __result = CreateTerminalNode
                (
                    name: "Unauthorized",
                    displayText: "Command not found or usable only by host.\n\n\n"
                );
            }
            else if (GameNetworkManager.Instance.localPlayerController.IsServer || GameNetworkManager.Instance.localPlayerController.IsHost)
            {
                if (__result.name.Equals("cyclerandomNode"))
                {
                    HandleCycleRandomNode(ref __result, ref __instance);
                }
                else if (__result.name.Contains("cycleaddmoon") || __result.name.Contains("cycleremovemoon"))
                {
                    HandleCycleAddRemoveNode(ref __result, "moon", __result.name.Contains("add"));
                }
                else if (__result.name.Contains("cycleadddungeon") || __result.name.Contains("cycleremovedungeon"))
                {
                    HandleCycleAddRemoveNode(ref __result, "dungeon", __result.name.Contains("add"));
                }
            }
        }

        private static void AddCycleRandomCommand(ref Terminal __instance)
        {
            TerminalKeyword routeKeyword = __instance.terminalNodes.allKeywords.FirstOrDefault(k => k.name == "Route");
            TerminalKeyword cycleKeyword = CreateTerminalKeyword
            (
                name: "CycleRandom",
                word: "cyclerandom",
                defaultVerb: routeKeyword
            );
            __instance.terminalNodes.allKeywords = CollectionExtensions.AddToArray(__instance.terminalNodes.allKeywords, cycleKeyword);

            routeKeyword.compatibleNouns = CollectionExtensions.AddToArray(routeKeyword.compatibleNouns, new CompatibleNoun
            {
                noun = cycleKeyword,
                result = CreateTerminalNode
                (
                    name: "cyclerandomNode",
                    displayText: null
                )
            });
        }

        private static void AddCycleDisplayCommands(ref Terminal __instance)
        {
            AddCycleDisplayCommand(ref __instance, "CycleDisplayMoons", "cdm", "CycleMCatalogue", "List of moons that cannot be randomly selected:\n\n\n");
            AddCycleDisplayCommand(ref __instance, "CycleDisplayDungeons", "cdd", "CycleDCatalogue", "List of dungeons that cannot be randomly selected:\n\n\n");
        }

        private static void AddCycleDisplayCommand(ref Terminal __instance, string name, string word, string nodeName, string displayText)
        {
            TerminalKeyword cycleDisplayKeyword = CreateTerminalKeyword
            (
                name: name,
                word: word,
                specialKeywordResult: CreateTerminalNode
                (
                    name: nodeName,
                    displayText: displayText
                )
            );
            __instance.terminalNodes.allKeywords = CollectionExtensions.AddToArray(__instance.terminalNodes.allKeywords, cycleDisplayKeyword);
        }

        private static void AddCycleAddRemoveCommands(ref Terminal __instance, string type, string addWord, string removeWord, IEnumerable<string> names)
        {
            TerminalKeyword addKeyword = CreateTerminalKeyword
            (
                name: $"CycleAdd{type}",
                word: addWord,
                isVerb: true
            );
            __instance.terminalNodes.allKeywords = CollectionExtensions.AddToArray(__instance.terminalNodes.allKeywords, addKeyword);

            TerminalKeyword removeKeyword = CreateTerminalKeyword
            (
                name: $"CycleRemove{type}",
                word: removeWord,
                isVerb: true
            );
            __instance.terminalNodes.allKeywords = CollectionExtensions.AddToArray(__instance.terminalNodes.allKeywords, removeKeyword);

            foreach (string name in names)
            {
                string processedName = GetNameOnlyWithLetters(name);
                string firstPartName = GetFirstPart(name);
                if (!string.IsNullOrEmpty(firstPartName))
                {
                    AddCycleAddRemoveCommand(ref __instance, ref addKeyword, $"cycleadd{type.ToLower()}{processedName.ToLower()}Node", ref firstPartName);
                    AddCycleAddRemoveCommand(ref __instance, ref removeKeyword, $"cycleremove{type.ToLower()}{processedName.ToLower()}Node", ref firstPartName);
                }
                AddCycleAddRemoveCommand(ref __instance, ref addKeyword, $"cycleadd{type.ToLower()}{processedName.ToLower()}NodeFull", ref processedName);
                AddCycleAddRemoveCommand(ref __instance, ref removeKeyword, $"cycleremove{type.ToLower()}{processedName.ToLower()}NodeFull", ref processedName);
            }
        }

        private static void AddCycleAddRemoveCommand(ref Terminal __instance, ref TerminalKeyword parentKeyword, string nodeWord, ref string name)
        {
            TerminalKeyword keyword = CreateTerminalKeyword
            (
                name: $"{parentKeyword.name}{name}",
                word: name.ToLower(),
                defaultVerb: parentKeyword
            );
            __instance.terminalNodes.allKeywords = CollectionExtensions.AddToArray(__instance.terminalNodes.allKeywords, keyword);

            parentKeyword.compatibleNouns = CollectionExtensions.AddToArray(parentKeyword.compatibleNouns, new CompatibleNoun
            {
                noun = keyword,

                result = CreateTerminalNode
                (
                    name: nodeWord,
                    displayText: null
                )
            });
        }

        private static void HandleCycleRandomNode(ref TerminalNode __result, ref Terminal __instance)
        {
            List<CompatibleNoun> routes = __instance.terminalNodes.allKeywords
                .First(k => k.name.Equals("Route"))
                .compatibleNouns
                .Where(n => n.result.buyRerouteToMoon == -2 && !CycleRandomizer.cycleMoons.Contains(StartOfRound.Instance.levels[n.result.displayPlanetInfo].PlanetName))
                .ToList();

            if (routes.Count <= 0)
            {
                __result = CreateTerminalNode
                (
                    name: "NoMoonFound",
                    displayText: "No moon found.\n\n\n"
                );
                return;
            }

            CompatibleNoun route = GetRandomMoon(ref routes);
            if (route == null)
            {
                CycleRandomizer.mls.LogInfo("No moons could be recovered from the weight configurations.");
                route = routes[new System.Random().Next(routes.Count)];
            }
            TerminalNode randomNode = route.result.terminalOptions.First(c => c.noun.name == "Confirm").result;
            __result = CreateTerminalNode
            (
                name: "NoMoonInfo",
                displayText: "Routing on a random moon.\n\n\n",
                buyRerouteToMoon: randomNode.buyRerouteToMoon
            );
        }

        private static CompatibleNoun GetRandomMoon(ref List<CompatibleNoun> compatibleNouns)
        {
            // Ajout des planètes éligibles en fonction de leur valeur d'importance
            List<CompatibleNoun> eligibleNouns = new List<CompatibleNoun>();
            foreach (CompatibleNoun compatibleNoun in compatibleNouns)
            {
                for (int i = 0; i < FindWeightByMoon(StartOfRound.Instance.levels[compatibleNoun.result.displayPlanetInfo].PlanetName); i++)
                {
                    eligibleNouns.Add(compatibleNoun);
                }
            }
            // Sélectionner une planète aléatoire parmi celles éligibles
            if (eligibleNouns.Count > 0)
            {
                return eligibleNouns[new System.Random().Next(eligibleNouns.Count)];
            }
            else
            {
                return null;
            }
        }

        public static int? FindWeightByMoon(string planetName)
        {
            foreach (var planetWeight in CycleRandomizer.planetWeights)
            {
                if (planetWeight.TryGetValue(planetName, out int weight))
                {
                    return weight;
                }
            }
            return null;
        }

        private static void HandleCycleAddRemoveNode(ref TerminalNode __result, string type, bool isAdd)
        {
            int prefixLength = ((isAdd ? "cycleadd" : "cycleremove") + type).Length;
            string typeNode = __result.name.Substring(prefixLength, __result.name.IndexOf("Node") - prefixLength);

            string name = type == "moon"
                ? StartOfRound.Instance.levels.Where(l => GetNameOnlyWithLetters(l.PlanetName).ToLower().Contains(typeNode)).FirstOrDefault()?.PlanetName
                : PatchedContent.ExtendedDungeonFlows.Select(d => d.DungeonName).Distinct().Where(d => GetNameOnlyWithLetters(d).ToLower().Contains(typeNode)).FirstOrDefault();

            if (string.IsNullOrEmpty(name))
            {
                __result = CreateTerminalNode
                (
                    name: $"Cycle{(isAdd ? "Add" : "Remove")}{type}NotFound",
                    displayText: $"{(type == "moon" ? "Moon" : "Dungeon")} not found.\n\n\n"
                );
                return;
            }

            bool success = type == "moon"
                ? (isAdd ? MoonPatch.AddCycleMoon(name) : MoonPatch.RemoveCycleMoon(name))
                : (isAdd ? DungeonPatch.AddCycleDungeon(name) : DungeonPatch.RemoveCycleDungeon(name));

            __result = CreateTerminalNode
            (
                name: $"Cycle{(isAdd ? "Add" : "Remove")}{type}Info{(success ? "Success" : "Fail")}",
                displayText: name + (success ? $" has been {(isAdd ? "added to" : "removed from")} the list.\n\n\n" : (isAdd ? $" is already in the list.\n\n\n" : $" is not in the list.\n\n\n"))
            );
        }

        private static TerminalKeyword CreateTerminalKeyword(string name, string word, bool isVerb = false, TerminalKeyword defaultVerb = null, TerminalNode specialKeywordResult = null)
        {
            TerminalKeyword terminalKeyword = ScriptableObject.CreateInstance<TerminalKeyword>();
            terminalKeyword.name = name;
            terminalKeyword.word = word;
            terminalKeyword.isVerb = isVerb;
            terminalKeyword.defaultVerb = defaultVerb;
            terminalKeyword.specialKeywordResult = specialKeywordResult;
            return terminalKeyword;
        }

        private static TerminalNode CreateTerminalNode(string name, string displayText, int buyRerouteToMoon = -1, int itemCost = 0, bool clearPreviousText = true)
        {
            TerminalNode terminalNode = ScriptableObject.CreateInstance<TerminalNode>();
            terminalNode.name = name;
            terminalNode.displayText = displayText;
            terminalNode.buyRerouteToMoon = buyRerouteToMoon;
            terminalNode.itemCost = itemCost;
            terminalNode.clearPreviousText = clearPreviousText;
            return terminalNode;
        }

        // Récupère la première partie du string s'il en existe au moins deux
        private static string GetFirstPart(string name)
        {
            // ^[^a-zA-Z]* -> Permet de skip les caractères qui ne sont pas des lettres au début du string
            // ([a-zA-Z]+) -> Permet de capturer la première séquence de lettres (majuscule ou minuscule) trouvée après les caractères ignorés
            // [^a-zA-Z]+ -> Permet de vérifier qu'il existe au moins un caractère non alphabétique après la première séquence de lettres
            // [a-zA-Z] -> Permet de vérifier qu'il existe au moins une lettre après la première séquence de lettres
            var match = Regex.Match(name, @"^[^a-zA-Z]*([a-zA-Z]+)[^a-zA-Z]+[a-zA-Z]");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        private static string GetNameOnlyWithLetters(string name)
        {
            return new string(name.Where(c => char.IsLetter(c)).ToArray());
        }
    }
}
