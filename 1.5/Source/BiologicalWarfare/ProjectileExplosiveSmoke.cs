using RimWorld;
using UnityEngine;
using Verse;

namespace BiologicalWarfare
{
    public class ProjectileExplosiveSmoke : Projectile_Explosive
    {
        private int _ticksUntilSmoke;
        private int _smokeSpawned;
        private static readonly IntRange SMOKE_INTERVAL = new IntRange(1, 25);
        private static readonly int FIRE_CYCLE = 2;
        public override void Tick()
        {
            base.Tick();

            _ticksUntilSmoke--;
            if (_ticksUntilSmoke <= 0)
                SpawnSmokeParticles();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _ticksUntilSmoke, "USH_TicksUntilSmoke", 0);
            Scribe_Values.Look(ref _smokeSpawned, "USH_SmokeSpawned", 0);
        }

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
            _ticksUntilSmoke = SMOKE_INTERVAL.Lerped(Random.Range(0f, 1f));
        }
    }
}
