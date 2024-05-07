using OPToxic;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BiologicalWarfare
{
    public class PathogenGas : Gas
    {
        private int _infectedTimes = 0;
        private int _duration;
        private float _density = 1f;
        public float Density
        {
            get
            {
                float result = _density;

                result *= MaxInfectionsMultiplier();
                result *= LifespanMultiplier();

                return result;
            }
            set
            {
                _density = value;
            }
        }

        private AnimationCurve _lifespanDensityCurve;

        private const float ALPHA_MULTIPLIER = 0.75f;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            _density = 1f;

            _duration = destroyTick - Find.TickManager.TicksGame;

            _lifespanDensityCurve = new AnimationCurve(
                new UnityEngine.Keyframe(0, 1),
                new UnityEngine.Keyframe(0.65f, 1),
                new UnityEngine.Keyframe(0f, 0f));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _infectedTimes, "USH_InfectedTimes", 0);
            Scribe_Values.Look(ref _duration, "USH_Duration");
            Scribe_Values.Look(ref _density, "USH_Density");
        }

        public override void Tick()
        {
            base.Tick();

            InfectionTick();
        }

        private void InfectionTick()
        {
            if (Destroyed)
                return;

            if (!this.IsHashIntervalTick(OPToxicDefGetValue.OPToxicGetSevUpVal(def)))
                return;

            List<Thing> thingsInGas = Position.GetThingList(Map);

            for (int i = 0; i < thingsInGas.Count; i++)
            {
                if (!(thingsInGas[i] is Pawn pawnInGas))
                    continue;

                if (BiologicalUtils.AddInfectionSeverity(pawnInGas, this, Density) > 0)
                    _infectedTimes++;
            }
        }

        public override string Label => $"{base.Label} ({Density.ToStringPercent()})";

        public float Alpha() => Density * ALPHA_MULTIPLIER;

        private float MaxInfectionsMultiplier()
        {
            int maxCount = BiologicalWarfareMod.Settings.MaxGasInfectionCount.Value;
            if (maxCount == -1)
                return 1f;

            return 1f - (_infectedTimes / (float)maxCount);
        }

        private float LifespanMultiplier()
        {
            float lifespan = (destroyTick - (float)Find.TickManager.TicksGame) / _duration;

            return _lifespanDensityCurve.Evaluate(lifespan);
        }
    }
}