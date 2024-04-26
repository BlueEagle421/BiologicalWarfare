using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            var ventingPos = GenGas.VentingPosition(pos, rot);

            GenDraw.DrawFieldEdges(new List<IntVec3> { ventingPos }, Color.white);

            var map = Find.CurrentMap;
            var affectedArea = GenGas.GetGasVentArea(ventingPos, map, def.GetCompProperties<CompProperties_GasVent>()?.ventingRadius ?? 0);
            if (affectedArea.NullOrEmpty())
                return;

            GenDraw.DrawFieldEdges(affectedArea, Color.white);
        }
    }

    public class CompProperties_GasVent : CompProperties_Interactable
    {
        public ThingDef gasDef;
        public float pathogensPerCell;
        public float ventingRadius = GenGas.DEFAULT_GAS_RADIUS;

        public CompProperties_GasVent() => compClass = typeof(CompGasVent);
    }

    public class CompGasVent : CompInteractable
    {
        private CompRefuelable _compRefuelable;
        protected IntVec3 _ventPos;

        private const int GAS_CELL_DELAY = 25;
        private const int SHUFFLE_STEPS = 12;

        public CompProperties_GasVent PropsVent => props as CompProperties_GasVent;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            _compRefuelable = parent.GetComp<CompRefuelable>();
            _ventPos = GenGas.VentingPosition(parent);
        }

        protected override void OnInteracted(Pawn caster)
        {
            base.OnInteracted(caster);

            FloodAreaWithGas();
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

        private async void FloodAreaWithGas()
        {
            Map map = parent.Map;
            Room room = parent.GetRoom();

            List<IntVec3> cellsToFlood = GenGas.GetGasVentArea(_ventPos, map, PropsVent?.ventingRadius ?? 0);

            cellsToFlood.OrderBy(x => x.DistanceTo(parent.Position)).ToList();

            cellsToFlood.Shuffle(SHUFFLE_STEPS);

            foreach (IntVec3 cell in cellsToFlood)
                await MakeGasAt(cell, map, room);
        }

        private async Task MakeGasAt(IntVec3 cell, Map map, Room room)
        {
            await Task.Delay(GAS_CELL_DELAY);

            while (Find.TickManager.CurTimeSpeed == 0f)
                await Task.Delay(0);

            if (_compRefuelable.Fuel < PropsVent.pathogensPerCell)
            {
                _compRefuelable.ConsumeFuel(_compRefuelable.Fuel);
                return;
            }

            if (!cell.Walkable(map))
                return;

            if (room != null && cell.GetRoom(map) != room)
                return;

            Thing firstThing = cell.GetFirstThing(map, PropsVent.gasDef);
            if (firstThing != null)
                return;

            Thing madeGas = ThingMaker.MakeThing(PropsVent.gasDef);

            GenPlace.TryPlaceThing(madeGas, cell, map, ThingPlaceMode.Near);
            _compRefuelable.ConsumeFuel(PropsVent.pathogensPerCell);
        }
    }
}
