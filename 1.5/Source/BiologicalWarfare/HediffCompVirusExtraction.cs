using System.Linq;
using Verse;

namespace BiologicalWarfare
{
    public class HediffCompProperties_VirusExtraction : HediffCompProperties
    {
        public int basePathogenCount;
        public bool multiplyByBodySize;
        public bool multiplyBySeverity;
        public HediffCompProperties_VirusExtraction() => compClass = typeof(HediffCompVirusExtraction);
    }
    public class HediffCompVirusExtraction : HediffComp
    {
        private CombatDiseaseDef _combatDiseaseDef;
        public CombatDiseaseDef CombatDiseaseDef { get => _combatDiseaseDef; set => _combatDiseaseDef = value; }
        public HediffCompProperties_VirusExtraction PropsVirusExtraction => (HediffCompProperties_VirusExtraction)props;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Defs.Look(ref _combatDiseaseDef, "USH_CombatDiseaseDef");
        }

        public override void Notify_PawnKilled()
        {
            base.Notify_PawnKilled();

            BiologicalUtils.SpawnThingAt(Pawn.Map, Pawn.CellsAdjacent8WayAndInside().ToList(), _combatDiseaseDef.pathogenDef, SpawnCount());
        }

        public override string CompDescriptionExtra => base.CompDescriptionExtra + "\n" + "USH_VirusReplicationCount".Translate(SpawnCount()).Colorize(parent.def.defaultLabelColor);

        private int SpawnCount()
        {
            int baseCount = PropsVirusExtraction.basePathogenCount;

            float sizeMultiplier = PropsVirusExtraction.multiplyByBodySize ? Pawn.BodySize : 1f;

            return (int)(baseCount * sizeMultiplier * SeverityMultiplier());
        }

        private float SeverityMultiplier()
        {
            float result = 0f;

            Hediff diseaseHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(_combatDiseaseDef.giveHediffDef);

            if (diseaseHediff != null && PropsVirusExtraction.multiplyBySeverity)
                result = diseaseHediff.Severity;

            return result;
        }
    }
}