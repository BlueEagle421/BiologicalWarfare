using System.Linq;
using System.Text;
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
        private const int RECACHE_TICKS = 1000;
        private int _cachedTick;
        private HediffInfo _cashedHediffInfo;
        private CombatDiseaseDef _combatDiseaseDef;
        public CombatDiseaseDef CombatDiseaseDef { get => _combatDiseaseDef; set => _combatDiseaseDef = value; }
        public HediffCompProperties_VirusExtraction PropsVirusExtraction => (HediffCompProperties_VirusExtraction)props;

        public override string CompDescriptionExtra
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder(base.CompDescriptionExtra);

                stringBuilder.AppendInNewLine('\n' + "USH_VirusReplicationRelated".Translate(_cashedHediffInfo.Label));

                stringBuilder.AppendInNewLine("USH_VirusReplicationCount".Translate(SpawnCount()).Colorize(parent.def.defaultLabelColor));

                return stringBuilder.ToString().TrimEnd();
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Deep.Look(ref _cashedHediffInfo, "USH_CashedHediffInfo");
            Scribe_Defs.Look(ref _combatDiseaseDef, "USH_CombatDiseaseDef");
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            CashTick();
        }

        private void CashTick()
        {
            _cachedTick++;

            if (_cachedTick >= RECACHE_TICKS)
                RecacheHediffInfo();
        }

        public void RecacheHediffInfo()
        {
            Hediff toCashe = RelatedHediff();

            if (toCashe != null)
                _cashedHediffInfo = new HediffInfo(toCashe.Severity, toCashe.def.label);

            _cachedTick = 0;
        }

        public override void Notify_PawnKilled()
        {
            base.Notify_PawnKilled();

            BiologicalUtils.SpawnThingAt(Pawn.Map, Pawn.CellsAdjacent8WayAndInside().ToList(), _combatDiseaseDef.pathogenDef, SpawnCount());
        }

        private int SpawnCount()
        {
            int baseCount = PropsVirusExtraction.basePathogenCount;

            float sizeMultiplier = PropsVirusExtraction.multiplyByBodySize ? Pawn.BodySize : 1f;

            return (int)(baseCount * sizeMultiplier * SeverityMultiplier());
        }

        private float SeverityMultiplier()
        {
            float result = 0f;

            if (_cashedHediffInfo != null && PropsVirusExtraction.multiplyBySeverity)
                result = _cashedHediffInfo.Severity;

            return result;
        }

        private Hediff RelatedHediff() => Pawn.health?.hediffSet?.GetFirstHediffOfDef(_combatDiseaseDef.giveHediffDef);

        private class HediffInfo : IExposable
        {
            public float Severity;
            public string Label;

            public HediffInfo() { }

            public HediffInfo(float severity, string label)
            {
                Severity = severity;
                Label = label;
            }

            public void ExposeData()
            {
                Scribe_Values.Look(ref Severity, "USH_Severity");
                Scribe_Values.Look(ref Label, "USH_Label");
            }
        }
    }
}