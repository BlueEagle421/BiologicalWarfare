using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace BiologicalWarfare
{
    public class WorkGiver_VaccineResearcher : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(USHDefOf.USH_AntigensAnalyzer);
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false) => new Job(USHDefOf.USH_ResearchVaccine, t);
        public override float GetPriority(Pawn pawn, TargetInfo t) => t.Thing.GetStatValue(StatDefOf.ResearchSpeedFactor, true, -1);
        public override bool Prioritized => true;

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_AntigensAnalyzer station = t as Building_AntigensAnalyzer;

            return station.CanPerformResearch(pawn) && pawn.CanReserve(t, 1, -1, null, forced);
        }
    }

    public class WorkGiver_InteractWithMarkedGatherer : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override Danger MaxPathDanger(Pawn pawn) => Danger.Deadly;

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            List<Designation> desList = pawn.Map.designationManager.AllDesignations;
            for (int i = 0; i < desList.Count; i++)
            {
                var des = desList[i];
                if (des.def == USHDefOf.USH_GathererInteraction)
                    yield return des.target.Thing;
            }
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return !pawn.Map.designationManager.AnySpawnedDesignationOfDef(USHDefOf.USH_GathererInteraction);
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (pawn.Map.designationManager.DesignationOn(t, USHDefOf.USH_GathererInteraction) == null)
                return false;

            if (!pawn.CanReserve(t, 1, -1, null, forced))
                return false;

            AcceptanceReport interactionReport = t.TryGetComp<CompMechanitesGatherer>().CanInteract(pawn);
            if (!interactionReport.Accepted)
            {
                JobFailReason.Is(interactionReport.Reason);
                return false;
            }

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false) => JobMaker.MakeJob(JobDefOf.InteractThing, t);
    }

}
