using RimWorld;
using Verse;
using Verse.AI;

namespace USH_BW
{
    public class WorkGiver_VaccineResearcher : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(USH_DefOf.USH_AntigensAnalyzer);
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false) => new Job(USH_DefOf.USH_ResearchVaccine, t);
        public override float GetPriority(Pawn pawn, TargetInfo t) => t.Thing.GetStatValue(StatDefOf.ResearchSpeedFactor, true, -1);
        public override bool Prioritized => true;

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_AntigensAnalyzer station = t as Building_AntigensAnalyzer;

            return station.CanPerformResearch(pawn) && pawn.CanReserve(t, 1, -1, null, forced);
        }
    }
}
