using RimWorld;
using UnityEngine;
using Verse;

namespace USH_BW
{
    public class GraphicPathogenGas : Graphic_Single
    {
        private readonly MaterialPropertyBlock _materialPropertyBlock = new MaterialPropertyBlock();

        private const float POSITION_VARIANCE = 0.45f;
        private const float SCALE_VARIANCE = 0.2f;
        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            Rand.PushState();
            Rand.Seed = thing.thingIDNumber.GetHashCode();

            PathogenGas gas = thing as PathogenGas;

            float angle = Rand.Range(0, 360) + ((gas == null) ? 0f : gas.graphicRotation);
            Vector3 position = thing.TrueCenter() + new Vector3(RandPosOffset(), 0f, RandPosOffset());
            Vector3 scale = new Vector3(RandScaleOffset() * drawSize.x, 0f, RandScaleOffset() * drawSize.y);
            Matrix4x4 matrix = Matrix4x4.TRS(position, Quaternion.AngleAxis(angle, Vector3.up), scale);

            _materialPropertyBlock.SetColor("_Color", Color.ToTransparent(gas.Alpha()));

            Graphics.DrawMesh(MeshPool.plane10, matrix, MatSingle, 0, Find.Camera, 0, _materialPropertyBlock);
            Rand.PopState();
        }

        private float RandPosOffset() => Rand.Range(-POSITION_VARIANCE, POSITION_VARIANCE);
        private float RandScaleOffset() => Rand.Range(1 - SCALE_VARIANCE, 1 + SCALE_VARIANCE);

    }
}
