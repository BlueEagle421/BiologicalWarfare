using RimWorld;
using System.Collections.Generic;
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


    public class JobDriver_InjectVaccine : JobDriver
    {
        private Pawn PawnToInjectTo => job.GetTarget(TargetIndex.A).Pawn;
        private Thing Item => job.GetTarget(TargetIndex.B).Thing;
        private HediffDef HediffDefToGive => Item.TryGetComp<CompTargetEffect_Vaccine>().PropsVaccine.hediffDefVaccine;
        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (pawn.Reserve(PawnToInjectTo, job, 1, -1, null, errorOnFailed))
                return pawn.Reserve(Item, job, 1, -1, null, errorOnFailed);

            return false;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.B).FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);
            Toil toil = Toils_General.Wait(80);
            toil.WithProgressBarToilDelay(TargetIndex.A);
            toil.FailOnDespawnedOrNull(TargetIndex.A);
            toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            yield return toil;
            yield return Toils_General.Do(() => GiveVaccineHediff(PawnToInjectTo));
        }

        private void GiveVaccineHediff(Pawn pawn)
        {
            if (HediffDefToGive == null)
            {
                Log.Error(nameof(JobDriver_InjectVaccine) + " tried to add null hediff");
                return;
            }

            pawn.health.AddHediff(HediffDefToGive);
            Item.SplitOff(1).Destroy(DestroyMode.Vanish);
        }
    }

    public class HediffCompProperties_RemoveHediff : HediffCompProperties
    {
        public HediffDef removeHediffDef;
        public HediffCompProperties_RemoveHediff() => compClass = typeof(HediffCompRemoveHediff);
    }
    public class HediffCompRemoveHediff : HediffComp
    {
        public HediffCompProperties_RemoveHediff PropsVaccine => (HediffCompProperties_RemoveHediff)props;

        public override void CompPostMake()
        {
            base.CompPostMake();
            RemoveHediff();
        }

        private void RemoveHediff()
        {
            Hediff toRemove = Pawn.health.hediffSet.GetFirstHediffOfDef(PropsVaccine.removeHediffDef);

            if (toRemove != null)
                Pawn.health.hediffSet.hediffs.Remove(toRemove);
        }
    }
}
