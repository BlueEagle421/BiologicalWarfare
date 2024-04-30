using RimWorld;
using System.Threading.Tasks;
using Verse;

namespace BiologicalWarfare
{
    public class HediffCompNecroa : HediffComp
    {
        private Faction _postMortemFaction;
        public Faction PostMortemFaction { get { return _postMortemFaction; } set { _postMortemFaction = value; } }

        public override void CompExposeData()
        {
            base.CompExposeData();

            Scribe_Deep.Look(ref _postMortemFaction, "USH_PostMortemFaction");
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo, culprit);

            if (!parent.FullyImmune())
                TurnIntoShambler();
        }

        private async void TurnIntoShambler()
        {
            //needs a very small delay to avoid in-game errors
            //looks like the game needs to calulate rot stage before resurrecting
            await Task.Delay(10);
            MutantUtility.ResurrectAsShambler(Pawn);

            if (_postMortemFaction != null)
                Pawn.SetFactionDirect(_postMortemFaction);
        }
    }
}
