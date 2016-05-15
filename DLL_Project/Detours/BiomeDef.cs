using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse.AI;
using Verse;

namespace CommunityCoreLibrary
{
    internal static class _BiomeDef
    {
        internal static List<BiomePlantRecord> wildPlants = new List<BiomePlantRecord>();

        internal static List<BiomeAnimalRecord> wildAnimals = new List<BiomeAnimalRecord>();

        internal static List<BiomeDiseaseRecord> diseases = new List<BiomeDiseaseRecord>();
        [Unsaved]
        internal static Dictionary<PawnKindDef, float> cachedAnimalCommonalities;

        [Unsaved]
        internal static Dictionary<ThingDef, float> cachedPlantCommonalities;

        [Unsaved]
        internal static Dictionary<IncidentDef, float> cachedDiseaseCommonalities;
        internal static float _CommonalityOfAnimal(this BiomeDef _this, PawnKindDef animalDef)
        {
            if (cachedAnimalCommonalities == null)
            {
                cachedAnimalCommonalities = new Dictionary<PawnKindDef, float>();
                for (int index = 0; index < wildAnimals.Count; ++index)
                    cachedAnimalCommonalities.Add(wildAnimals[index].animal, wildAnimals[index].commonality);
                foreach (PawnKindDef pawnKindDef in DefDatabase<PawnKindDef>.AllDefs)
                {
                    if (pawnKindDef.RaceProps.wildBiomes != null)
                    {
                        for (int index = 0; index < pawnKindDef.RaceProps.wildBiomes.Count; ++index)
                            if (pawnKindDef.RaceProps.wildBiomes[index].biome.defName == _this.defName)
                                cachedAnimalCommonalities.Add(pawnKindDef.RaceProps.wildBiomes[index].animal, pawnKindDef.RaceProps.wildBiomes[index].commonality);
                    }
                }
            }
            float num;
            if (cachedAnimalCommonalities.TryGetValue(animalDef, out num))
                return num;
            return 0.0f;
        }

        internal static float _CommonalityOfPlant(this BiomeDef _this, ThingDef plantDef)
        {
            if (cachedPlantCommonalities == null)
            {
                cachedPlantCommonalities = new Dictionary<ThingDef, float>();
                for (int i = 0; i < wildPlants.Count; i++)
                {
                    cachedPlantCommonalities.Add(wildPlants[i].plant, wildPlants[i].commonality);
                }
                foreach (ThingDef current in DefDatabase<ThingDef>.AllDefs)
                {
                    if (current.plant != null && current.plant.wildBiomes != null)
                    {
                        for (int index = 0; index < current.plant.wildBiomes.Count; ++index)
                            if (current.plant.wildBiomes[index].biome.defName == _this.defName)
                                cachedPlantCommonalities.Add(current.plant.wildBiomes[index].plant, current.plant.wildBiomes[index].commonality);
                    }
                }
            }
            float result;
            if (cachedPlantCommonalities.TryGetValue(plantDef, out result))
            {
                return result;
            }
            return 0f;
        }

        internal static float _MTBDaysOfDisease(this BiomeDef _this, IncidentDef diseaseInc)
        {
            if (cachedDiseaseCommonalities == null)
            {
                cachedDiseaseCommonalities = new Dictionary<IncidentDef, float>();
                for (int i = 0; i < diseases.Count; i++)
                {
                    cachedDiseaseCommonalities.Add(diseases[i].diseaseInc, diseases[i].mtbDays);
                }
                foreach (IncidentDef current in DefDatabase<IncidentDef>.AllDefs)
                {
                    if (current.diseaseBiomeRecords != null)
                    {
                        for (int index = 0; index < current.diseaseBiomeRecords.Count; ++index)
                            if (current.diseaseBiomeRecords[index].biome.defName == _this.defName)
                                cachedDiseaseCommonalities.Add(current.diseaseBiomeRecords[index].diseaseInc, current.diseaseBiomeRecords[index].mtbDays);
                    }
                }
            }
            float result;
            if (cachedDiseaseCommonalities.TryGetValue(diseaseInc, out result))
            {
                return result;
            }
            return 9999999f;
        }
    }
}
