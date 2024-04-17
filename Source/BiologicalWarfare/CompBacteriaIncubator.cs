using RimWorld;
using System.Linq;
using System.Text;
using Verse;

namespace BiologicalWarfare
{
    public class CompProperties_BacteriaIncubator : CompProperties_Interactable
    {
        public int pathogensPerFuel;
        public int incubationTicks;
        public CompProperties_BacteriaIncubator() => compClass = typeof(CompBacteriaIncubator);
    }

    public class CompBateriaIncubatorContainer : CompDiseaseSampleContainer
    {
        private CompBacteriaIncubator _compBactriaIncubator;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            _compBactriaIncubator = parent.GetComp<CompBacteriaIncubator>();
        }

        public override AcceptanceReport CanExtract(Pawn pawn)
        {
            AcceptanceReport baseResult = base.CanExtract(pawn);

            if (!baseResult.Accepted)
                return baseResult;

            if (_compBactriaIncubator.IsIncubating)
                return "USH_IncubationInProgress".Translate();

            return baseResult;
        }
    }

    public class CompBacteriaIncubator : CompInteractable
    {
        private CompDiseaseSampleContainer _sampleContainer;
        private CompRefuelable _compRefuelable;
        private CompPowerTrader _compPowerTrader;
        private int _incubationTicks;
        private int _contaminationTicks;
        private int _patogensToProduce;
        private bool _isIncubating;
        public bool IsIncubating => _isIncubating;
        public CompProperties_BacteriaIncubator PropsBacteriaIncubator => (CompProperties_BacteriaIncubator)props;

        protected override bool TryInteractTick() => true;

        public override void CompTick()
        {
            base.CompTick();

            IncubateTick();
        }

        private void IncubateTick()
        {
            if (!_isIncubating)
                return;

            if (!CanIncubate())
                return;

            ContamidationTick();

            _incubationTicks++;

            if (_incubationTicks >= PropsBacteriaIncubator.incubationTicks)
                IncubationEnded();
        }

        private void ContamidationTick()
        {
            _contaminationTicks++;

            if (_contaminationTicks < BiologicalUtils.CONTAMINATION_TICKS)
                return;

            _contaminationTicks = 0;

            BiologicalUtils.TryToContaminate(parent, _sampleContainer.ContainedSampleComp().PropsDiseaseSample.combatDiseaseDef);
        }

        private void IncubationEnded()
        {
            ThingDef toSpawn = _sampleContainer.ContainedSampleComp().PropsDiseaseSample.combatDiseaseDef.pathogenDef;
            BiologicalUtils.SpawnThingAt(parent.Map, parent.CellsAdjacent8WayAndInside().ToList(), toSpawn, _patogensToProduce);

            _sampleContainer.innerContainer.ClearAndDestroyContents();
            _incubationTicks = 0;
            _patogensToProduce = 0;
            _isIncubating = false;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            _sampleContainer = parent.GetComp<CompDiseaseSampleContainer>();
            _compRefuelable = parent.GetComp<CompRefuelable>();
            _compPowerTrader = parent.GetComp<CompPowerTrader>();
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            _incubationTicks = 0;
            _contaminationTicks = 0;
            _patogensToProduce = 0;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref _incubationTicks, "USH_IncubationTicks", 0);
            Scribe_Values.Look(ref _contaminationTicks, "USH_ContaminationTicks", 0);
            Scribe_Values.Look(ref _patogensToProduce, "USH_PatogensToProduce", 0);
            Scribe_Values.Look(ref _isIncubating, "USH_IsIncubating", false);
        }

        public override string CompInspectStringExtra()
        {
            string contaminationChance = USHDefOf.USH_ContaminationChanceFactor.LabelCap + ": " + parent.GetStatValue(USHDefOf.USH_ContaminationChanceFactor).ToStringPercent();

            if (!_isIncubating)
                return base.CompInspectStringExtra() + "\n" + contaminationChance;

            StringBuilder stringBuilder = new StringBuilder();

            string timeLeft = GenDate.ToStringTicksToPeriod(PropsBacteriaIncubator.incubationTicks - _incubationTicks);
            stringBuilder.AppendLine("USH_IncubatorTimeLeft".Translate(timeLeft));

            string diseaseLabel = _sampleContainer.ContainedSampleComp().PropsDiseaseSample.combatDiseaseDef.label;
            stringBuilder.AppendLine("USH_IncubatorWillProduce".Translate(_patogensToProduce, diseaseLabel));

            stringBuilder.AppendLine(contaminationChance);

            return stringBuilder.ToString().Trim();
        }

        protected override void OnInteracted(Pawn caster)
        {
            if (_isIncubating)
                return;

            base.OnInteracted(caster);

            BiologicalUtils.TryToContaminate(parent, _sampleContainer.ContainedSampleComp().PropsDiseaseSample.combatDiseaseDef);

            _patogensToProduce = PathogensCount();
            _isIncubating = true;
            _compRefuelable.ConsumeFuel(_compRefuelable.Fuel);
        }

        private int PathogensCount() => (int)(_compRefuelable.Fuel * PropsBacteriaIncubator.pathogensPerFuel);

        private AcceptanceReport CanIncubate()
        {
            if (!_compPowerTrader.PowerOn)
                return "NoPower".Translate();

            return true;
        }



        public override AcceptanceReport CanInteract(Pawn activateBy = null, bool checkOptionalItems = true)
        {
            AcceptanceReport baseResult = base.CanInteract(activateBy, checkOptionalItems);

            if (_isIncubating)
                return "AlreadyActive".Translate();

            if (!baseResult)
                return baseResult;

            if (_sampleContainer.Empty)
            {
                NamedArgument typeArgument = _sampleContainer.PropsSampleContainer.acceptableDiseaseType.ToStringUncapitalized().Named("TYPE");
                return "USH_NoSample".Translate(typeArgument);
            }

            return baseResult;
        }
    }
}
