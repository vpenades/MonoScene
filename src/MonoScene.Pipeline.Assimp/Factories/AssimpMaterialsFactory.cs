using System;
using System.Collections.Generic;
using System.Text;

using MonoScene.Graphics.Content;

namespace MonoScene.Graphics.Pipeline
{
    sealed class AssimpMaterialsFactory : MaterialCollectionBuilder<Assimp.Material>
    {
        public AssimpMaterialsFactory(IEnumerable<Assimp.Material> srcMaterials)
        {
            foreach (var m in srcMaterials) UseMaterial(m);
        }

        protected override MaterialContent Convert(Assimp.Material srcMaterial)
        {
            var dstMaterial = new MaterialContent();
            dstMaterial.Name = srcMaterial.Name;

            dstMaterial.DoubleSided = srcMaterial.HasTwoSided;
            dstMaterial.Mode = srcMaterial.BlendMode == Assimp.BlendMode.Default ? MaterialBlendMode.Opaque : MaterialBlendMode.Blend;
            dstMaterial.AlphaCutoff = 0.5f;

            if (srcMaterial.IsPBRMaterial)
            {
                dstMaterial.PreferredShading = "MetallicRoughness";
                SetTexture(dstMaterial, "Normals", srcMaterial.PBR.TextureNormalCamera);
                SetTexture(dstMaterial, "BaseColor", srcMaterial.PBR.TextureBaseColor);
                SetTexture(dstMaterial, "MetallicRoughness", srcMaterial.PBR.TextureMetalness);
                SetTexture(dstMaterial, "Emissive", srcMaterial.PBR.TextureEmissionColor);
            }
            else
            {
                dstMaterial.PreferredShading = "MetallicRoughness";
                SetTexture(dstMaterial, "BaseColor", srcMaterial.TextureDiffuse);
            }

            return dstMaterial;
        }

        private static void SetTexture(MaterialContent dstMaterial, string slot, Assimp.TextureSlot srcSlot)
        {
            var dstChannel = dstMaterial.UseChannel(slot);

            dstChannel.VertexIndexSet = srcSlot.UVIndex;
        }
    }
}
