using RimWorld;
using UnityEngine;
using Verse;

namespace BiologicalWarfare
{
    public class Building_TurretRocketCutoutComplex : Building_TurretRocket
    {
        private Material _cachedMaterialFull, _cachedMaterialEmpty;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            CreateFullMat();
            CreateEmptyMat();
        }
        private void CreateFullMat()
        {
            Color colorFull = def.building.turretGunDef.graphicData.color;

            string pathTexFull = def.building.turretGunDef.building.turretTopLoadedGraphic.texPath;
            string pathMaskFull = def.building.turretGunDef.building.turretTopLoadedGraphic.maskPath;

            CreateMat(ref _cachedMaterialFull, pathTexFull, pathMaskFull, colorFull);
        }

        private void CreateEmptyMat()
        {
            Color colorEmpty = def.building.turretGunDef.graphicData.color;

            string pathTexEmpty = def.building.turretGunDef.graphicData.texPath;
            string pathMaskEmpty = def.building.turretGunDef.graphicData.maskPath;

            CreateMat(ref _cachedMaterialEmpty, pathTexEmpty, pathMaskEmpty, colorEmpty);
        }

        private void CreateMat(ref Material material, string texPath, string maskPath, Color color)
        {
            material = MaterialPool.MatFrom(texPath, ShaderDatabase.CutoutComplex, color);
            material.SetTexture(ShaderPropertyIDs.MaskTex, ContentFinder<Texture2D>.Get(maskPath, true));
        }

        public override Material TurretTopMaterial
        {
            get
            {
                if (refuelableComp.IsFull)
                    return _cachedMaterialFull;

                return _cachedMaterialEmpty;
            }
        }
    }

}
