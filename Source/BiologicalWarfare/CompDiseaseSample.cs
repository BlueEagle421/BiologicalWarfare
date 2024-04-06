using Verse;

namespace BiologicalWarfare
{
    public class CompProperties_DiseaseSample : CompProperties
    {
        public CombatDiseaseDef combatDiseaseDef;
        public CompProperties_DiseaseSample() => compClass = typeof(CompDiseaseSample);
    }

    public class CompDiseaseSample : ThingComp
    {
        public CompProperties_DiseaseSample Props => (CompProperties_DiseaseSample)props;
    }
}