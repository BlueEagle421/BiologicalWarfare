using RimWorld;
using UnityEngine;
using Verse;

namespace USH_BW
{
    public class BuildingTurretRocketCutoutComplex : Building_TurretRocket
    {
        private Material _cachedMaterialFull, _cachedMaterialEmpty;

        public Material MaterialFull
        {
            get
            {
                if (_cachedMaterialFull == null)
                {
                    GraphicData graphicDataFull = def.building.turretGunDef.building.turretTopLoadedGraphic;
                    _cachedMaterialFull = CreateMaterial(graphicDataFull.texPath, graphicDataFull.maskPath, graphicDataFull.color);
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
                    _cachedMaterialEmpty = CreateMaterial(graphicDataEmpty.texPath, graphicDataEmpty.maskPath, graphicDataEmpty.color);
                }

                return _cachedMaterialEmpty;
            }
        }

        private Material CreateMaterial(string texPath, string maskPath, Color color)
        {
            Material result = MaterialPool.MatFrom(texPath, ShaderDatabase.CutoutComplex, color);
            result.SetTexture(ShaderPropertyIDs.MaskTex, ContentFinder<Texture2D>.Get(maskPath, true));

            return result;
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
