using OPToxic;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
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
                    Infect(this, pawn);
        }

        public void Infect(Thing filth, Pawn pawn)
        {
            if (!BiologicalUtils.CanPathogenInfect(pawn))
                return;

            HediffDef hediffToAdd = DefDatabase<HediffDef>.GetNamedSilentFail(OPToxicDefGetValue.OPToxicGetHediff(filth.def));

            if (hediffToAdd == null)
                return;

            HediffSet hediffSet = pawn.health.hediffSet;
            Hediff hediffFound = (hediffSet?.GetFirstHediffOfDef(hediffToAdd, false));

            float statValue = 1 - pawn.GetStatValue(StatDefOf.ToxicResistance, true);
            float newSeverity = Mathf.Max(hediffToAdd.minSeverity, OPToxicDefGetValue.OPToxicGetSev(filth.def));

            newSeverity = Rand.Range(newSeverity / 2f, newSeverity) * statValue;

            if (hediffFound != null && newSeverity > 0f)
            {
                hediffFound.Severity += newSeverity;
                return;
            }

            Hediff hediffMade = HediffMaker.MakeHediff(hediffToAdd, pawn);
            hediffMade.Severity = newSeverity;
            pawn.health.AddHediff(hediffMade);
        }
    }
}
