using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace BiologicalWarfare
{
    public class IngestionOutcomeDoer_RemoveHediffs : IngestionOutcomeDoer
    {
        public List<HediffDef> hediffDefsToRemove;
        public bool sendMessage;
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount) => RemoveHediffsFrom(pawn, ingested);

        private void RemoveHediffsFrom(Pawn pawn, Thing ingested)
        {
            List<Hediff> removedHediffs = new List<Hediff>();

            foreach (HediffDef defToRemove in hediffDefsToRemove)
            {
                Hediff toRemove = pawn.health.hediffSet.GetFirstHediffOfDef(defToRemove);

                if (toRemove == null)
                    continue;

                if (!pawn.health.hediffSet.hediffs.Remove(toRemove))
                    continue;

                removedHediffs.Add(toRemove);
            }

            SendRemovedMessage(pawn, ingested, removedHediffs);
        }

        private void SendRemovedMessage(Pawn pawn, Thing ingested, List<Hediff> removed)
        {
            if (!sendMessage)
                return;

            string thingArg = ingested.def.label.UncapitalizeFirst();
            string hediffsArg = string.Join(", ", removed.Select(x => x.Label.UncapitalizeFirst()));
            string pawnArg = pawn.LabelShort;

            string content = (string)"USH_ThingHealed".Translate(thingArg.Named("THING"), hediffsArg.Named("HEDIFF"), pawnArg.Named("PAWN"));

            Messages.Message(content, pawn, MessageTypeDefOf.PositiveEvent);
        }
    }
}
