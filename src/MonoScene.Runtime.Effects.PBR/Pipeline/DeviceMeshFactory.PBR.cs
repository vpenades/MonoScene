using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

using MonoScene.Graphics.Content;

using XNAV2 = Microsoft.Xna.Framework.Vector2;
using XNAV3 = Microsoft.Xna.Framework.Vector3;
using XNAV4 = Microsoft.Xna.Framework.Vector4;

using TEXTURELOOKUP = System.Converter<int, Microsoft.Xna.Framework.Graphics.Texture2D>;

namespace MonoScene.Graphics.Pipeline
{
    public class PBRMeshFactory : DeviceMeshFactory
    {
        public PBRMeshFactory(GraphicsDevice device)
            : base(device) { }

        protected override Effect CreateEffect(MaterialContent srcMaterial, bool isSkinned)
        {
            var pbrMaterial = new MaterialContentConverter(srcMaterial, GetTexture);

            var isUnlit = srcMaterial.PreferredShading == "Unlit";

            var effect = isUnlit ? _CreateUnlitEffect(pbrMaterial) : _CreatePBREffect(pbrMaterial);

            if (srcMaterial.Mode == MaterialBlendMode.Blend)
            {
                effect.AlphaBlend = true;
            }

            if (srcMaterial.Mode == MaterialBlendMode.Mask)
            {
                effect.AlphaCutoff = srcMaterial.AlphaCutoff;
            }

            if (effect is PBREffect pbrEffect)
            {
                pbrEffect.NormalMode = srcMaterial.DoubleSided ? GeometryNormalMode.DoubleSided : GeometryNormalMode.Reverse;
            }

            return effect;
        }        

        private AnimatedEffect _CreatePBREffect(MaterialContentConverter srcMaterial)
        {
            if (srcMaterial.PreferredShading == "Unlit") throw new ArgumentException(nameof(srcMaterial));

            PBREffect effect;

            if (srcMaterial.PreferredShading == "SpecularGlossiness")
            {
                var xeffect = new PBRSpecularGlossinessEffect(this.Device);
                effect = xeffect;

                srcMaterial.CopyToEffect(xeffect.DiffuseMap, "Diffuse", XNAV4.One);
                srcMaterial.CopyToEffect(xeffect.SpecularGlossinessMap, "SpecularGlossiness", XNAV4.Zero);
            }
            else
            {
                var xeffect = new PBRMetallicRoughnessEffect(this.Device);
                effect = xeffect;

                srcMaterial.CopyToEffect(xeffect.BaseColorMap, "BaseColor", XNAV4.One);
                srcMaterial.CopyToEffect(xeffect.MetalRoughnessMap, "MetallicRoughness", XNAV2.One);
            }

            srcMaterial.CopyToEffect(effect.NormalMap, "Normal", 1);
            srcMaterial.CopyToEffect(effect.EmissiveMap, "Emissive", XNAV3.Zero);
            srcMaterial.CopyToEffect(effect.OcclusionMap, "Occlusion", 0);
            if (effect.OcclusionMap.Texture == null) effect.OcclusionMap.Scale = 0;

            return effect;
        }

        private AnimatedEffect _CreateUnlitEffect(MaterialContentConverter srcMaterial)
        {
            var ueffect = new UnlitEffect(this.Device);

            srcMaterial.CopyToEffect(ueffect.BaseColorMap, "BaseColor", XNAV4.One);
            srcMaterial.CopyToEffect(ueffect.EmissiveMap, "Emissive", XNAV3.Zero);
            srcMaterial.CopyToEffect(ueffect.OcclusionMap, "Occlusion", 0);
            if (ueffect.OcclusionMap.Texture == null) ueffect.OcclusionMap.Scale = 0;

            return ueffect;
        }
    }    
}
