using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace BiologicalWarfare
{
    public class CompDiseaseSampleContainer : CompThingContainer
    {
        private bool _markedForExtract;

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption option in base.CompFloatMenuOptions(selPawn))
                yield return option;


            yield return new FloatMenuOption("USH_ExtractSample".Translate(), delegate ()
            {
                OrderExtractionJob(selPawn);
            }, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);

            yield break;
        }

        private void OrderExtractionJob(Pawn pawn)
        {
            if (Empty)
            {
                Messages.Message("USH_SampleContainerEmpty".Translate(parent.Named("BUILDING")), parent, MessageTypeDefOf.CautionInput);
                return;
            }

            Job job = JobMaker.MakeJob(USH_DefOf.USH_ExtractSample, parent);
            pawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            if (Empty)
                yield break;

            Command_Action command_Action = new Command_Action
            {
                action = delegate ()
                {
                    _markedForExtract = !_markedForExtract;
                    UpdateDesignation(parent);
                },
                defaultLabel = "CommandPodEject".Translate(),
                defaultDesc = "CommandPodEjectDesc".Translate(),
                activateSound = SoundDefOf.Designate_Cancel,
                icon = ContentFinder<Texture2D>.Get("UI/Gizmo/ExtractDiseaseSample", true)
            };
            yield return command_Action;
        }

        private void UpdateDesignation(Thing t)
        {
            Designation designation = t.Map.designationManager.DesignationOn(t, USH_DefOf.USH_ExtractDesignation);
            if (designation == null)
                t.Map.designationManager.AddDesignation(new Designation(t, USH_DefOf.USH_ExtractDesignation, null));
            else
                designation.Delete();
        }

        public override string CompInspectStringExtra() => "USH_ContainedSample".Translate(SampleLabelFormatted());

        private string SampleLabelFormatted()
        {
            if (Empty)
                return (string)"Nothing".Translate();

            return ContainedThing.TryGetComp<CompDiseaseSample>().Props.combatDiseaseDef.label;
        }
    }
}
