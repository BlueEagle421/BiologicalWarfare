using OPToxic;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
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

            //if (Find.TickManager.TicksGame % OPToxicDefGetValue.OPToxicGetSevUpVal(def) != 0)
            //    return;

            List<Thing> thingsInGas = Position.GetThingList(Map);

            if (thingsInGas.Count <= 0)
                return;

            for (int i = 0; i < thingsInGas.Count; i++)
                if (thingsInGas[i] is Pawn pawn)
                    Infect(this, pawn);
        }

        public void Infect(Thing gas, Pawn pawn)
        {
            if (!BiologicalUtils.CanPathogenInfect(pawn))
                return;

            HediffDef hediffToAdd = DefDatabase<HediffDef>.GetNamedSilentFail(OPToxicDefGetValue.OPToxicGetHediff(gas.def));

            if (hediffToAdd == null)
                return;

            HediffSet hediffSet = pawn.health.hediffSet;
            Hediff hediffFound = (hediffSet?.GetFirstHediffOfDef(hediffToAdd, false));

            float statValue = 1 - pawn.GetStatValue(StatDefOf.ToxicResistance, true);
            float severityToSet = Mathf.Max(OPToxicDefGetValue.OPToxicGetSev(gas.def), hediffToAdd.minSeverity);

            severityToSet = Rand.Range(hediffToAdd.minSeverity * statValue, severityToSet * statValue);

            if (hediffFound != null && severityToSet > 0f)
            {
                hediffFound.Severity += severityToSet;
                return;
            }

            Hediff hediffMade = HediffMaker.MakeHediff(hediffToAdd, pawn);
            hediffMade.Severity = severityToSet;
            pawn.health.AddHediff(hediffMade);
        }
    }
}