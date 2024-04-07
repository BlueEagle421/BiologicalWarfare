using RimWorld;

namespace BiologicalWarfare
{
    public class CompProperties_BacteriaIncubator : CompProperties_Activable
    {
        public int fuelPerPathogen;
        public CompProperties_BacteriaIncubator() => compClass = typeof(CompBacteriaIncubator);
    }

    public class CompBacteriaIncubator : CompActivable
    {
        private CompDiseaseSampleContainer _sampleContainer;
        private CompRefuelable _compRefuelable;
        public CompProperties_BacteriaIncubator IncubatorProps => (CompProperties_BacteriaIncubator)props;

        protected override bool TryUse() => true;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            _sampleContainer = parent.GetComp<CompDiseaseSampleContainer>();
            _compRefuelable = parent.GetComp<CompRefuelable>();
        }
    }
}
