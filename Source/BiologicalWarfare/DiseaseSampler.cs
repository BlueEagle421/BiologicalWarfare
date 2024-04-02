using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace BiologicalWarfare
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
        public CompProperties_TargetEffectSampleDisease Props => (CompProperties_TargetEffectSampleDisease)props;
        public override void DoEffectOn(Pawn user, Thing target)
        {
            if (!user.IsColonistPlayerControlled)
                return;

            if (!user.CanReserveAndReach(target, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false))
                return;

            Job job = JobMaker.MakeJob(Props.jobDef, target, parent);
            job.count = 1;
            user.jobs.TryTakeOrderedJob(job);
        }
    }


    public class JobDriver_SampleDisease : JobDriver
    {
        private Pawn Pawn => job.GetTarget(TargetIndex.A).Pawn;
        private Thing Item => job.GetTarget(TargetIndex.B).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (pawn.Reserve(Pawn, job, 1, -1, null, errorOnFailed))
                return pawn.Reserve(Item, job, 1, -1, null, errorOnFailed);

            return false;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.B).FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);
            Toil toil = Toils_General.Wait(120);
            toil.WithProgressBarToilDelay(TargetIndex.A);
            toil.FailOnDespawnedOrNull(TargetIndex.A);
            toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            yield return toil;
            yield return Toils_General.Do(SampleDisease);
        }

        private void SampleDisease()
        {
            SoundDef.Named("USH_SampleDisease").PlayOneShot(SoundInfo.InMap(Pawn));
            Messages.Message("A disease was sampled from: " + Pawn.ToString(), Pawn, MessageTypeDefOf.PositiveEvent);
            Item.SplitOff(1).Destroy(DestroyMode.Vanish);
        }
    }
}
