﻿using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace BiologicalWarfare
{

    public class CompMechanitesGathererContainer : CompDiseaseSampleContainer
    {
        private CompMechanitesGatherer _compMechanitesGatherer;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            _compMechanitesGatherer = parent.GetComp<CompMechanitesGatherer>();
        }

        public override void OnInserted(Pawn pawn)
        {
            base.OnInserted(pawn);
            _compMechanitesGatherer.OnSampleInserted();
        }
    }

    public class CompAssignableToPawnGatherer : CompAssignableToPawn
    {
        public override string CompInspectStringExtra()
        {
            if (AssignedPawnsForReading.Count == 0)
                return "USH_User".Translate() + ": " + "Nobody".Translate();

            if (AssignedPawnsForReading.Count == 1)
                return "USH_User".Translate() + ": " + AssignedPawnsForReading[0].LabelShortCap;

            return "USH_Users".Translate() + ": " + string.Join(", ", AssignedPawnsForReading.Select(x => x.LabelShortCap));
        }

        protected override string GetAssignmentGizmoLabel() => "USH_SetUsers".Translate();
    }

    public class CompProperties_MechanitesGatherer : CompProperties_Interactable
    {
        public int ticksToGather;
        public CompProperties_MechanitesGatherer() => compClass = typeof(CompMechanitesGatherer);
    }

    public class CompMechanitesGatherer : CompInteractable
    {
        private bool _readyToInfect;
        private int _gatheringTicks;

        public bool IsGathering => !_readyToInfect && !_compSampleContainer.Empty;

        private GatheringInfo _currentBiomeInfo;

        private CompAssignableToPawn _compAssignableToPawn;
        private CompDiseaseSampleContainer _compSampleContainer;

        public new CompProperties_MechanitesGatherer Props => (CompProperties_MechanitesGatherer)props;

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (IsGathering)
            {
                stringBuilder.AppendLine("USH_GatheringProgress".Translate(GatheringProgress().ToStringPercent()));
                stringBuilder.AppendLine("USH_GatheringMultiplier".Translate(_currentBiomeInfo.SpeedMultiplier.ToStringPercent()));

                if (!CanGather().Accepted)
                    stringBuilder.AppendLine("USH_CantGather".Translate(CanGather().Reason).Colorize(ColorLibrary.RedReadable));
            }

            if (_readyToInfect)
                stringBuilder.AppendLine("USH_MechanitesReady".Translate().Colorize(ColorLibrary.Cyan));

            return stringBuilder.ToString().Trim();
        }

        private float GatheringProgress() => _gatheringTicks / (float)Props.ticksToGather;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            _compAssignableToPawn = parent.GetComp<CompAssignableToPawn>();
            _compSampleContainer = parent.GetComp<CompDiseaseSampleContainer>();
        }

        public override void CompTick()
        {
            base.CompTick();

            if (parent.IsHashIntervalTick(GenTicks.TickLongInterval))
                GatherLongTick();
        }

        private void GatherLongTick()
        {
            if (_readyToInfect)
                return;

            if (!CanGather().Accepted)
                return;

            _gatheringTicks += (int)(GenTicks.TickLongInterval * _currentBiomeInfo.SpeedMultiplier);

            if (_gatheringTicks >= Props.ticksToGather)
                GatheringEnded();
        }

        private void GatheringEnded()
        {
            _readyToInfect = true;
            UpdateDesignation(true);

            _gatheringTicks = 0;
        }

        private void UpdateDesignation(bool add)
        {
            Designation designation = parent.Map.designationManager.DesignationOn(parent, USHDefOf.USH_GathererInteraction);
            if (designation == null && add)
                parent.Map.designationManager.AddDesignation(new Designation(parent, USHDefOf.USH_GathererInteraction));
            else
                designation.Delete();
        }

        protected override void OnInteracted(Pawn caster)
        {
            base.OnInteracted(caster);

            caster.health.AddHediff(_compSampleContainer.ContainedCombatDiseaseDef.giveHediffDef);

            _readyToInfect = false;
            UpdateDesignation(false);
        }

        public void OnSampleInserted()
        {
            _currentBiomeInfo = new GatheringInfo(parent.Map.Biome,
                    _compSampleContainer.ContainedCombatDiseaseDef.giveHediffDef);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (DebugSettings.ShowDevGizmos)
            {
                Command_Action commandAction = new Command_Action
                {
                    defaultLabel = "DEBUG: Gather now",
                    action = () => _gatheringTicks = Props.ticksToGather
                };
                yield return commandAction;
            }
        }

        public override AcceptanceReport CanInteract(Pawn activateBy = null, bool checkOptionalItems = true)
        {
            if (!_compAssignableToPawn.AssignedPawnsForReading.Contains(activateBy))
                return "NotAssigned".Translate();

            HediffDef diseaseDef = _compSampleContainer.ContainedCombatDiseaseDef.giveHediffDef;
            if (activateBy.health.hediffSet.HasHediff(diseaseDef))
                return "USH_AlreadyInfected".Translate(diseaseDef.label);

            return base.CanInteract(activateBy, checkOptionalItems);
        }

        private AcceptanceReport CanGather()
        {
            if (!power.PowerOn)
                return "NoPower".Translate().CapitalizeFirst();

            if (_compSampleContainer.Empty)
                return "USH_NoSample".Translate(_compSampleContainer.PropsSampleContainer.acceptableDiseaseType.ToStringUncapitalized().Named("TYPE"));

            if (!_currentBiomeInfo.IsGatheringPossible)
                return "USH_WrongBiome".Translate(
                    _compSampleContainer.ContainedCombatDiseaseDef.giveHediffDef.label.Named("DISEASE"),
                    _currentBiomeInfo.Label.Named("BIOME"));

            if (_readyToInfect)
                return "USH_AlreadyGathered".Translate();

            return true;
        }

        private struct GatheringInfo : IExposable
        {
            private string _label;
            private float _diseaseCommonality;
            private bool _isGatheringPossible;
            private float _speedMultiplier;
            private HediffDef _hediffDef;
            public string Label => _label;
            public float DiseaseCommonality => _diseaseCommonality;
            public bool IsGatheringPossible => _isGatheringPossible;
            public float SpeedMultiplier => _speedMultiplier;
            public HediffDef HediffDef => _hediffDef;

            public GatheringInfo(BiomeDef biomeDef, HediffDef hediffDef)
            {
                _label = biomeDef.label;
                IncidentDef incidentDef = DefDatabase<IncidentDef>.AllDefs.First(x => x.diseaseIncident == hediffDef);
                _diseaseCommonality = biomeDef.CommonalityOfDisease(incidentDef);
                _isGatheringPossible = _diseaseCommonality > 0;
                _speedMultiplier = 1 + _diseaseCommonality / 100f;
                _hediffDef = hediffDef;
            }

            public void ExposeData()
            {
                Scribe_Values.Look(ref _label, "USH_Label");
                Scribe_Values.Look(ref _diseaseCommonality, "USH_DiseaseCommonality");
                Scribe_Values.Look(ref _isGatheringPossible, "USH_IsGatheringPossible");
                Scribe_Values.Look(ref _speedMultiplier, "USH_SpeedMultiplier");
                Scribe_Defs.Look(ref _hediffDef, "USH_HediffDef");
            }
        }
    }
}