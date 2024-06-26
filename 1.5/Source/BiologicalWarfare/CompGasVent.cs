using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace BiologicalWarfare
{
    public class PlaceWorker_GasVent : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 pos, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            var ventingPos = pos + IntVec3.North.RotatedBy(rot);

            if (ventingPos.Impassable(map))
                return false;

            return true;
        }

        public override void DrawGhost(ThingDef def, IntVec3 pos, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            var ventingPos = GasVentUtils.VentingPosition(pos, rot);

            GenDraw.DrawFieldEdges(new List<IntVec3> { ventingPos }, Color.white);

            var map = Find.CurrentMap;
            var affectedArea = GasVentUtils.GetGasVentArea(ventingPos, map, def.GetCompProperties<CompProperties_GasVent>()?.ventingRadius ?? 0);
            if (affectedArea.NullOrEmpty())
                return;

            GenDraw.DrawFieldEdges(affectedArea, Color.white);
        }
    }

    public class CompProperties_GasVent : CompProperties_Interactable
    {
        public ThingDef gasDef;
        public float pathogensPerCell;
        public float ventingRadius = GasVentUtils.DEFAULT_GAS_RADIUS;

        public CompProperties_GasVent() => compClass = typeof(CompGasVent);
    }

    public class CompGasVent : CompInteractable
    {
        private CompRefuelable _compRefuelable;
        protected IntVec3 _ventPos;

        private List<GasSpreadTask> _gasSpreadTasks = new List<GasSpreadTask>();

        private const int GAS_CELL_DELAY = 2;
        private const int SHUFFLE_STEPS = 12;

        private const int GOODWILL_CHANGE = -12;

        public CompProperties_GasVent PropsVent => props as CompProperties_GasVent;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (_gasSpreadTasks == null)
                _gasSpreadTasks = new List<GasSpreadTask>();

            _compRefuelable = parent.GetComp<CompRefuelable>();
            _ventPos = GasVentUtils.VentingPosition(parent);
        }

        public override void CompTick()
        {
            base.CompTick();

            _gasSpreadTasks.ForEach(task => task.TickPassed());
            _gasSpreadTasks.RemoveAll(task => task.IsFinished);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Collections.Look(ref _gasSpreadTasks, "USH_GasSpreadTasks", LookMode.Deep);
        }

        public override string CompInspectStringExtra()
        {
            return base.CompInspectStringExtra() + "\n" + "USH_GasVentConsumtion".Translate(PropsVent.pathogensPerCell.ToString());
        }

        public override AcceptanceReport CanInteract(Pawn activateBy = null, bool checkOptionalItems = true)
        {
            if (activateBy != null && activateBy.WorkTagIsDisabled(WorkTags.Violent))
                return "IsIncapableOfViolence".Translate(activateBy.LabelShort, activateBy);

            return base.CanInteract(activateBy, checkOptionalItems);
        }

        public AcceptanceReport CanInteractRemotely(Pawn activateBy = null, bool checkOptionalItems = true)
        {
            AcceptanceReport baseResult = base.CanInteract(activateBy, checkOptionalItems);

            //this check can be safely skipped because it's the last one
            if (baseResult.Reason == "CannotReach".Translate())
                return true;

            return baseResult;
        }

        protected override void OnInteracted(Pawn caster)
        {
            base.OnInteracted(caster);

            FloodAreaWithGas(caster);
        }

        private void FloodAreaWithGas(Pawn caster)
        {
            Map map = parent.Map;
            Room room = parent.GetRoom();

            List<IntVec3> cellsToFlood = GasVentUtils.GetGasVentArea(_ventPos, map, PropsVent?.ventingRadius ?? 0);

            cellsToFlood.OrderBy(x => x.DistanceTo(parent.Position)).ToList();

            cellsToFlood.Shuffle(SHUFFLE_STEPS);

            for (int i = 0; i < cellsToFlood.Count; i++)
                _gasSpreadTasks.Add(new GasSpreadTask(cellsToFlood[i], caster, parent, i * GAS_CELL_DELAY));
        }

        public void MakeGasAt(Pawn caster, IntVec3 cell)
        {
            Map map = parent.Map;
            Room room = parent.GetRoom();

            if (_compRefuelable.Fuel < PropsVent.pathogensPerCell)
            {
                _compRefuelable.ConsumeFuel(_compRefuelable.Fuel);
                return;
            }

            if (!cell.Walkable(map))
                return;

            if (room != null && cell.GetRoom(map) != room)
                return;

            Thing madeGas = ThingMaker.MakeThing(PropsVent.gasDef);

            if (GenPlace.TryPlaceThing(madeGas, cell, parent.Map, ThingPlaceMode.Near))
                DamageGoodwill(caster, cell);

            _compRefuelable.ConsumeFuel(PropsVent.pathogensPerCell);
        }

        private void DamageGoodwill(Pawn caster, IntVec3 cell)
        {
            Pawn pawnInGas = cell.GetFirstPawn(parent.Map);

            if (caster?.HomeFaction == null || pawnInGas?.HomeFaction == null)
                return;

            caster.HomeFaction.TryAffectGoodwillWith(
                pawnInGas.HomeFaction,
                GOODWILL_CHANGE,
                true,
                true,
                HistoryEventDefOf.UsedHarmfulItem,
                pawnInGas);
        }

        private class GasSpreadTask : IExposable
        {
            private IntVec3 _cell;
            private Pawn _caster;
            private ThingWithComps _buildingVent;
            private int _ticksDelay;
            private bool _isFinished;
            public bool IsFinished { get { return _isFinished; } }

            public GasSpreadTask() { } //empty constructor for IExposable
            public GasSpreadTask(IntVec3 arg, Pawn caster, ThingWithComps buildingVent, int ticksDelay)
            {
                _cell = arg;
                _caster = caster;
                _buildingVent = buildingVent;
                _ticksDelay = ticksDelay;
            }
            public void ExposeData()
            {
                Scribe_Values.Look(ref _cell, "USH_Cell");
                Scribe_References.Look(ref _caster, "USH_Caster");
                Scribe_References.Look(ref _buildingVent, "USH_BuildingVent");
                Scribe_Values.Look(ref _ticksDelay, "USH_TicksDelay");
                Scribe_Values.Look(ref _isFinished, "USH_IsFinished");
            }

            public void TickPassed()
            {
                if (_isFinished)
                    return;

                _ticksDelay--;

                if (_ticksDelay <= 0)
                {
                    _buildingVent.TryGetComp<CompGasVent>().MakeGasAt(_caster, _cell);
                    _isFinished = true;
                }
            }
        }
    }
}
