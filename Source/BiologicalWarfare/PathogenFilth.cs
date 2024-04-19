using OPToxic;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace BiologicalWarfare
{
    public class PathogenFilth : LiquidFuel
    {
        public override void Tick() => InfectionTick();

        private void InfectionTick()
        {
            if (Destroyed)
                return;

            if (!this.IsHashIntervalTick(OPToxicDefGetValue.OPToxicGetSevUpVal(def)))
                return;

            List<Thing> thingsInFilth = Position.GetThingList(Map);

            if (thingsInFilth.Count <= 0)
                return;

            for (int i = 0; i < thingsInFilth.Count; i++)
                if (thingsInFilth[i] is Pawn pawn)
                    BiologicalUtils.DoPathogenInfection(this, pawn);
        }
    }
}
