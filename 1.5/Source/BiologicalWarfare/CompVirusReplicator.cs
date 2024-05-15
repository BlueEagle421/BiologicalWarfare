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
        private const int GOODWILL_CHANGE = -18;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            _sampleContainer = parent.GetComp<CompDiseaseSampleContainer>();
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

            foreach (IntVec3 cell in parent.GetRoom().Cells.ToList())
                foreach (Thing thing in parent.Map.thingGrid.ThingsAt(cell))
                {
                    HediffDef diseaseToAdd = _sampleContainer.ContainedSampleComp().PropsDiseaseSample.combatDiseaseDef.giveHediffDef;

                    if (!(thing is Pawn pawn))
                        continue;

                    if (!BiologicalUtils.CanPathogenInfect(pawn))
                        continue;

                    if (BiologicalUtils.IsImmuneTo(pawn, diseaseToAdd))
                        continue;

                    if (pawn.Position.DistanceTo(parent.Position) <= parent.def.specialDisplayRadius)
                        continue;

                    pawn.health.AddHediff(diseaseToAdd);

                    HediffCompVirusExtraction virusExtraction = pawn.health.AddHediff(USHDefOf.USH_VirusExtraction).TryGetComp<HediffCompVirusExtraction>();
                    virusExtraction.CombatDiseaseDef = _sampleContainer.ContainedSampleComp().PropsDiseaseSample.combatDiseaseDef;
                    virusExtraction.RecacheHediffInfo();

                    DamageGoodwill(caster, pawn);
                }

            _sampleContainer.innerContainer.ClearAndDestroyContents();
        }

        private void DamageGoodwill(Pawn caster, Pawn damaged)
        {
            if (caster?.HomeFaction == null || damaged?.HomeFaction == null)
                return;

            caster.HomeFaction.TryAffectGoodwillWith(
                damaged.HomeFaction,
                GOODWILL_CHANGE,
                true,
                true,
                HistoryEventDefOf.UsedHarmfulItem,
                damaged);
        }
    }


}
