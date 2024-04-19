using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace BiologicalWarfare
{
    public class PlaceWorker_VirusReplicator : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            Map currentMap = Find.CurrentMap;
            Room room = center.GetRoom(currentMap);

            if (room == null)
                return;

            if (!room.ProperRoom)
                return;

            Color egesColor = WillWork(room, def) ? Color.white : Color.red;

            GenDraw.DrawFieldEdges(room.Cells.ToList(), egesColor, null);
        }

        private bool WillWork(Room room, ThingDef thingDef)
        {
            if (room == null)
                return false;

            if (room.CellCount > thingDef.GetCompProperties<CompProperties_VirusReplicator>().maxRoomCellSize)
                return false;

            if (room.OpenRoofCount != 0)
                return false;

            return true;
        }
    }

    public class CompProperties_VirusReplicator : CompProperties_Interactable
    {
        public int maxRoomCellSize;
        public CompProperties_VirusReplicator() => compClass = typeof(CompVirusReplicator);
    }

    public class CompVirusReplicator : CompInteractable
    {
        public CompProperties_VirusReplicator ReplicatorProps => (CompProperties_VirusReplicator)props;

        private CompDiseaseSampleContainer _sampleContainer;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            _sampleContainer = parent.GetComp<CompDiseaseSampleContainer>();
        }

        public override string CompInspectStringExtra()
        {
            string contaminationChance = USHDefOf.USH_ContaminationChanceFactor.LabelCap + ": " + parent.GetStatValue(USHDefOf.USH_ContaminationChanceFactor).ToStringPercent();
            return base.CompInspectStringExtra() + "\n" + contaminationChance;
        }

        public override AcceptanceReport CanInteract(Pawn activateBy = null, bool checkOptionalItems = true)
        {
            AcceptanceReport baseResult = base.CanInteract(activateBy, checkOptionalItems);

            if (!baseResult)
                return baseResult;

            Room currentRoom = parent.GetRoom();

            if (currentRoom == null)
                return "USH_ReplicatorNoRoom".Translate();

            if (parent.GetRoom().CellCount > ReplicatorProps.maxRoomCellSize)
                return "USH_ReplicatorRoomTooBig".Translate(ReplicatorProps.maxRoomCellSize);

            if (parent.GetRoom().OpenRoofCount != 0)
                return "USH_ReplicatorRoomUnroofed".Translate();

            if (_sampleContainer.Empty)
            {
                NamedArgument typeArgument = _sampleContainer.PropsSampleContainer.acceptableDiseaseType.ToStringUncapitalized().Named("TYPE");
                return "USH_NoSample".Translate(typeArgument);
            }

            return baseResult;
        }

        protected override bool TryInteractTick() => true;
        protected override void OnInteracted(Pawn caster)
        {
            base.OnInteracted(caster);

            BiologicalUtils.TryToContaminate(parent, _sampleContainer.ContainedSampleComp().PropsDiseaseSample.combatDiseaseDef);

            foreach (IntVec3 cell in parent.GetRoom().Cells.ToList())
                foreach (Thing thing in parent.Map.thingGrid.ThingsAt(cell))
                {
                    if (!(thing is Pawn pawn))
                        continue;

                    if (pawn.GetStatValue(StatDefOf.ToxicEnvironmentResistance) >= 0.8f)
                        continue;

                    if (pawn.Position.DistanceTo(parent.Position) <= parent.def.specialDisplayRadius)
                        continue;

                    pawn.health.AddHediff(_sampleContainer.ContainedSampleComp().PropsDiseaseSample.combatDiseaseDef.giveHediffDef);

                    Hediff extractionHediff = pawn.health.AddHediff(USHDefOf.USH_VirusExtraction);
                    extractionHediff.TryGetComp<HediffCompVirusExtraction>().CombatDiseaseDef = _sampleContainer.ContainedSampleComp().PropsDiseaseSample.combatDiseaseDef;
                }

            _sampleContainer.innerContainer.ClearAndDestroyContents();
        }
    }


}
