using Verse;

namespace USH_BW
{
    public class CompProperties_DiseaseSample : CompProperties
    {
        public CombatDiseaseDef combatDiseaseDef;
        public CompProperties_DiseaseSample() => compClass = typeof(CompDiseaseSample);
    }

    public class CompDiseaseSample : ThingComp
    {
        public CompProperties_DiseaseSample PropsDiseaseSample => (CompProperties_DiseaseSample)props;
    }
}