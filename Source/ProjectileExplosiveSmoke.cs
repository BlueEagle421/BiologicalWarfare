using RimWorld;
using UnityEngine;
using Verse;

namespace USH_BW
{
    public class ProjectileExplosiveSmoke : Projectile_Explosive
    {
        private int _ticksToSmoke;
        private int _smokeSpawned;
        private static readonly IntRange SMOKE_INTERVAL = new(1, 25);
        private static readonly int FIRE_CYCLE = 2;

        protected override void TickInterval(int delta)
        {
            base.TickInterval(delta);

            _ticksToSmoke -= delta;

            if (_ticksToSmoke <= 0)
                SpawnSmokeParticles();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _ticksToSmoke, "USH_TicksUntilSmoke", 0);
            Scribe_Values.Look(ref _smokeSpawned, "USH_SmokeSpawned", 0);
        }

        private Vector3 LookTowards =>
            new(
                destination.x - origin.x,
                def.Altitude,
                destination.z - origin.z + ArcHeightFactor * (4 - 8 * DistanceCoveredFraction));
        public override Quaternion ExactRotation => Quaternion.LookRotation(LookTowards);

        private float ArcHeightFactor
        {
            get
            {
                float arcHeightFactor = def.projectile.arcHeightFactor;
                float magnitude = (destination - origin).MagnitudeHorizontalSquared();

                if (arcHeightFactor * arcHeightFactor > magnitude * 0.2f * 0.2f)
                    arcHeightFactor = Mathf.Sqrt(magnitude) * 0.2f;

                return arcHeightFactor;
            }
        }

        private Vector3 WorldPosition()
        {
            float heightMultiplier = ArcHeightFactor * GenMath.InverseParabola(DistanceCoveredFractionArc);
            return DrawPos + new Vector3(0f, 0f, 1f) * heightMultiplier;
        }

        private void SpawnSmokeParticles()
        {
            Vector3 realPosition = WorldPosition();

            FleckMaker.ThrowSmoke(realPosition, Map, 1f);

            if (_smokeSpawned % FIRE_CYCLE == 0)
                FleckMaker.ThrowFireGlow(realPosition, Map, 1f);

            _smokeSpawned++;
            _ticksToSmoke = SMOKE_INTERVAL.Lerped(Random.Range(0f, 1f));
        }
    }
}
