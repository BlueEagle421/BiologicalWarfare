using RimWorld;
using Verse;

namespace BiologicalWarfare
{
    public class HediffCompProperties_DevelopingScaria : HediffCompProperties
    {
        public float severityToDevelop;

        public MentalStateDef animalMentalState;
        public MentalStateDef animalMentalStateAlias;
        public MentalStateDef humanMentalState;
        public HediffCompProperties_DevelopingScaria() => compClass = typeof(HediffCompDevelopingScaria);
    }

    public class HediffCompDevelopingScaria : HediffComp
    {
        public HediffCompProperties_DevelopingScaria Props => (HediffCompProperties_DevelopingScaria)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (parent.Severity >= Props.severityToDevelop)
                DevelopScaria();
        }

        private void DevelopScaria()
        {
            Pawn.health.AddHediff(HediffDefOf.Scaria);
            Pawn.health.RemoveHediff(parent);

            TryTriggerForHumanlike(Pawn);
            TryTriggerForAnimal(Pawn);
        }

        private bool TryTriggerForHumanlike(Pawn pawn)
        {
            if (!CanTriggerForAnyKind(pawn))
                return false;

            if (!pawn.RaceProps.Humanlike)
                return false;

            if (pawn.mindState.mentalStateHandler.CurStateDef == Props.humanMentalState)
                return false;

            return pawn.mindState.mentalStateHandler
                .TryStartMentalState(Props.humanMentalState, parent.def.LabelCap, false);
        }

        private bool TryTriggerForAnimal(Pawn pawn)
        {
            if (!CanTriggerForAnyKind(pawn))
                return false;

            if (!pawn.RaceProps.Animal)
                return false;

            if (pawn.mindState.mentalStateHandler.CurStateDef == Props.animalMentalState)
                return false;

            if (Props.animalMentalStateAlias != null && pawn.mindState.mentalStateHandler.CurStateDef == Props.animalMentalStateAlias)
                return false;

            return pawn.mindState.mentalStateHandler
                .TryStartMentalState(Props.animalMentalState, parent.def.LabelCap, false);
        }

        private bool CanTriggerForAnyKind(Pawn pawn)
        {
            if (!pawn.Awake())
                return false;

            if (!pawn.Spawned)
                return false;

            return true;
        }
    }
}
