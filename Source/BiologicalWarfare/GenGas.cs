// Copyright Karel Kroeze, 2020-2020.
// modified from VanillaPowerExpanded/VanillaPowerExpanded/GenGas.cs

using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace BiologicalWarfare
{
    public static class GenGas
    {
        public const float DEFAULT_GAS_RADIUS = 5.4f;
        public static Queue<IntVec3> _queue = new Queue<IntVec3>();
        public static HashSet<IntVec3> _cells = new HashSet<IntVec3>();
        public static HashSet<IntVec3> _checked = new HashSet<IntVec3>();

        public static List<IntVec3> GetGasVentArea(IntVec3 ventPos, Map map, float radius)
        {
            var cellCount = GenRadial.NumCellsInRadius(radius);
            var room = ventPos.GetRoom(map);

            _queue.Clear();
            _cells.Clear();
            _checked.Clear();

            _queue.Enqueue(ventPos);
            _checked.Add(ventPos);
            while (_queue.Count > 0 && _cells.Count < cellCount)
            {
                var cell = _queue.Dequeue();
                _cells.Add(cell);

                foreach (var adj in GenAdjFast.AdjacentCellsCardinal(cell)
                                              .Where(adj => !adj.Impassable(map)
                                                         && !_checked.Contains(adj)
                                                         && adj.GetRoom(map) == room))
                {
                    _queue.Enqueue(adj);
                    _checked.Add(adj);
                }
            }

            return _cells.ToList();
        }

        public static IntVec3 VentingPosition(ThingWithComps parent) => VentingPosition(parent.PositionHeld, parent.Rotation);
        public static IntVec3 VentingPosition(IntVec3 pos, Rot4 rot) => pos + IntVec3.North.RotatedBy(rot);

        public static float AddGas(IntVec3 pos, Map map, ThingDef gasDef, float amount = -1, bool spread = true)
        {
            var startingAmount = amount;
            var gas = pos.GetFirstThing(map, gasDef);
            if (gas == null)
            {
                gas = ThingMaker.MakeThing(gasDef) as Gas;
                GenSpawn.Spawn(gas, pos, map);
            }

            if (amount < 0)
            {
                Log.Error(
                    $"Cannot add {amount} to {gas} at {gas.Position}. AddGas on a spreading gas should have a positive amount of gas.");
                gas.Destroy();
                return amount;
            }

            if (spread)
                AddGas_FloodFill(pos, map, gasDef);
            else
                AddGas_Cell(pos, map, gasDef);


            return startingAmount - amount;
        }

        private static void AddGas_FloodFill(IntVec3 pos, Map map, ThingDef gasDef)
        {
            var room = pos.GetRoom(map);
            _queue.Clear();
            _checked.Clear();

            _queue.Enqueue(pos);
            _checked.Add(pos);
            while (_queue.Count > 0)
            {
                var cell = _queue.Dequeue();
                AddGas_Cell(cell, map, gasDef);
                foreach (var adj in GenAdjFast.AdjacentCellsCardinal(cell)
                                              .Where(adj => !_checked.Contains(adj)
                                                         && !adj.Impassable(map)
                                                         && adj.GetRoom(map) == room))
                {
                    _queue.Enqueue(adj);
                    _checked.Add(adj);
                }
            }
        }

        private static void AddGas_Cell(IntVec3 cell, Map map, ThingDef gasDef)
        {
            var spreadingGas = cell.GetFirstThing(map, gasDef);
            if (spreadingGas == null)
            {
                spreadingGas = ThingMaker.MakeThing(gasDef);
                GenSpawn.Spawn(spreadingGas, cell, map);
            }
        }
    }
}