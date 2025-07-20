using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace USH_BW
{
    public class CompGasVentsController : CompInteractable
    {
        private InteractParams _interactParams;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref _interactParams, "USH_InteractParams");
        }

        public override void OrderForceTarget(LocalTargetInfo target)
        {
            if (ValidateTarget(target, false))
                BeginVentTargeting(target.Pawn);
        }

        protected override void OnInteracted(Pawn caster)
        {
            base.OnInteracted(caster);

            _interactParams.OnInteracted(caster);
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            AcceptanceReport report = CanInteract(selPawn, true);

            FloatMenuOption activateOption = CreateActivateOption(Props.jobString.CapitalizeFirst(), selPawn, () => BeginVentTargeting(selPawn));

            FloatMenuOption activateAllOption = CreateActivateOption("USH_GasVentActivateAll".Translate(), selPawn, delegate ()
            {
                _interactParams = new InteractParams(InteractParams.InteractType.All, this, null);
                OrderPawnToInteract(selPawn);
            });

            if (!report.Accepted)
            {
                DisableOption(activateOption, report.Reason);
                DisableOption(activateAllOption, report.Reason);
            }

            yield return activateOption;
            yield return activateAllOption;
        }

        private FloatMenuOption CreateActivateOption(string label, Pawn pawn, Action action)
        {
            return FloatMenuUtility.DecoratePrioritizedTask(
            new FloatMenuOption(label, action, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0), pawn, parent);
        }

        private void DisableOption(FloatMenuOption option, string reason)
        {
            option.Label += string.Format(" ({0})", reason);
            option.Disabled = true;
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

                AcceptanceReport report = compGasVent.CanInteractRemotely(caster);

                if (!report.Accepted)
                {
                    Messages.Message("USH_GasVentCantActivate".Translate(t.Thing.LabelShort.UncapitalizeFirst(), report.Reason), t.Thing, MessageTypeDefOf.RejectInput);
                    BeginVentTargeting(caster);
                    return;
                }

                _interactParams = new InteractParams(InteractParams.InteractType.Single, this, compGasVent);
                OrderPawnToInteract(caster);

            }, delegate (LocalTargetInfo t) { OnVentTargetingGUI(t); });
        }

        private void OrderPawnToInteract(Pawn pawn)
        {
            Job job = JobMaker.MakeJob(JobDefOf.InteractThing, parent);
            pawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
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

        public bool IsValidVentTarget(Thing thing)
        {
            if (thing == null)
                return false;

            if (thing.TryGetComp<CompGasVent>() == null)
                return false;

            if (!InTheSamePowerNet(power, thing.TryGetComp<CompPower>()))
                return false;

            return true;
        }

        public bool InTheSamePowerNet(CompPower compPower1, CompPower compPower2)
        {
            if (compPower1 == null)
                return false;

            if (compPower2 == null)
                return false;

            return compPower1.PowerNet == compPower2.PowerNet;
        }

        private class InteractParams : IExposable
        {
            private InteractType _interactType;
            private ThingWithComps _controller;
            private ThingWithComps _targetedGasVent;
            public InteractParams() { } //empty constructor for IExposable
            public InteractParams(InteractType interactType, CompGasVentsController controller, CompGasVent targetedGasVent)
            {
                _interactType = interactType;
                _controller = controller?.parent;
                _targetedGasVent = targetedGasVent?.parent;
            }
            public void ExposeData()
            {
                Scribe_Values.Look(ref _interactType, "USH_InteractType");
                Scribe_References.Look(ref _controller, "USH_Controller");
                Scribe_References.Look(ref _targetedGasVent, "USH_TargetedGasVent");
            }

            public void OnInteracted(Pawn caster)
            {
                switch (_interactType)
                {
                    case InteractType.Single:
                        _targetedGasVent?.GetComp<CompGasVent>().Interact(caster, true);
                        break;
                    case InteractType.All:
                        PowerNet powerNet = _controller.GetComp<CompPowerTrader>().PowerNet;
                        AllVentsInPowerNet(powerNet).Where(x => x.CanInteractRemotely(caster)).ToList().ForEach(x => x.Interact(caster, true));
                        break;
                }
            }
            private List<CompGasVent> AllVentsInPowerNet(PowerNet powerNet)
            {
                return powerNet.transmitters
                    .Where(x => x.parent.GetComp<CompGasVent>() != null)
                    .Select(x => x.parent.GetComp<CompGasVent>()).ToList();
            }

            public enum InteractType
            {
                Single,
                All
            }
        }
    }
}
