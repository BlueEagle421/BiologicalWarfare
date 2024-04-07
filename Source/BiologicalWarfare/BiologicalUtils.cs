

using System.Collections.Generic;
using Verse;

namespace BiologicalWarfare
{
    public static class BiologicalUtils
    {
        public static string ToStringUncapitalized(this DiseaseType diseaseType) => diseaseType.ToString().UncapitalizeFirst();

        public static void SpawnThingAt(Map map, List<IntVec3> cells, ThingDef thingDef, int count)
        {
            for (int index = 0; index < cells.Count; ++index)
            {
                IntVec3 cell = cells[index];
                if (cell.Walkable(map))
                {
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
        }
    }
}
