using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

using GLTFMATERIAL = SharpGLTF.Schema2.Material;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public class PBRMeshFactory : GLTFMeshFactory
    {
        public PBRMeshFactory(GraphicsDevice device) : base(device) { }

        protected override MeshPrimitiveMaterial ConvertMaterial(GLTFMATERIAL srcMaterial, bool isSkinned)
        {
            if (srcMaterial == null) srcMaterial = GetDefaultMaterial();

            var effect = CreateEffect(srcMaterial, isSkinned);            

            var blending = BlendState.Opaque;            

            if (srcMaterial.Alpha == SharpGLTF.Schema2.AlphaMode.BLEND)
            {
                blending = BlendState.NonPremultiplied;
                effect.AlphaBlend = true;
            }

            if (srcMaterial.Alpha == SharpGLTF.Schema2.AlphaMode.MASK)
            {
                effect.AlphaCutoff = srcMaterial.AlphaCutoff;
            }            

            if (effect is PBREffect pbrEffect)
            {
                pbrEffect.NormalMode = srcMaterial.DoubleSided ? GeometryNormalMode.DoubleSided : GeometryNormalMode.Reverse;
            }

            var material = new MeshPrimitiveMaterial();

            material.Effect = effect;
            material.DoubleSided = srcMaterial.DoubleSided;
            material.Blend = blending;            

            return material;
        }

        private AnimatedEffect CreateEffect(GLTFMATERIAL srcMaterial, bool isSkinned)
        {
            if (srcMaterial.Unlit)
            {
                var ueffect = new UnlitEffect(this.Device);

                TransferChannel(ueffect.BaseColorMap, srcMaterial, "BaseColor", Vector4.One);
                TransferChannel(ueffect.EmissiveMap, srcMaterial, "Emissive", Vector3.Zero);
                TransferChannel(ueffect.OcclusionMap, srcMaterial, "Occlusion", 0);
                if (ueffect.OcclusionMap.Texture == null) ueffect.OcclusionMap.Scale = 0;

                return ueffect;
            }

            PBREffect effect = null;

            if (srcMaterial.FindChannel("SpecularGlossiness") != null)
            {
                var xeffect = new PBRSpecularGlossinessEffect(this.Device);
                effect = xeffect;

                TransferChannel(xeffect.DiffuseMap, srcMaterial, "Diffuse", Vector4.One);
                TransferChannel(xeffect.SpecularGlossinessMap, srcMaterial, "SpecularGlossiness", Vector4.Zero);
            }
            else
            {
                var xeffect = new PBRMetallicRoughnessEffect(this.Device);
                effect = xeffect;

                TransferChannel(xeffect.BaseColorMap, srcMaterial, "BaseColor", Vector4.One);
                TransferChannel(xeffect.MetalRoughnessMap, srcMaterial, "MetallicRoughness", Vector2.One);
            }

            TransferChannel(effect.NormalMap, srcMaterial, "Normal", 1);
            TransferChannel(effect.EmissiveMap, srcMaterial, "Emissive", Vector3.Zero);
            TransferChannel(effect.OcclusionMap, srcMaterial, "Occlusion", 0);
            if (effect.OcclusionMap.Texture == null) effect.OcclusionMap.Scale = 0;            

            return effect;
        }
    }
}
