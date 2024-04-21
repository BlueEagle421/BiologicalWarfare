using RimWorld;
using Verse;

namespace BiologicalWarfare
{
    public class IngestionOutcomeDoer_RemoveHediff : IngestionOutcomeDoer
    {
        public HediffDef hediffDefToRemove;
        public bool sendMessage;
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount) => RemoveHediffFrom(pawn, ingested);

        private void RemoveHediffFrom(Pawn pawn, Thing ingested)
        {
            Hediff toRemove = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDefToRemove);

            if (toRemove == null)
                return;

            pawn.health.hediffSet.hediffs.Remove(toRemove);

            if (sendMessage)
            {
                string content = (string)"USH_ThingHealed"
                    .Translate(ingested.Named("THING"), toRemove.Named("HEDIFF"), pawn.Named("PAWN"));
                Messages.Message(content, pawn, MessageTypeDefOf.PositiveEvent, true);
            }
        }
    }
}
