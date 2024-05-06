using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace BiologicalWarfare
{
    public class CompGasVentsController : CompInteractable
    {
        private CompPower _compPower;
        private CompGasVent _targetedGasVent;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            _compPower = parent.GetComp<CompPower>();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref _targetedGasVent, "USH_TargetedGasVent");
        }

        public override void OrderForceTarget(LocalTargetInfo target)
        {
            if (ValidateTarget(target, false))
                BeginVentTargeting(target.Pawn);
        }

        protected override void OnInteracted(Pawn caster)
        {
            base.OnInteracted(caster);

            _targetedGasVent.Interact(caster);
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            AcceptanceReport report = CanInteract(selPawn, true);
            string optionLabel = Props.jobString.CapitalizeFirst();

            FloatMenuOption interactOption = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(optionLabel, delegate ()
            {
                BeginVentTargeting(selPawn);
            }, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0), selPawn, parent);

            if (!report.Accepted)
            {
                interactOption.Label += string.Format(" ({0})", report.Reason);
                interactOption.Disabled = true;
            }

            yield return interactOption;

            yield break;
        }

        public override AcceptanceReport CanInteract(Pawn activateBy = null, bool checkOptionalItems = true)
        {
            AcceptanceReport result = base.CanInteract(activateBy, checkOptionalItems);
            if (!result.Accepted)
                return result;

            if (activateBy != null && activateBy.WorkTagIsDisabled(WorkTags.Violent))
                return "IsIncapableOfViolence".Translate(activateBy.LabelShort, activateBy);

            return true;
        }

        private void BeginVentTargeting(Pawn caster)
        {
            Find.Targeter.BeginTargeting(VentTargetingParams(), delegate (LocalTargetInfo t)
            {
                CompGasVent compGasVent = t.Thing.TryGetComp<CompGasVent>();

                AcceptanceReport report = compGasVent.CanInteract(caster);

                if (!report.Accepted)
                {
                    Messages.Message("USH_GasVentCantActivate".Translate(t.Thing.LabelShort.UncapitalizeFirst(), report.Reason), t.Thing, MessageTypeDefOf.RejectInput);
                    BeginVentTargeting(caster);
                    return;
                }

                _targetedGasVent = compGasVent;

                Job job = JobMaker.MakeJob(JobDefOf.InteractThing, parent);
                caster.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);

            }, delegate (LocalTargetInfo t) { OnVentTargetingGUI(t); });
        }

        private void OnVentTargetingGUI(LocalTargetInfo target)
        {
            OverlayDrawHandler.DrawPowerGridOverlayThisFrame();
            Widgets.MouseAttachedLabel("USH_GasVentChoose".Translate());

            if (IsValidVentTarget(target.Thing))
                GenUI.DrawMouseAttachment(UIIcon);
            else
                GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
        }

        private TargetingParameters VentTargetingParams()
        {
            return new TargetingParameters
            {
                canTargetPawns = false,
                canTargetBuildings = true,
                canTargetItems = false,
                mapObjectTargetsMustBeAutoAttackable = false,
                canTargetLocations = false,
                canTargetSelf = false,
                validator = (TargetInfo x) => IsValidVentTarget(x.Thing)
            };
        }

        private bool IsValidVentTarget(Thing thing)
        {
            if (thing == null)
                return false;

            if (thing.TryGetComp<CompGasVent>() == null)
                return false;

            if (!InTheSamePowerNet(_compPower, thing.TryGetComp<CompPower>()))
                return false;

            return true;
        }

        private bool InTheSamePowerNet(CompPower compPower1, CompPower compPower2)
        {
            if (compPower1 == null)
                return false;

            if (compPower2 == null)
                return false;

            return compPower1.PowerNet == compPower2.PowerNet;
        }
    }
}
