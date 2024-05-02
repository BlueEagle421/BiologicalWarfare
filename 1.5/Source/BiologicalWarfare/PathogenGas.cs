using OPToxic;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace BiologicalWarfare
{
    public class PathogenGas : Gas
    {
        private int _infectedTimes = 0;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _infectedTimes, "USH_InfectedTimes", 0);
        }

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

            List<Thing> thingsInGas = Position.GetThingList(Map);

            for (int i = 0; i < thingsInGas.Count; i++)
            {
                if (!(thingsInGas[i] is Pawn pawnInGas))
                    continue;

                if (BiologicalUtils.AddInfectionSeverity(pawnInGas, this) > 0)
                    _infectedTimes++;

                int maxCount = BiologicalWarfareMod.Settings.MaxGasInfectionCount.Value;

                if (maxCount != -1 && _infectedTimes >= maxCount)
                {
                    Destroy();
                    break;
                }
            }
        }
    }
}