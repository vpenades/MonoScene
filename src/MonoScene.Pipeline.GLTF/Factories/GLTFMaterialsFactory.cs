using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

using MonoScene.Graphics.Content;

namespace MonoScene.Graphics.Pipeline
{
    sealed class GLTFMaterialsFactory : MaterialCollectionBuilder<SharpGLTF.Schema2.Material>
    {
        public GLTFMaterialsFactory(Converter<SharpGLTF.Schema2.ExtraProperties, Object> tagConverter)
        {
            _TagConverter = tagConverter;
        }

        private readonly Converter<SharpGLTF.Schema2.ExtraProperties, Object> _TagConverter;

        protected override MaterialContent Convert(SharpGLTF.Schema2.Material srcMaterial)
        {
            var dstMaterial = new MaterialContent();
            dstMaterial.Name = srcMaterial.Name;
            dstMaterial.Tag = _TagConverter?.Invoke(srcMaterial);

            dstMaterial.DoubleSided = srcMaterial.DoubleSided;

            dstMaterial.AlphaCutoff = srcMaterial.AlphaCutoff;
            switch (srcMaterial.Alpha)
            {
                case SharpGLTF.Schema2.AlphaMode.OPAQUE: dstMaterial.Mode = MaterialBlendMode.Opaque; break;
                case SharpGLTF.Schema2.AlphaMode.MASK: dstMaterial.Mode = MaterialBlendMode.Mask; break;
                case SharpGLTF.Schema2.AlphaMode.BLEND: dstMaterial.Mode = MaterialBlendMode.Blend; break;
            }

            if (srcMaterial.Unlit) dstMaterial.PreferredShading = "Unlit";
            else if (srcMaterial.FindChannel("SpecularGlossiness") != null) dstMaterial.PreferredShading = "SpecularGlossiness";
            else if (srcMaterial.FindChannel("MetallicRoughness") != null) dstMaterial.PreferredShading = "MetallicRoughness";

            foreach (var srcChannel in srcMaterial.Channels)
            {
                var dstChannel = dstMaterial.UseChannel(srcChannel.Key);

                dstChannel.Value = ParamToArray(srcChannel);

                if (srcChannel.Texture != null)
                {
                    var imgData = srcChannel.Texture.PrimaryImage.Content.Content.ToArray();

                    var texContent = new ImageContent(imgData);

                    dstChannel.TextureIndex = UseTexture(texContent);
                    dstChannel.Sampler = ToXna(srcChannel.Texture.Sampler);
                }
                else
                {
                    dstChannel.Sampler = SamplerStateContent.CreateDefault();
                }


                dstChannel.VertexIndexSet = srcChannel.TextureCoordinate;
                dstChannel.Transform = (srcChannel.TextureTransform?.Matrix ?? System.Numerics.Matrix3x2.Identity).ToXna();
            }

            return dstMaterial;
        }

        private static float[] ParamToArray(SharpGLTF.Schema2.MaterialChannel srcChannel)
        {
            var val = srcChannel.Parameter;
            switch (srcChannel.Key)
            {
                case "Normal":
                case "Occlusion":
                case "ClearCoat":
                case "ClearCoatNormal":
                case "ClearCoatRoughness":
                    return new float[] { val.X };

                case "MetallicRoughness":
                    return new float[] { val.X, val.Y };

                case "Emissive":
                    return new float[] { val.X, val.Y, val.Z };

                case "Diffuse":
                case "BaseColor":
                case "SpecularGlossiness":
                    return new float[] { val.X, val.Y, val.Z, val.W };
            }

            throw new NotImplementedException();
        }

        private static SamplerStateContent ToXna(SharpGLTF.Schema2.TextureSampler srcSampler)
        {
            if (srcSampler == null) return SamplerStateContent.CreateDefault();

            return new SamplerStateContent
            {
                AddressU = srcSampler.WrapS.ToXna(),
                AddressV = srcSampler.WrapT.ToXna(),
                AddressW = TextureAddressMode.Wrap,
                Filter = (srcSampler.MagFilter, srcSampler.MinFilter).ToXna()
            };
        }
    }
}
