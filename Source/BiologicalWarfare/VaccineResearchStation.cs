using Verse;

namespace BiologicalWarfare
{
    public class CompVaccineResearchContainer : CompDiseaseSampleContainer
    {
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override AcceptanceReport CanExtract(Pawn pawn)
        {
            AcceptanceReport baseResult = base.CanExtract(pawn);

            if (!baseResult.Accepted)
                return baseResult;

            return baseResult;
        }
    }

    public class BuildingVaccineResearchStation : Building
    {
        public override string GetInspectString()
        {
            return "Researching...";
        }

        public void Test()
        {
            Log.Message("I'm working!");
        }
    }
}