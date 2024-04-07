using System.Linq;
using Verse;

namespace BiologicalWarfare
{
    public class HediffCompProperties_VirusExtraction : HediffCompProperties
    {
        public int basePathogenCount;
        public bool multiplyByBodySize;
        public HediffCompProperties_VirusExtraction() => compClass = typeof(HediffCompVirusExtraction);
    }
    public class HediffCompVirusExtraction : HediffComp
    {
        private CombatDiseaseDef _combatDiseaseDef;
        public CombatDiseaseDef CombatDiseaseDef { get => _combatDiseaseDef; set => _combatDiseaseDef = value; }
        public HediffCompProperties_VirusExtraction Props => (HediffCompProperties_VirusExtraction)props;

        public override void Notify_PawnDied()
        {
            base.Notify_PawnDied();

            RemoveItself();
        }

        public override void Notify_PawnKilled()
        {
            base.Notify_PawnKilled();

            BiologicalUtils.SpawnThingAt(Pawn.Map, Pawn.CellsAdjacent8WayAndInside().ToList(), CombatDiseaseDef.pathogenDef, SpawnCount());
        }

        private int SpawnCount() => (int)(Props.basePathogenCount * Pawn.BodySize);

        private void RemoveItself()
        {
            Hediff firstHediffOfDef = Pawn.health.hediffSet.GetFirstHediffOfDef(parent.def, false);
            if (firstHediffOfDef == null)
                return;

            Pawn.health.RemoveHediff(firstHediffOfDef);
        }
    }
}