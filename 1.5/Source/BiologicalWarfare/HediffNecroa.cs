using RimWorld;
using System.Threading.Tasks;
using Verse;

namespace BiologicalWarfare
{
    public class HediffCompProperties_Necroa : HediffCompProperties
    {

        public HediffCompProperties_Necroa() => compClass = typeof(HediffCompNecroa);
    }

    public class HediffCompNecroa : HediffComp
    {
        public HediffCompProperties_Necroa PropsNecroa => (HediffCompProperties_Necroa)props;

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
        }
    }
}
