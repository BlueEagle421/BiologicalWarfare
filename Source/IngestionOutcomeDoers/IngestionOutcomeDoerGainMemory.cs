using RimWorld;
using Verse;

namespace BiologicalWarfare
{
    public class IngestionOutcomeGainMemory : IngestionOutcomeDoer
    {
        public ThoughtDef thoughtDef;
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount) => GainMemory(pawn);
        private void GainMemory(Pawn pawn) => pawn.needs.mood.thoughts.memories.TryGainMemory(thoughtDef);
    }
}
