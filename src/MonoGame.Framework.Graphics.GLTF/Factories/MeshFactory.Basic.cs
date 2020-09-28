using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

using GLTFMATERIAL = SharpGLTF.Schema2.Material;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public class BasicMeshFactory : GLTFMeshFactory
    {
        public BasicMeshFactory(GraphicsDevice device) : base(device)
        {
        }

        // Monogame's BasicEffect uses Phong's shading, while glTF uses PBR shading, so
        // given monogame's limitations, we try to guess the most appropiate values
        // to have a reasonably good looking renders.

        protected override Type GetPreferredVertexType(IMeshPrimitiveDecoder<GLTFMATERIAL> srcPrim)
        {
            return base.GetPreferredVertexType(srcPrim);
        }

        protected override MeshPrimitiveMaterial ConvertMaterial(GLTFMATERIAL srcMaterial, bool mustSupportSkinning)
        {
            if (srcMaterial == null) srcMaterial = GetDefaultMaterial();

            var effect = mustSupportSkinning ? CreateSkinnedEffect(srcMaterial) : CreateRigidEffect(srcMaterial);

            return new MeshPrimitiveMaterial
            {
                Effect = effect,
                DoubleSided = srcMaterial.DoubleSided,
                Blend = BlendState.Opaque
            };
        }

        #region effects creation
        

        protected virtual Effect CreateRigidEffect(GLTFMATERIAL srcMaterial)
        {
            var dstMaterial = srcMaterial.Alpha == SharpGLTF.Schema2.AlphaMode.MASK
                ? CreateAlphaTestEffect(srcMaterial)
                : CreateBasicEffect(srcMaterial);

            return dstMaterial;
        }

        protected virtual Effect CreateBasicEffect(GLTFMATERIAL srcMaterial)
        {
            var dstMaterial = new BasicEffect(Device);

            dstMaterial.Name = srcMaterial.Name;

            dstMaterial.Alpha = GetAlphaLevel(srcMaterial);
            dstMaterial.DiffuseColor = GetDiffuseColor(srcMaterial);
            dstMaterial.SpecularColor = GetSpecularColor(srcMaterial);
            dstMaterial.SpecularPower = GetSpecularPower(srcMaterial);
            dstMaterial.EmissiveColor = GeEmissiveColor(srcMaterial);
            dstMaterial.Texture = UseDiffuseTexture(srcMaterial);

            if (srcMaterial.Unlit)
            {
                dstMaterial.EmissiveColor = dstMaterial.DiffuseColor;
                dstMaterial.SpecularColor = Vector3.Zero;
                dstMaterial.SpecularPower = 16;
            }

            dstMaterial.PreferPerPixelLighting = true;
            dstMaterial.TextureEnabled = dstMaterial.Texture != null;

            return dstMaterial;
        }

        protected virtual Effect CreateAlphaTestEffect(GLTFMATERIAL srcMaterial)
        {
            var dstMaterial = new AlphaTestEffect(Device);

            dstMaterial.Name = srcMaterial.Name;

            dstMaterial.Alpha = GetAlphaLevel(srcMaterial);
            //dstMaterial.AlphaFunction = CompareFunction.GreaterEqual;
            dstMaterial.ReferenceAlpha = (int)(srcMaterial.AlphaCutoff * 255);

            dstMaterial.DiffuseColor = GetDiffuseColor(srcMaterial);

            dstMaterial.Texture = UseDiffuseTexture(srcMaterial);

            return dstMaterial;
        }

        protected virtual Effect CreateSkinnedEffect(GLTFMATERIAL srcMaterial)
        {
            var dstMaterial = new SkinnedEffect(Device);

            dstMaterial.Name = srcMaterial.Name;

            dstMaterial.Alpha = GetAlphaLevel(srcMaterial);
            dstMaterial.DiffuseColor = GetDiffuseColor(srcMaterial);
            dstMaterial.SpecularColor = GetSpecularColor(srcMaterial);
            dstMaterial.SpecularPower = GetSpecularPower(srcMaterial);
            dstMaterial.EmissiveColor = GeEmissiveColor(srcMaterial);
            dstMaterial.Texture = UseDiffuseTexture(srcMaterial);

            dstMaterial.WeightsPerVertex = 4;
            dstMaterial.PreferPerPixelLighting = true;

            // apparently, SkinnedEffect does not support disabling textures, so we set a white texture here.
            if (dstMaterial.Texture == null) dstMaterial.Texture = UseTexture(null); // creates a dummy white texture.

            return dstMaterial;
        }

        #endregion
    }
}
