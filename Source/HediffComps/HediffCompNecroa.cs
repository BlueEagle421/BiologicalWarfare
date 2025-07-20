using RimWorld;
using System.Threading.Tasks;
using Verse;

namespace USH_BW
{
    public class HediffCompNecroa : HediffComp
    {
        private Faction _postMortemFaction;
        public Faction PostMortemFaction { get { return _postMortemFaction; } set { _postMortemFaction = value; } }

        private const int LIFESPAN_TICKS = 3 * 60000; //3 days

        public override void CompExposeData()
        {
            base.CompExposeData();

            Scribe_References.Look(ref _postMortemFaction, "USH_PostMortemFaction");
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
            MutantUtility.ResurrectAsShambler(Pawn, LIFESPAN_TICKS);

            if (_postMortemFaction != null)
                Pawn.SetFactionDirect(_postMortemFaction);
        }
    }
}
