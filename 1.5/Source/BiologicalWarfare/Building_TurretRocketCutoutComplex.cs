using RimWorld;
using UnityEngine;
using Verse;

namespace BiologicalWarfare
{
    public class Building_TurretRocketCutoutComplex : Building_TurretRocket
    {
        private Material _cachedMaterialFull, _cachedMaterialEmpty;

        public Material MaterialFull
        {
            get
            {
                if (_cachedMaterialFull == null)
                {
                    GraphicData graphicDataFull = def.building.turretGunDef.building.turretTopLoadedGraphic;
                    CreateMat(ref _cachedMaterialFull, graphicDataFull.texPath, graphicDataFull.maskPath, graphicDataFull.color);
                }

                return _cachedMaterialFull;
            }
        }

        public Material MaterialEmpty
        {
            get
            {
                if (_cachedMaterialEmpty == null)
                {
                    GraphicData graphicDataEmpty = def.building.turretGunDef.graphicData;
                    CreateMat(ref _cachedMaterialEmpty, graphicDataEmpty.texPath, graphicDataEmpty.maskPath, graphicDataEmpty.color);
                }

                return _cachedMaterialEmpty;
            }
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
                    return MaterialFull;

                return MaterialEmpty;
            }
        }
    }

}
