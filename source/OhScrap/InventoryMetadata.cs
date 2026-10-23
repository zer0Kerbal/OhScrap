using System;
using System.Collections.Generic;
using System.Linq;
using ScrapYard;

namespace OhScrap
{
    public static class InventoryMetadata
    {
        public static ConfigNode GetForInventoryPart(ConfigNode inventoryPartNode)
        {
            ConfigNode metadata = new ConfigNode("OHS_INVENTORY_METADATA");
            if (inventoryPartNode == null)
            {
                return metadata;
            }

            string name = inventoryPartNode.GetValue("_name");
            string idText = inventoryPartNode.GetValue("_id");
            uint id = 0;
            uint.TryParse(idText, out id);

            int uses = 0;
            int.TryParse(inventoryPartNode.GetValue("_timesRecovered"), out uses);

            int generation = GetGeneration(id, name);
            bool tested = uses > 0 || (Utils.instance != null && Utils.instance.testedParts.Contains(id));
            int? safetyRating = GetSafetyRating(name, generation, uses, tested);

            if (generation > 0)
            {
                metadata.SetValue("Generation", generation, true);
            }
            metadata.SetValue("PreviousUses", uses, true);
            metadata.SetValue("Tested", tested, true);
            if (safetyRating.HasValue)
            {
                metadata.SetValue("SafetyRating", safetyRating.Value, true);
            }

            return metadata;
        }

        private static int GetGeneration(uint id, string partName)
        {
            if (id != 0 && tils.instance != null)
            {
                int knownGeneration;
                if (Utils.instance.generations.TryGetValue(id, out knownGeneration))
                {
                    return knownGeneration;
                }
            }

            Part prefab = GetPrefab(partName);
            if (id != 0 && prefab != null && Utils.instance != null)
            {
                return Utils.instance.GetGeneration(id, prefab);
            }

            return 0;
        }

        private static int? GetSafetyRating(string partName, int generation, int uses, bool tested)
        {
            Part prefab = GetPrefab(partName);
            if (prefab == null || generation <= 0)
            {
                return null;
            }

            List<int> ratings = new List<int>();
            foreach (BaseFailureModule module in prefab.Modules.OfType<BaseFailureModule>())
            {
                if (!FailureModuleCanRate(prefab, module))
                {
                    continue;
                }

                ratings.Add(CalculateSafetyRating(prefab, module, generation, uses, tested));
            }

            if (ratings.Count == 0)
            {
                return null;
            }

            return (int)Math.Round(ratings.Average());
        }

        private static Part GetPrefab(string partName)
        {
            if (string.IsNullOrEmpty(partName) || PartLoader.LoadedPartsList == null)
            {
                return null;
            }

            AvailablePart availablePart = PartLoader.LoadedPartsList
                .FirstOrDefault(p => string.Equals(p.name, partName, StringComparison.OrdinalIgnoreCase));
            return availablePart == null ? null : availablePart.partPrefab;
        }

        private static bool FailureModuleCanRate(Part prefab, BaseFailureModule module)
        {
            if (module == null || module.excluded)
            {
                return false;
            }

            if (module is TankFailureModule)
            {
                return HasLeakableResource(prefab);
            }

            return true;
        }

        private static bool HasLeakableResource(Part prefab)
        {
            if (prefab == null || prefab.Resources == null || prefab.Resources.Count == 0)
            {
                return false;
            }

            List<string> blacklist = new List<string>();
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("OHSCRAP_RESOURCE_BLACKLIST"))
            {
                string name = node.GetValue("name");
                if (!string.IsNullOrEmpty(name))
                {
                    blacklist.Add(name);
                }
            }

            foreach (PartResource resource in prefab.Resources)
            {
                if (!blacklist.Contains(resource.resourceName))
                {
                    return true;
                }
            }

            return false;
        }

        private static int CalculateSafetyRating(Part prefab, BaseFailureModule module, int generation, int uses, bool tested)
        {
            int cappedGeneration = Math.Max(1, Math.Min(generation, 10));
            int expectedLifetime = Math.Max(1, module.expectedLifetime);
            float baseChance = module.baseChanceOfFailure;
            bool isSrb = module is SRBFailureModule;

            ApplyEngineSpecialCase(prefab, module, ref expectedLifetime, ref baseChance);

            float initialFailureRate = isSrb
                ? baseChance / cappedGeneration
                : baseChance + 0.01f - (cappedGeneration * (baseChance / 10));

            float chanceOfFailure = initialFailureRate;
            if (uses != 0 && tested)
            {
                chanceOfFailure = initialFailureRate * (uses / (float)expectedLifetime);
            }

            if (Utils.instance != null && chanceOfFailure < Utils.instance.minimumFailureChance)
            {
                chanceOfFailure = Utils.instance.minimumFailureChance;
            }

            if (uses > expectedLifetime)
            {
                float endOfLifeBonus = expectedLifetime / (float)uses;
                chanceOfFailure += (1 - endOfLifeBonus) / 10;
            }

            return RatingFromChance(chanceOfFailure, baseChance, isSrb);
        }

        private static void ApplyEngineSpecialCase(Part prefab, BaseFailureModule module, ref int expectedLifetime, ref float baseChance)
        {
            EngineFailureModule engineFailure = module as EngineFailureModule;
            if (engineFailure == null || prefab == null)
            {
                return;
            }

            ModuleEngines engine = prefab.FindModuleImplementing<ModuleEngines>();
            if (engine == null)
            {
                return;
            }

            float staticPressure = (float)(FlightGlobals.GetHomeBody().GetPressure(0) * PhysicsGlobals.KpaToAtmospheres);
            if (engine.atmosphereCurve.Evaluate(staticPressure) > 100.0f)
            {
                return;
            }

            ConfigNode engineNode = new ConfigNode("MODULE");
            engineFailure.Save(engineNode);

            int configuredLifetime;
            float configuredChance;
            if (int.TryParse(engineNode.GetValue("spaceEngineExpectedLifetime"), out configuredLifetime))
            {
                expectedLifetime = Math.Max(1, configuredLifetime);
            }
            if (float.TryParse(engineNode.GetValue("spaceEngineBaseChanceOfFailure"), out configuredChance))
            {
                baseChance = configuredChance;
            }
        }

        private static int RatingFromChance(float chanceOfFailure, float baseChance, bool isSrb)
        {
            if (!isSrb)
            {
                if (chanceOfFailure <= baseChance / 10) return 10;
                if (chanceOfFailure < baseChance / 10 * 2) return 9;
                if (chanceOfFailure < baseChance / 10 * 3) return 8;
                if (chanceOfFailure < baseChance / 10 * 4) return 7;
                if (chanceOfFailure < baseChance / 10 * 5) return 6;
                if (chanceOfFailure < baseChance / 10 * 6) return 5;
                if (chanceOfFailure < baseChance / 10 * 7) return 4;
                if (chanceOfFailure < baseChance / 10 * 8) return 3;
                if (chanceOfFailure < baseChance / 10 * 9) return 2;
                return 1;
            }

            if (chanceOfFailure <= baseChance / 10) return 10;
            if (chanceOfFailure < baseChance / 9) return 9;
            if (chanceOfFailure < baseChance / 8) return 8;
            if (chanceOfFailure < baseChance / 7) return 7;
            if (chanceOfFailure < baseChance / 6) return 6;
            if (chanceOfFailure < baseChance / 5) return 5;
            if (chanceOfFailure < baseChance / 4) return 4;
            if (chanceOfFailure < baseChance / 3) return 3;
            if (chanceOfFailure < baseChance / 2) return 2;
            return 1;
        }
    }
}