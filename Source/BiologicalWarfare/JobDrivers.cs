using System;
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
        private bool IsFilled => TargetBuilding.GetComp<CompDiseaseSampleContainer>().Full;
        private const int insertDurationTicks = 300;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            bool successfullyReservedItem = pawn.Reserve(TargetItem, job, 1, -1, null, true);
            bool successfullyReservedBuilding = pawn.Reserve(TargetBuilding, job, 1, -1, null, true);
            return successfullyReservedItem && successfullyReservedBuilding;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.Goto(TargetIndex.A, PathEndMode.OnCell).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOn(() => IsFilled);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, true, false, true).FailOn(() => IsFilled);
            yield return Toils_Haul.CarryHauledThingToCell(TargetIndex.C, PathEndMode.ClosestTouch).FailOn(() => IsFilled);
            Toil installToil = Toils_General.Wait(insertDurationTicks, TargetIndex.B).WithProgressBarToilDelay(TargetIndex.B, false, -0.5f).FailOn(() => IsFilled);
            installToil.handlingFacing = true;
            yield return installToil;
            Action playSound = () => TargetItem.def.soundDrop.PlayOneShot(pawn);

            yield return Toils_Haul.DepositHauledThingInContainer(TargetIndex.B, TargetIndex.A, playSound);
            yield break;
        }
    }

    public class JobDriver_ExtractSample : JobDriver
    {
        private Building TargetBuilding => job.GetTarget(TargetIndex.A).Thing as Building;
        private bool IsFilled => TargetBuilding.GetComp<CompDiseaseSampleContainer>().Full;
        public override bool TryMakePreToilReservations(bool errorOnFailed) => pawn.Reserve(TargetBuilding, job, 1, -1, null, errorOnFailed);
        protected virtual PathEndMode ContainerPathEndMode => TargetBuilding.def.hasInteractionCell ? PathEndMode.InteractionCell : PathEndMode.Touch;

        private const int extractDurationTicks = 300;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, ContainerPathEndMode).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOn(() => !IsFilled).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            Toil emptyToil = Toils_General.Wait(extractDurationTicks, TargetIndex.A).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            emptyToil.AddFinishAction(() => TargetBuilding.TryGetInnerInteractableThingOwner().TryDropAll(pawn.Position, pawn.Map, ThingPlaceMode.Near, null, null, true));
            emptyToil.handlingFacing = true;
            yield return emptyToil;
            yield break;
        }
    }

    public class JobDriver_ExtractSampleFromDesignator : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed) => pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);

        private const int extractDurationTicks = 300;
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOn(() => Map.designationManager.DesignationOn(TargetThingA, USH_DefOf.USH_ExtractDesignation) == null);
            Thing thing = job.targetA.Thing;
            CompDiseaseSampleContainer sampleContainer = thing.TryGetComp<CompDiseaseSampleContainer>();
            this.FailOn(() => sampleContainer == null);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.Wait(extractDurationTicks, TargetIndex.None).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch).FailOnDestroyedOrNull(TargetIndex.A).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            Toil finalize = new Toil
            {
                initAction = delegate ()
                {
                    sampleContainer.parent.TryGetInnerInteractableThingOwner().TryDropAll(pawn.Position, pawn.Map, ThingPlaceMode.Near, null, null, true);
                    Designation designation = Map.designationManager.DesignationOn(thing, USH_DefOf.USH_ExtractDesignation);
                    designation?.Delete();
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return finalize;
            yield break;
        }
    }
}