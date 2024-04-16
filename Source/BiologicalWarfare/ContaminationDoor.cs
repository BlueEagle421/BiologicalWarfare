using RimWorld;
using Verse;

namespace BiologicalWarfare
{
    public class Building_ContaminationDoor : Building_Door
    {
        private CompPowerTrader _compPowerTrader;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            _compPowerTrader = GetComp<CompPowerTrader>();
        }

        public override bool PawnCanOpen(Pawn p)
        {
            if (!p.IsPlayerControlled)
                base.PawnCanOpen(p);

            if (_compPowerTrader.PowerOn && BiologicalUtils.CanPathogenInfect(p))
                return false;

            return base.PawnCanOpen(p);
        }
    }
}
