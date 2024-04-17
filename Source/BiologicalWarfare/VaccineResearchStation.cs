using RimWorld;
using Verse;

namespace BiologicalWarfare
{
    public class BuildingVaccineResearchStation : Building
    {
        private CompDiseaseSampleContainer _compDiseaseContainer;
        private CompPowerTrader _compPowerTrader;

        private int _contaminationTicks;

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

            return _compDiseaseContainer.ContainedSampleComp().PropsDiseaseSample.combatDiseaseDef.vaccineResProjectDef;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref _contaminationTicks, "USH_ContaminationTicks", 0);
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

            string contaminationChance = USHDefOf.USH_ContaminationChanceFactor.LabelCap + ": " + this.GetStatValue(USHDefOf.USH_ContaminationChanceFactor).ToStringPercent();

            CompDiseaseSample diseaseSample = _compDiseaseContainer.ContainedSampleComp();

            ResearchProjectDef vaccineProj = CurrentVaccineProject();

            if (diseaseSample == null)
                return baseString + string.Format("\n{0}", "USH_InsertSampleToResearch".Translate()) + "\n" + contaminationChance;

            if (vaccineProj == null)
                return baseString + string.Format("\n{0}", "USH_WrongSampleToResearch".Translate()) + "\n" + contaminationChance;

            baseString += string.Format("\n{0}: {1}\n{2}: {3:F0} / {4:F0} ({5})", new object[]
                {
                    "CurrentProject".Translate(),
                    vaccineProj.LabelCap,
                    "ResearchProgress".Translate(),
                    vaccineProj.ProgressApparent,
                    vaccineProj.CostApparent,
                    vaccineProj.ProgressPercent.ToStringPercent("0.#")
                });

            baseString += "\n" + contaminationChance;

            return baseString;
        }

        public void ResearchPerformed(float progress, Pawn researcher)
        {
            if (CurrentVaccineProject() == null)
            {
                Log.Error("Researched without having a vaccine project.");
                return;
            }

            ContamidationTick();

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

        private void ContamidationTick()
        {
            _contaminationTicks++;

            if (_contaminationTicks < BiologicalUtils.CONTAMINATION_TICKS)
                return;

            _contaminationTicks = 0;

            BiologicalUtils.TryToContaminate(this, _compDiseaseContainer.ContainedSampleComp().PropsDiseaseSample.combatDiseaseDef);
        }
    }
}