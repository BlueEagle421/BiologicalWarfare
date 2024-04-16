using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace BiologicalWarfare
{
    public class JobDriver_InsertSample : JobDriver
    {
        private Thing TargetItem => job.GetTarget(TargetIndex.A).Thing;
        private Building TargetBuilding => job.GetTarget(TargetIndex.B).Thing as Building;
        private bool IsFull => TargetBuilding.GetComp<CompDiseaseSampleContainer>().Full;
        private const int installDurationTicks = 300;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            bool successfullyReservedItem = pawn.Reserve(TargetItem, job, 1, -1, null, true);
            bool successfullyReservedBuilding = pawn.Reserve(TargetBuilding, job, 1, -1, null, true);
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
        public override bool TryMakePreToilReservations(bool errorOnFailed) => pawn.Reserve(TargetBuilding, job, 1, -1, null, errorOnFailed);
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
                TargetBuilding.TryGetInnerInteractableThingOwner().TryDropAll(pawn.Position, pawn.Map, ThingPlaceMode.Near, null, null, true);
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
        private BuildingVaccineResearchStation Station => (BuildingVaccineResearchStation)TargetThingA;

        public override bool TryMakePreToilReservations(bool errorOnFailed) => pawn.Reserve(job.targetA, job, 1, -1, null, true, false);
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null, false);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell, false);

            Toil researchToil = new Toil();
            researchToil.tickAction = delegate ()
            {
                Pawn actor = researchToil.actor;

                float progress = actor.GetStatValue(StatDefOf.ResearchSpeed, true, -1);
                progress *= TargetThingA.GetStatValue(StatDefOf.ResearchSpeedFactor, true, -1);

                Station.ResearchPerformed(progress, actor);

                float xpToLearn = 0.1f;
                actor.skills?.GetSkill(SkillDefOf.Intellectual).Learn(xpToLearn, false, false);

                actor.GainComfortFromCellIfPossible(true);
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
            yield break;
        }
    }
}