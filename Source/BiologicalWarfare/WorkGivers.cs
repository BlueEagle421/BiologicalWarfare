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
            BuildingVaccineResearchStation buildingVaccineStation = t as BuildingVaccineResearchStation;
            //RimatomicResearchDef currentProj = buildingVaccineStation.currentProj;
            //if (currentProj == null)
            //{
            //    return false;
            //}
            //if (!currentProj.CurrentStep.requiredResearchFacilities.NullOrEmpty<ThingDef>())
            //{
            //    return false;
            //}
            //if (currentProj.CurrentStep.WorkType != this.def.workType)
            //{
            //    return false;
            //}
            //int skillLevel = currentProj.CurrentStep.SkillLevel;
            //if (skillLevel > 0)
            //{
            //    foreach (SkillDef skillDef in this.def.workType.relevantSkills)
            //    {
            //        float skill = pawn.GetSkill(skillDef);
            //        if (skill < skillLevel)
            //        {
            //            JobFailReason.Is("SkillTooLow".Translate(skillDef.label, skill, skillLevel), null);
            //            return false;
            //        }
            //    }
            //}
            //return currentProj.CurrentStep.CanBeResearchedAt(buildingVaccineStation, false) && pawn.CanReserve(t, 1, -1, null, forced);
            return pawn.CanReserve(t, 1, -1, null, forced);
        }
    }
}
