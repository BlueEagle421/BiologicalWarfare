using RimWorld;
using Verse;

namespace USH_BW
{
    public class Building_AntigensAnalyzer : Building
    {
        private CompDiseaseSampleContainer _compDiseaseContainer;
        private CompPowerTrader _compPowerTrader;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            _compDiseaseContainer = GetComp<CompDiseaseSampleContainer>();
            _compPowerTrader = GetComp<CompPowerTrader>();
        }
        public ResearchProjectDef CurrentVaccineProject()
        {
            if (_compDiseaseContainer.Empty)
                return null;

            return _compDiseaseContainer.ContainedCombatDiseaseDef.vaccineResProjectDef;
        }

        public bool CanPerformResearch(Pawn pawn)
        {
            if (!Spawned)
                return false;

            ResearchProjectDef vaccineProj = CurrentVaccineProject();

            if (vaccineProj == null)
                return false;

            if (vaccineProj.IsFinished)
                return false;

            if (vaccineProj.prerequisites.Any(x => !x.IsFinished))
                return false;

            if (pawn == null)
                return false;

            if (pawn.WorkTypeIsDisabled(WorkTypeDefOf.Research))
                return false;

            if (!_compPowerTrader.PowerOn)
                return false;

            return true;
        }

        public override string GetInspectString()
        {
            string baseString = base.GetInspectString();

            if (!Spawned)
                return baseString;

            CompDiseaseSample diseaseSample = _compDiseaseContainer.ContainedSampleComp;

            ResearchProjectDef vaccineProj = CurrentVaccineProject();

            if (diseaseSample == null)
                return baseString + string.Format("\n{0}", "USH_InsertSampleToResearch".Translate());

            if (vaccineProj == null)
                return baseString + string.Format("\n{0}", "USH_WrongSampleToResearch".Translate());

            baseString += string.Format("\n{0}: {1}\n{2}: {3:F0} / {4:F0} ({5})", new object[]
                {
                    "CurrentProject".Translate(),
                    vaccineProj.LabelCap,
                    "ResearchProgress".Translate(),
                    vaccineProj.ProgressApparent,
                    vaccineProj.CostApparent,
                    vaccineProj.ProgressPercent.ToStringPercent("0.#")
                });

            return baseString;
        }

        public void ResearchPerformed(float progress, Pawn researcher)
        {
            if (CurrentVaccineProject() == null)
            {
                Log.Error("Researched without having a vaccine project.");
                return;
            }

            ResearchProjectDef vaccineProj = CurrentVaccineProject();

            progress *= ResearchManager.ResearchPointsPerWorkTick;
            progress *= Find.Storyteller.difficulty.researchSpeedFactor;

            if (researcher?.Faction != null)
                progress /= vaccineProj.CostFactor(researcher.Faction.def.techLevel);

            if (DebugSettings.fastResearch)
                progress *= 500f;

            researcher?.records.AddTo(RecordDefOf.ResearchPointsResearched, progress);

            if (!vaccineProj.IsFinished)
                Find.ResearchManager.AddProgress(vaccineProj, progress, researcher);
        }
    }
}