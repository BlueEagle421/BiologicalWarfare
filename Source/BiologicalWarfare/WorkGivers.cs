using RimWorld;
using Verse;
using Verse.AI;

namespace BiologicalWarfare
{
    public class WorkGiver_VaccineResearcher : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(USHDefOf.USH_VaccineResearchStation);
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false) => new Job(USHDefOf.USH_ResearchVaccine, t);
        public override float GetPriority(Pawn pawn, TargetInfo t) => t.Thing.GetStatValue(StatDefOf.ResearchSpeedFactor, true, -1);
        public override bool Prioritized => true;

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            BuildingVaccineResearchStation station = t as BuildingVaccineResearchStation;

            return station.CanPerformResearch(pawn) && pawn.CanReserve(t, 1, -1, null, forced);
        }
    }
}
