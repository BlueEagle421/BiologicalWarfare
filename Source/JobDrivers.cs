using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace USH_BW
{
    public class JobDriver_SampleDisease : JobDriver
    {
        private Pawn PawnToSampleFrom => job.GetTarget(TargetIndex.A).Pawn;
        private Thing Item => job.GetTarget(TargetIndex.B).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (pawn.Reserve(PawnToSampleFrom, job))
                return pawn.Reserve(Item, job);

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
            ThingDef sampleToSpawn = BiologicalUtils.FirstSampleDefFrom(PawnToSampleFrom.health.hediffSet.hediffs);

            if (sampleToSpawn == null)
            {
                Messages.Message("USH_NoDisease".Translate(PawnToSampleFrom.Named("PAWN")), PawnToSampleFrom, MessageTypeDefOf.NeutralEvent);
                return;
            }

            USH_DefOf.USH_SampleDisease.PlayOneShot(SoundInfo.InMap(PawnToSampleFrom));
            BiologicalUtils.SpawnThingAt(PawnToSampleFrom.Map, PawnToSampleFrom.CellsAdjacent8WayAndInside().ToList(), sampleToSpawn, 1);
            Item.SplitOff(1).Destroy(DestroyMode.Vanish);
        }
    }

    public class JobDriver_InsertSample : JobDriver
    {
        private Thing TargetItem => job.GetTarget(TargetIndex.A).Thing;
        private Building TargetBuilding => job.GetTarget(TargetIndex.B).Thing as Building;
        private bool IsFull => TargetBuilding.GetComp<CompDiseaseSampleContainer>().Full;
        private const int installDurationTicks = 300;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            bool successfullyReservedItem = pawn.Reserve(TargetItem, job);
            bool successfullyReservedBuilding = pawn.Reserve(TargetBuilding, job);
            return successfullyReservedItem && successfullyReservedBuilding;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.Goto(TargetIndex.A, PathEndMode.OnCell).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOn(() => IsFull);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, true, false, true).FailOn(() => IsFull);
            yield return Toils_Haul.CarryHauledThingToCell(TargetIndex.C, PathEndMode.ClosestTouch).FailOn(() => IsFull);
            Toil insertToil = Toils_General.Wait(installDurationTicks, TargetIndex.B);
            insertToil.WithProgressBarToilDelay(TargetIndex.B, false, -0.5f);
            insertToil.FailOn(() => IsFull);
            insertToil.FailOnDespawnedNullOrForbidden(TargetIndex.B);
            insertToil.FailOnCannotTouch(TargetIndex.B, PathEndMode.Touch);
            insertToil.handlingFacing = true;
            yield return insertToil;

            void onDeposited()
            {
                TargetItem.def.soundDrop.PlayOneShot(pawn);
                TargetBuilding.GetComp<CompDiseaseSampleContainer>().OnInserted(pawn);
            }

            yield return Toils_Haul.DepositHauledThingInContainer(TargetIndex.B, TargetIndex.A, onDeposited);
            yield break;
        }
    }

    public class JobDriver_ExtractSample : JobDriver
    {
        private Building TargetBuilding => job.GetTarget(TargetIndex.A).Thing as Building;
        private bool CanExtract(Pawn pawn) => TargetBuilding.GetComp<CompDiseaseSampleContainer>().CanExtract(pawn).Accepted;
        public override bool TryMakePreToilReservations(bool errorOnFailed) => pawn.Reserve(TargetBuilding, job);
        protected virtual PathEndMode ContainerPathEndMode => TargetBuilding.def.hasInteractionCell ? PathEndMode.InteractionCell : PathEndMode.Touch;

        private const int extractionDurationTicks = 300;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil gotoToil = Toils_Goto.GotoThing(TargetIndex.A, ContainerPathEndMode);
            gotoToil.FailOn(() => !CanExtract(pawn));
            gotoToil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            gotoToil.FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return gotoToil;

            Toil extractionToil = Toils_General.Wait(extractionDurationTicks, TargetIndex.A);
            extractionToil.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            extractionToil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            extractionToil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            extractionToil.AddFinishAction(() =>
            {
                TargetBuilding.TryGetInnerInteractableThingOwner().TryDropAll(pawn.Position, pawn.Map, ThingPlaceMode.Near);
                TargetBuilding.GetComp<CompDiseaseSampleContainer>().OnExtracted(pawn);
            });
            extractionToil.handlingFacing = true;
            yield return extractionToil;

            yield break;
        }
    }

    public class JobDriver_VaccineResearch : JobDriver
    {
        private const int JobEndInterval = 4000;
        private Building_AntigensAnalyzer Station => (Building_AntigensAnalyzer)TargetThingA;

        public override bool TryMakePreToilReservations(bool errorOnFailed) => pawn.Reserve(job.targetA, job);
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);


            Toil researchToil = new();
            researchToil.tickIntervalAction = delegate (int delta)
            {
                Pawn actor = researchToil.actor;
                float statValue = actor.GetStatValue(StatDefOf.ResearchSpeed);
                statValue *= TargetThingA.GetStatValue(StatDefOf.ResearchSpeedFactor);
                Station.ResearchPerformed(statValue * (float)delta, actor);
                actor.skills.Learn(SkillDefOf.Intellectual, 0.1f * (float)delta);
                actor.GainComfortFromCellIfPossible(delta, chairsOnly: true);
            };
            researchToil.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            researchToil.FailOn(() => !Station.CanPerformResearch(researchToil.actor));
            researchToil.WithEffect(EffecterDefOf.Research, TargetIndex.A);
            researchToil.WithProgressBar(TargetIndex.A, delegate
            {
                ResearchProjectDef vaccineProj = Station.CurrentVaccineProject();

                if (vaccineProj == null)
                    return 0f;

                return vaccineProj.ProgressPercent;
            },
            false, -0.5f, false);
            researchToil.defaultCompleteMode = ToilCompleteMode.Delay;
            researchToil.defaultDuration = JobEndInterval;
            researchToil.activeSkill = () => SkillDefOf.Intellectual;
            yield return researchToil;
            yield return Toils_General.Wait(2, TargetIndex.None);
        }
    }
}