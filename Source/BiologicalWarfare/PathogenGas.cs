using OPToxic;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace BiologicalWarfare
{
    public class PathogenGas : Gas
    {
        public override void Tick()
        {
            if (destroyTick <= Find.TickManager.TicksGame)
                Destroy(DestroyMode.Vanish);

            graphicRotation += graphicRotationSpeed;

            InfectionTick();
        }

        private void InfectionTick()
        {
            if (Destroyed)
                return;

            if (!this.IsHashIntervalTick(OPToxicDefGetValue.OPToxicGetSevUpVal(def)))
                return;

            List<Thing> thingsInGas = Position.GetThingList(Map);

            if (thingsInGas.Count <= 0)
                return;

            for (int i = 0; i < thingsInGas.Count; i++)
                if (thingsInGas[i] is Pawn pawn)
                    BiologicalUtils.DoPathogenInfection(this, pawn);
        }


    }
}