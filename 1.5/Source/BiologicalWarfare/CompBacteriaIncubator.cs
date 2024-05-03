using RimWorld;
using System.Linq;
using System.Text;
using UnityEngine;
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
        private int _patogensToProduce;
        private bool _isIncubating;

        private const int SMOKE_INTERVAL = 8;

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

            _incubationTicks++;

            if (_incubationTicks % SMOKE_INTERVAL == 0)
                ThrowSmoke(parent.DrawPos, parent.Map, 1f, LidColor());

            if (_incubationTicks >= PropsBacteriaIncubator.incubationTicks)
                IncubationEnded(true);
        }

        private void IncubationEnded(bool produceResult)
        {
            if (produceResult)
            {
                ThingDef toSpawn = _sampleContainer.ContainedSampleComp().PropsDiseaseSample.combatDiseaseDef.pathogenDef;
                BiologicalUtils.SpawnThingAt(parent.Map, parent.CellsAdjacent8WayAndInside().ToList(), toSpawn, _patogensToProduce);
            }

            _sampleContainer.innerContainer.ClearAndDestroyContents();
            _incubationTicks = 0;
            _patogensToProduce = 0;
            _isIncubating = false;
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            IncubationEnded(false);
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
            if (!parent.Spawned)
                return base.CompInspectStringExtra();

            if (!_isIncubating)
                return base.CompInspectStringExtra();

            StringBuilder stringBuilder = new StringBuilder();

            string timeLeft = GenDate.ToStringTicksToPeriod(PropsBacteriaIncubator.incubationTicks - _incubationTicks);
            stringBuilder.AppendLine("USH_IncubatorTimeLeft".Translate(timeLeft));

            string diseaseLabel = _sampleContainer.ContainedSampleComp().PropsDiseaseSample.combatDiseaseDef.label;
            stringBuilder.AppendLine("USH_IncubatorWillProduce".Translate(_patogensToProduce, diseaseLabel));

            return stringBuilder.ToString().Trim();
        }

        protected override void OnInteracted(Pawn caster)
        {
            if (_isIncubating)
                return;

            base.OnInteracted(caster);

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

        public override void PostDraw()
        {
            base.PostDraw();

            if (parent.Rotation != Rot4.South)
                return;

            if (!_isIncubating)
                return;

            if (!CanIncubate().Accepted)
                return;

            Vector3 position = parent.DrawPos + new Vector3(0, 1, -0.10f);
            Vector3 scale = new Vector3(0.4f, 1f, 0.22f);

            string path = "Things/Building/BacteriaIncubator/BacteriaIncubatorLid";
            Material mat = MaterialPool.MatFrom(path);
            mat.shader = ShaderDatabase.SolidColor;
            mat.SetColor("_Color", LidColor());

            Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(position, Quaternion.identity, scale), mat, 0);
        }

        private Color LidColor()
        {
            if (_sampleContainer.Empty)
                return Color.clear;

            float pulseSpeed = 40f;
            float alphaMultiplier = 0.6f;


            Color result = _sampleContainer.ContainedSampleComp().PropsDiseaseSample.combatDiseaseDef.colorInt.ToColor;
            result = result.ToTransparent(Mathf.Abs(Mathf.Sin(Find.TickManager.TicksGame / pulseSpeed)) * alphaMultiplier);

            return result;
        }

        public static void ThrowSmoke(Vector3 loc, Map map, float size, Color color)
        {
            if (loc.ShouldSpawnMotesAt(map))
            {
                FleckCreationData dataStatic = FleckMaker.GetDataStatic(loc, map, FleckDefOf.Smoke, Rand.Range(1.5f, 2.5f) * size);
                dataStatic.rotationRate = Rand.Range(-30f, 30f);
                dataStatic.velocityAngle = Rand.Range(30, 40);
                dataStatic.velocitySpeed = Rand.Range(0.5f, 0.7f);
                dataStatic.airTimeLeft = 6;
                dataStatic.solidTimeOverride = 2;
                dataStatic.instanceColor = color;
                map.flecks.CreateFleck(dataStatic);
            }
        }

    }
}
