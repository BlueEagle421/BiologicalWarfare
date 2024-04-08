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
        private const int installDurationTicks = 300;

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
            Toil insertToil = Toils_General.Wait(installDurationTicks, TargetIndex.B);
            insertToil.WithProgressBarToilDelay(TargetIndex.B, false, -0.5f);
            insertToil.FailOn(() => IsFilled);
            insertToil.FailOnDespawnedNullOrForbidden(TargetIndex.B);
            insertToil.FailOnCannotTouch(TargetIndex.B, PathEndMode.Touch);
            insertToil.handlingFacing = true;
            yield return insertToil;
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

        private const int extractionDurationTicks = 300;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, ContainerPathEndMode).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOn(() => !IsFilled).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            Toil extractionToil = Toils_General.Wait(extractionDurationTicks, TargetIndex.A);
            extractionToil.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            extractionToil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            extractionToil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            extractionToil.AddFinishAction(() => TargetBuilding.TryGetInnerInteractableThingOwner().TryDropAll(pawn.Position, pawn.Map, ThingPlaceMode.Near, null, null, true));
            extractionToil.handlingFacing = true;
            yield return extractionToil;
            yield break;
        }
    }
}