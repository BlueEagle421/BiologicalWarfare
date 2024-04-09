using RimWorld;
using System.Linq;
using System.Text;
using Verse;

namespace BiologicalWarfare
{
    public class CompProperties_BacteriaIncubator : CompProperties_Activable
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

    public class CompBacteriaIncubator : CompActivable
    {
        private CompDiseaseSampleContainer _sampleContainer;
        private CompRefuelable _compRefuelable;
        private CompPowerTrader _compPowerTrader;
        private int _incubationTicks;
        private int _patogensToProduce;
        private bool _isIncubating;
        public bool IsIncubating => _isIncubating;
        public CompProperties_BacteriaIncubator IncubatorProps => (CompProperties_BacteriaIncubator)props;

        protected override bool TryUse() => true;

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

            _incubationTicks++;

            if (_incubationTicks >= IncubatorProps.incubationTicks)
                IncubationEnded();
        }

        private void IncubationEnded()
        {
            ThingDef toSpawn = _sampleContainer.ContainedSampleComp().Props.combatDiseaseDef.pathogenDef;
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
            _patogensToProduce = 0;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref _incubationTicks, "USH_IncubationTicks", 0);
            Scribe_Values.Look(ref _patogensToProduce, "USH_PatogensToProduce", 0);
            Scribe_Values.Look(ref _isIncubating, "USH_IsIncubating", false);
        }

        public override string CompInspectStringExtra()
        {
            if (!_isIncubating)
                return base.CompInspectStringExtra();

            StringBuilder stringBuilder = new StringBuilder();

            string timeLeft = GenDate.ToStringTicksToPeriod(IncubatorProps.incubationTicks - _incubationTicks);
            stringBuilder.AppendLine("USH_IncubatorTimeLeft".Translate(timeLeft));

            string diseaseLabel = _sampleContainer.ContainedSampleComp().Props.combatDiseaseDef.label;
            stringBuilder.AppendLine("USH_IncubatorWillProduce".Translate(_patogensToProduce, diseaseLabel));

            return stringBuilder.ToString().TrimEnd();
        }

        public override void Activate()
        {
            if (_isIncubating)
                return;

            base.Activate();
            _patogensToProduce = PathogensCount();
            _isIncubating = true;
            _compRefuelable.ConsumeFuel(_compRefuelable.Fuel);
        }

        private int PathogensCount() => (int)(_compRefuelable.Fuel * IncubatorProps.pathogensPerFuel);

        private AcceptanceReport CanIncubate()
        {
            if (!_compPowerTrader.PowerOn)
                return "NoPower".Translate();

            return true;
        }

        public override AcceptanceReport CanActivate(Pawn activateBy = null)
        {
            AcceptanceReport baseResult = base.CanActivate(activateBy);

            if (_isIncubating)
                return "AlreadyActive".Translate();

            if (!baseResult)
                return baseResult;

            if (_sampleContainer.Empty)
            {
                NamedArgument typeArgument = _sampleContainer.ContainerProps.acceptableDiseaseType.ToStringUncapitalized().Named("TYPE");
                return "USH_NoSample".Translate(typeArgument);
            }

            return baseResult;
        }
    }
}
