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

        //private RimatomicResearchDef Proj
        //{
        //    get
        //    {
        //        return this.Bench.currentProj;
        //    }
        //}

        private const int JobEndInterval = 4000;

        private BuildingVaccineResearchStation Station
        {
            get
            {
                return (BuildingVaccineResearchStation)base.TargetThingA;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null, true, false);
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null, false);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell, false);
            //ResearchStepDef step = this.Proj.CurrentStep;

            Toil research = new Toil();
            research.tickAction = delegate ()
            {
                Pawn actor = research.actor;

                Station.Test();

                actor.GainComfortFromCellIfPossible(false);
            };
            research.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            research.WithEffect(EffecterDefOf.Research, TargetIndex.A, null);
            research.WithProgressBar(TargetIndex.A, () => 0.5f, false, -0.5f, false);
            research.defaultCompleteMode = ToilCompleteMode.Delay;
            research.defaultDuration = JobEndInterval;
            yield return research;
            yield return Toils_General.Wait(2, TargetIndex.None);
            yield break;
        }
    }
}