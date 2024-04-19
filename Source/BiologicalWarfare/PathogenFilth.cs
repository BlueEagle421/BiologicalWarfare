using OPToxic;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace BiologicalWarfare
{
    public class PathogenFilth : LiquidFuel
    {
        public override void Tick()
        {
            base.Tick();

            InfectionTick();
        }

        private void InfectionTick()
        {
            if (Destroyed)
                return;

            if (!this.IsHashIntervalTick(OPToxicDefGetValue.OPToxicGetSevUpVal(def)))
                return;

            List<Thing> thingsInFilth = Position.GetThingList(Map);

            foreach (Thing thingInFilth in thingsInFilth)
                if (thingInFilth is Pawn pawnInFilth)
                    BiologicalUtils.DoPathogenInfection(this, pawnInFilth);
        }
    }
}
