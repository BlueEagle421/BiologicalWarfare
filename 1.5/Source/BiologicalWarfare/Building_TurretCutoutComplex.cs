using RimWorld;
using UnityEngine;
using Verse;

namespace BiologicalWarfare
{
    public class Building_TurretCutoutComplex : Building_TurretRocket
    {
        private Material cachedMaterialFull;
        private Material cachedMaterialEmpty;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            Color color = def.building.turretGunDef.graphicData.color;

            string pathFull = def.building.turretGunDef.building.turretTopLoadedGraphic.texPath;
            string pathMaskFull = def.building.turretGunDef.building.turretTopLoadedGraphic.maskPath;

            cachedMaterialFull = MaterialPool.MatFrom(pathFull, ShaderDatabase.CutoutComplex, color);
            cachedMaterialFull.SetTexture(ShaderPropertyIDs.MaskTex, ContentFinder<Texture2D>.Get(pathMaskFull, true));
        }

        public override Material TurretTopMaterial => cachedMaterialFull;
    }

}
