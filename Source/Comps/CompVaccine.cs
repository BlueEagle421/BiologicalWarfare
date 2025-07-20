using RimWorld;
using Verse;
using Verse.AI;

namespace BiologicalWarfare
{
    public class CompProperties_TargetEffectVaccine : CompProperties
    {
        public JobDef jobDef;
        public HediffDef hediffDefVaccine;

        public CompProperties_TargetEffectVaccine()
        {
            compClass = typeof(CompTargetEffect_Vaccine);
        }
    }

    public class CompTargetEffect_Vaccine : CompTargetEffect
    {
        public CompProperties_TargetEffectVaccine PropsVaccine => (CompProperties_TargetEffectVaccine)props;
        public override void DoEffectOn(Pawn user, Thing target)
        {
            if (!user.IsColonistPlayerControlled)
                return;

            if (!user.CanReserveAndReach(target, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false))
                return;

            Job job = JobMaker.MakeJob(PropsVaccine.jobDef, target, parent);
            job.count = 1;
            user.jobs.TryTakeOrderedJob(job);
        }
    }
}
