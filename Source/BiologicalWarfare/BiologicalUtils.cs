using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace BiologicalWarfare
{
    public static class BiologicalUtils
    {
        public static string ToStringUncapitalized(this DiseaseType diseaseType) => diseaseType.ToString().UncapitalizeFirst();

        public static void SpawnThingAt(Map map, List<IntVec3> cells, ThingDef thingDef, int count)
        {
            foreach (IntVec3 cell in cells)
            {
                if (!cell.Walkable(map))
                    continue;

                Thing firstThing = cell.GetFirstThing(map, thingDef);
                if (firstThing != null)
                {
                    if (firstThing.stackCount + count <= firstThing.def.stackLimit)
                    {
                        firstThing.stackCount += count;
                        break;
                    }
                }
                else
                {
                    Thing thing = ThingMaker.MakeThing(thingDef);
                    thing.stackCount = count;
                    if (GenPlace.TryPlaceThing(thing, cell, map, ThingPlaceMode.Near))
                        break;
                }

            }
        }

        public static ThingDef FirstSampleDefFrom(List<Hediff> hediffs)
        {
            if (hediffs == null)
                return null;

            CombatDiseaseDef foundDisease = DefDatabase<CombatDiseaseDef>.AllDefs.ToList()
                .Find(disease => hediffs.Any(x => x.def == disease.hediffDef && disease.canBeSampled));

            if (foundDisease == null)
                return null;

            return foundDisease.sampleDef;
        }

        public static float DistanceTo(this IntVec3 a, IntVec3 b)
        {
            float num = a.x - b.x;
            float num2 = a.y - b.y;
            float num3 = a.z - b.z;
            return (float)Math.Sqrt(num * num + num2 * num2 + num3 * num3);
        }
    }
}
