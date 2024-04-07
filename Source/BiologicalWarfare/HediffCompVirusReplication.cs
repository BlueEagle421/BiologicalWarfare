using System.Linq;
using Verse;

namespace BiologicalWarfare
{
    public class HediffCompProperties_VirusReplication : HediffCompProperties
    {
        public int basePathogenCount;
        public bool multiplyByBodySize;
        public HediffCompProperties_VirusReplication() => compClass = typeof(HediffCompVirusReplication);
    }
    public class HediffCompVirusReplication : HediffComp
    {
        private CombatDiseaseDef _combatDiseaseDef;
        public CombatDiseaseDef CombatDiseaseDef { get => _combatDiseaseDef; set => _combatDiseaseDef = value; }
        public HediffCompProperties_VirusReplication Props => (HediffCompProperties_VirusReplication)props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
        }

        public override void Notify_PawnDied()
        {
            base.Notify_PawnDied();

            BiologicalUtils.SpawnThingAt(Pawn.Map, Pawn.CellsAdjacent8WayAndInside().ToList(), CombatDiseaseDef.pathogenDef, SpawnCount());
            RemoveItself();
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