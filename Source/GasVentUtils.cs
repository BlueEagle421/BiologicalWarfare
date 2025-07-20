using System.Collections.Generic;
using System.Linq;
using Verse;

namespace USH_BW
{
    public static class GasVentUtils
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
    }
}