using Verse;

namespace BiologicalWarfare
{
    public class HediffCompProperties_DisorientationSeverity : HediffCompProperties_Disorientation
    {
        public FloatRange severityRange = FloatRange.One;
        public HediffCompProperties_DisorientationSeverity() => compClass = typeof(HediffCompDisorientationSeverity);
    }
    public class HediffCompDisorientationSeverity : HediffComp_Disorientation
    {
        public HediffCompProperties_DisorientationSeverity Props => (HediffCompProperties_DisorientationSeverity)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (parent.Severity < Props.severityRange.min || parent.Severity > Props.severityRange.max)
                return;

            base.CompPostTick(ref severityAdjustment);
        }
    }
}