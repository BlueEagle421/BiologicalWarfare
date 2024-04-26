using RimWorld;
using UnityEngine;
using Verse;

namespace BiologicalWarfare
{
    public class Projectile_ExplosiveSmoke : Projectile_Explosive
    {
        private int _ticksUntilSmoke;
        private int _smokeSpawned;
        private static readonly IntRange SMOKE_INTERVAL = new IntRange(1, 25);
        public override void Tick()
        {
            base.Tick();

            _ticksUntilSmoke--;
            if (_ticksUntilSmoke <= 0)
                SpawnSmokeParticles();
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

        private void SpawnSmokeParticles()
        {
            float num = ArcHeightFactor * GenMath.InverseParabola(DistanceCoveredFractionArc);
            Vector3 vector = DrawPos + new Vector3(0f, 0f, 1f) * num;

            FleckMaker.ThrowSmoke(vector, Map, 1f);

            if (_smokeSpawned % 2 == 0)
                FleckMaker.ThrowFireGlow(vector, Map, 1f);

            _smokeSpawned++;
            _ticksUntilSmoke = SMOKE_INTERVAL.Lerped(UnityEngine.Random.Range(0f, 1f));
        }
    }
}
