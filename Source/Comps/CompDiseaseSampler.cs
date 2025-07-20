using RimWorld;
using Verse;
using Verse.AI;

namespace USH_BW
{
    public class CompProperties_TargetEffectSampleDisease : CompProperties
    {
        public JobDef jobDef;

        public CompProperties_TargetEffectSampleDisease()
        {
            compClass = typeof(CompTargetEffect_SampleDisease);
        }
    }

    public class CompTargetEffect_SampleDisease : CompTargetEffect
    {
        public CompProperties_TargetEffectSampleDisease PropsSampleDisease => (CompProperties_TargetEffectSampleDisease)props;
        public override void DoEffectOn(Pawn user, Thing target)
        {
            if (!user.IsColonistPlayerControlled)
                return;

            if (!user.CanReserveAndReach(target, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false))
                return;

            Job job = JobMaker.MakeJob(PropsSampleDisease.jobDef, target, parent);
            job.count = 1;
            user.jobs.TryTakeOrderedJob(job);
        }
    }
}
