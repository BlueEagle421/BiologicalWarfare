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
        public CombatDiseaseDef CombatDiseaseDef { get; set; }
        public HediffCompProperties_VirusExtraction Props => (HediffCompProperties_VirusExtraction)props;

        public override void Notify_PawnKilled()
        {
            base.Notify_PawnKilled();

            BiologicalUtils.SpawnThingAt(Pawn.Map, Pawn.CellsAdjacent8WayAndInside().ToList(), CombatDiseaseDef.pathogenDef, SpawnCount());
        }

        private int SpawnCount() => (int)(Props.basePathogenCount * Pawn.BodySize);
    }
}