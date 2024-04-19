using OPToxic;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace BiologicalWarfare
{
    public static class BiologicalUtils
    {
        public static string ToStringUncapitalized(this DiseaseType diseaseType) => diseaseType.ToString().UncapitalizeFirst();

        public static float CELL_CONTAMINATION_CHANCE = 0.8f; //8%
        public static int CONTAMINATION_TICKS = 2500 * 8; //8 hours

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
                .Find(disease => hediffs.Any(x => disease.samplableHediffDefs.Contains(x.def) && disease.CanBeSampled));

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

        public static float GetSkill(this Pawn pawn, SkillDef skillDef)
        {
            if (pawn.skills != null)
                return pawn.skills.GetSkill(skillDef).Level;

            if (pawn.IsColonyMech)
                return pawn.RaceProps.mechFixedSkillLevel;

            return 1f;
        }

        public static void DoPathogenInfection(Thing thingInfecter, Pawn pawn)
        {
            if (!CanPathogenInfect(pawn))
                return;

            HediffDef hediffToAdd = DefDatabase<HediffDef>.GetNamedSilentFail(OPToxicDefGetValue.OPToxicGetHediff(thingInfecter.def));

            if (hediffToAdd == null)
                return;

            if (IsImmuneTo(pawn, hediffToAdd))
                return;

            HediffSet hediffSet = pawn.health.hediffSet;
            Hediff hediffFound = (hediffSet?.GetFirstHediffOfDef(hediffToAdd, false));

            float statValue = 1 - pawn.GetStatValue(StatDefOf.ToxicResistance, true);
            float severityToSet = Mathf.Max(OPToxicDefGetValue.OPToxicGetSev(thingInfecter.def), hediffToAdd.minSeverity);

            severityToSet = Rand.Range(severityToSet / 2f, severityToSet) * statValue;

            if (hediffFound != null && severityToSet > 0f)
            {
                hediffFound.Severity += severityToSet;
                return;
            }

            Hediff hediffMade = HediffMaker.MakeHediff(hediffToAdd, pawn);
            hediffMade.Severity = severityToSet;
            pawn.health.AddHediff(hediffMade);
        }

        public static bool CanPathogenInfect(Pawn pawn)
        {
            if (pawn == null)
                return false;

            if (pawn.RaceProps.IsMechanoid)
                return false;

            if (pawn.IsGhoul)
                return false;

            if (pawn.GetStatValue(StatDefOf.ToxicEnvironmentResistance) >= 0.8f)
                return false;

            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Breathing))
                return false;

            return true;
        }

        public static bool IsImmuneTo(Pawn pawn, HediffDef hediffDef)
        {
            if (pawn.health.immunity.AnyGeneMakesFullyImmuneTo(hediffDef))
                return true;

            if (AnyHediffMakesFullyImmuneTo(pawn, hediffDef))
                return true;

            return false;
        }

        public static bool AnyHediffMakesFullyImmuneTo(Pawn pawn, HediffDef hediffDef)
        {
            List<Hediff> allHediffs = pawn.health.hediffSet.hediffs;

            if (allHediffs == null || allHediffs.Count == 0)
                return false;

            if (allHediffs.Any(x => CanCheckHediff(x) && x.CurStage.makeImmuneTo.Any(y => y == hediffDef)))
                return true;

            return false;
        }

        private static bool CanCheckHediff(Hediff hediff) => hediff.CurStage != null && hediff.CurStage.makeImmuneTo != null;

        public static bool RandomChance(float chance)
        {
            float randomValue = UnityEngine.Random.value;

            if (randomValue >= chance)
                return false;

            return true;
        }

        public static void TryToContaminate(Thing thingSource, CombatDiseaseDef combatDiseaseDef, float chance)
        {
            if (combatDiseaseDef == null)
                return;

            if (thingSource == null)
                return;

            ThingDef filthDef = combatDiseaseDef.filthDef;

            if (filthDef == null)
                return;

            if (RandomChance(chance))
            {
                ContaminateAreaAt(thingSource, filthDef);
                Messages.Message("USH_AreaContaminated".Translate(combatDiseaseDef.label.CapitalizeFirst(), thingSource.Label), thingSource, MessageTypeDefOf.NegativeEvent);
            }
        }

        public static void TryToContaminate(Thing thingSource, CombatDiseaseDef combatDiseaseDef)
        {
            if (combatDiseaseDef == null)
                return;

            if (thingSource == null)
                return;

            ThingDef filthDef = combatDiseaseDef.filthDef;

            if (filthDef == null)
                return;

            if (RandomChance(thingSource.GetStatValue(USHDefOf.USH_ContaminationChanceFactor)))
            {
                ContaminateAreaAt(thingSource, filthDef);
                Messages.Message("USH_AreaContaminated".Translate(combatDiseaseDef.label.CapitalizeFirst(), thingSource.Label), thingSource, MessageTypeDefOf.NegativeEvent);
            }
        }

        public static void ContaminateAreaAt(Thing thingSource, ThingDef filthDef)
        {
            Map map = thingSource.Map;

            List<IntVec3> cells = thingSource.CellsAdjacent8WayAndInside().ToList();

            foreach (IntVec3 cell in cells)
            {
                if (!cell.Walkable(map))
                    continue;

                Thing firstThing = cell.GetFirstThing(map, filthDef);
                if (firstThing != null)
                    continue;

                Thing thing = ThingMaker.MakeThing(filthDef);

                GenPlace.TryPlaceThing(thing, cell, map, ThingPlaceMode.Near);
            }

            Room room = thingSource.GetRoom();

            if (room == null)
                return;

            if (!room.ProperRoom)
                return;

            foreach (IntVec3 cell in room.Cells.ToList())
            {
                if (!RandomChance(CELL_CONTAMINATION_CHANCE))
                    continue;

                if (!cell.Walkable(map))
                    continue;

                Thing firstThing = cell.GetFirstThing(map, filthDef);

                if (firstThing != null)
                    continue;

                Thing thing = ThingMaker.MakeThing(filthDef);
                if (GenPlace.TryPlaceThing(thing, cell, map, ThingPlaceMode.Near))
                    break;
            }
        }
    }
}
