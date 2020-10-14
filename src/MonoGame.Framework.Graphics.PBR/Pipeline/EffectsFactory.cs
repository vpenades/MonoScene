using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public static class PBREffectsFactory
    {
        public static Effect CreateClassicEffect(MaterialContent srcMaterial, bool isSkinned, GraphicsDevice device, Converter<Object, Texture2D> texFactory)
        {
            var classicMaterial = new ClassicMaterialContentAdapter(srcMaterial);
            return classicMaterial.CreateEffect(device, isSkinned, texFactory);
        }

        public static AnimatedEffect CreatePBREffect(MaterialContent srcMaterial, bool isSkinned, GraphicsDevice device, Converter<Object, Texture2D> texFactory)
        {
            var pbrMaterial = new PBRMaterialContentAdapter(srcMaterial);
            var effect = CreatePBREffect(pbrMaterial, device, texFactory);            

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

        private static AnimatedEffect CreatePBREffect(PBRMaterialContentAdapter srcMaterial, GraphicsDevice device, Converter<Object, Texture2D> texFactory)
        {
            if (srcMaterial.PreferredShading == "Unlit") return CreateUnlitEffect(srcMaterial, device, texFactory);

            PBREffect effect = null;

            if (srcMaterial.PreferredShading == "SpecularGlossiness")
            {
                var xeffect = new PBRSpecularGlossinessEffect(device);
                effect = xeffect;

                srcMaterial.CopyToEffect(xeffect.DiffuseMap, "Diffuse", Vector4.One, texFactory);
                srcMaterial.CopyToEffect(xeffect.SpecularGlossinessMap, "SpecularGlossiness", Vector4.Zero, texFactory);
            }
            else
            {
                var xeffect = new PBRMetallicRoughnessEffect(device);
                effect = xeffect;

                srcMaterial.CopyToEffect(xeffect.BaseColorMap, "BaseColor", Vector4.One, texFactory);
                srcMaterial.CopyToEffect(xeffect.MetalRoughnessMap, "MetallicRoughness", Vector2.One, texFactory);
            }

            srcMaterial.CopyToEffect(effect.NormalMap, "Normal", 1, texFactory);
            srcMaterial.CopyToEffect(effect.EmissiveMap, "Emissive", Vector3.Zero, texFactory);
            srcMaterial.CopyToEffect(effect.OcclusionMap, "Occlusion", 0, texFactory);
            if (effect.OcclusionMap.Texture == null) effect.OcclusionMap.Scale = 0;

            return effect;
        }

        private static AnimatedEffect CreateUnlitEffect(PBRMaterialContentAdapter srcMaterial, GraphicsDevice device, Converter<Object, Texture2D> texFactory)
        {
            var ueffect = new UnlitEffect(device);

            srcMaterial.CopyToEffect(ueffect.BaseColorMap, "BaseColor", Vector4.One, texFactory);
            srcMaterial.CopyToEffect(ueffect.EmissiveMap, "Emissive", Vector3.Zero, texFactory);
            srcMaterial.CopyToEffect(ueffect.OcclusionMap, "Occlusion", 0, texFactory);
            if (ueffect.OcclusionMap.Texture == null) ueffect.OcclusionMap.Scale = 0;

            return ueffect;
        }
    }

    /// <summary>
    /// Helper object used to convert a <see cref="MaterialContent"/> to a <see cref="BasicEffect"/> or <see cref="SkinnedEffect"/>
    /// </summary>
    readonly struct ClassicMaterialContentAdapter
    {
        #region constructor
        public ClassicMaterialContentAdapter(MaterialContent content)
        {
            _Source = content;
        }

        #endregion

        #region data

        private readonly MaterialContent _Source;

        #endregion

        #region properties

        public MaterialContent Source => _Source;

        public string Name => _Source.Name;

        public bool IsUnlit => _Source.PreferredShading == "Unlit";

        public float AlphaCutoff => _Source.AlphaCutoff;

        public float AlphaLevel
        {
            get
            {
                if (_Source.Mode == MaterialBlendMode.Opaque) return 1;

                var baseColor = _Source.FindChannel("BaseColor");
                if (baseColor == null) baseColor = _Source.FindChannel("Diffuse");
                if (baseColor == null) return 1;

                return baseColor.Value[3];
            }
        }

        public Vector3 DiffuseColor
        {
            get
            {
                var diffuse = _Source.FindChannel("Diffuse");
                if (diffuse == null) diffuse = _Source.FindChannel("BaseColor");
                if (diffuse == null) return Vector3.One;

                return new Vector3(diffuse.Value[0], diffuse.Value[1], diffuse.Value[2]);
            }
        }

        public Vector3 SpecularColor
        {
            get
            {
                if (IsUnlit) return Vector3.Zero;

                var mr = _Source.FindChannel("MetallicRoughness");

                if (mr == null) return Vector3.One; // default value 16

                var diffuse = DiffuseColor;
                var metallic = mr.Value[0];
                var roughness = mr.Value[1];

                var k = Vector3.Zero;
                k += Vector3.Lerp(diffuse, Vector3.Zero, roughness);
                k += Vector3.Lerp(diffuse, Vector3.One, metallic);
                k *= 0.5f;

                return k;
            }
        }

        public Single SpecularPower
        {
            get
            {
                if (IsUnlit) return 16;

                var mr = _Source.FindChannel("MetallicRoughness");

                if (mr == null) return 16; // default value = 16

                var metallic = mr.Value[0];
                var roughness = mr.Value[1];

                return 4 + 16 * metallic;
            }
        }

        public Vector3 EmissiveColor
        {
            get
            {
                if (IsUnlit) return DiffuseColor;

                var emissive = _Source.FindChannel("Emissive");

                if (emissive == null) return Vector3.Zero;

                return new Vector3(emissive.Value[0], emissive.Value[1], emissive.Value[2]);
            }
        }

        public Object DiffuseTexture
        {
            get
            {
                var diffuse = _Source.FindChannel("Diffuse");
                if (diffuse == null) diffuse = _Source.FindChannel("BaseColor");

                return diffuse?.Texture;
            }
        }

        #endregion

        #region API        

        public Effect CreateEffect(GraphicsDevice device, bool mustSupportSkinning, Converter<Object, Texture2D> texFactory)
        {
            return mustSupportSkinning ? CreateSkinnedEffect(device, texFactory) : CreateRigidEffect(device, texFactory);
        }

        private Effect CreateRigidEffect(GraphicsDevice device, Converter<Object, Texture2D> texFactory)
        {
            var dstMaterial = this.Source.Mode == MaterialBlendMode.Mask
                ? CreateAlphaTestEffect(device, texFactory)
                : CreateBasicEffect(device, texFactory);

            return dstMaterial;
        }

        private Effect CreateBasicEffect(GraphicsDevice device, Converter<Object, Texture2D> texFactory)
        {
            var dstMaterial = new BasicEffect(device);

            dstMaterial.Name = this.Name;

            dstMaterial.Name = this.Name;
            dstMaterial.Alpha = this.AlphaLevel;
            dstMaterial.DiffuseColor = this.DiffuseColor;
            dstMaterial.SpecularColor = this.SpecularColor;
            dstMaterial.SpecularPower = this.SpecularPower;
            dstMaterial.EmissiveColor = this.EmissiveColor;

            var dt = this.DiffuseTexture as Byte[];
            dstMaterial.Texture = texFactory(dt);
            dstMaterial.TextureEnabled = dstMaterial.Texture != null;

            dstMaterial.PreferPerPixelLighting = true;

            return dstMaterial;
        }

        private Effect CreateAlphaTestEffect(GraphicsDevice device, Converter<Object, Texture2D> texFactory)
        {
            var dstMaterial = new AlphaTestEffect(device);

            dstMaterial.Name = this.Name;

            dstMaterial.Alpha = this.AlphaLevel;
            //dstMaterial.AlphaFunction = CompareFunction.GreaterEqual;
            dstMaterial.ReferenceAlpha = (int)(this.AlphaCutoff * 255);

            dstMaterial.DiffuseColor = this.DiffuseColor;

            var dt = this.DiffuseTexture as Byte[];
            dstMaterial.Texture = texFactory(dt);

            return dstMaterial;
        }

        private Effect CreateSkinnedEffect(GraphicsDevice device, Converter<Object, Texture2D> texFactory)
        {
            var dstMaterial = new SkinnedEffect(device);

            dstMaterial.Name = this.Name;
            dstMaterial.Alpha = this.AlphaLevel;
            dstMaterial.DiffuseColor = this.DiffuseColor;
            dstMaterial.SpecularColor = this.SpecularColor;
            dstMaterial.SpecularPower = this.SpecularPower;
            dstMaterial.EmissiveColor = this.EmissiveColor;

            // apparently, SkinnedEffect does not support disabling textures, so we set a white texture here.
            if (!(this.DiffuseTexture is Byte[] dt)) dt = DefaultPngImage;
            dstMaterial.Texture = texFactory(dt);

            dstMaterial.WeightsPerVertex = 4;
            dstMaterial.PreferPerPixelLighting = true;

            return dstMaterial;
        }

        private const string DEFAULT_PNG_IMAGE = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAIAAACQkWg2AAAACXBIWXMAAA7DAAAOwwHHb6hkAAAAHXpUWHRUaXRsZQAACJlzSU1LLM0pCUmtKCktSgUAKVIFt/VCuZ8AAAAoelRYdEF1dGhvcgAACJkLy0xOzStJVQhIzUtMSS1WcCzKTc1Lzy8BAG89CQyAoFAQAAAANElEQVQoz2O8cuUKAwxoa2vD2VevXsUqzsRAIqC9Bsb///8TdDey+CD0Awsx7h6NB5prAADPsx0VAB8VRQAAAABJRU5ErkJggg==";

        internal static Byte[] DefaultPngImage => Convert.FromBase64String(DEFAULT_PNG_IMAGE);

        #endregion
    }

    /// <summary>
    /// Helper object used to convert a <see cref="MaterialContent"/> to a <see cref="PBREffect"/>
    /// </summary>
    readonly struct PBRMaterialContentAdapter
    {
        #region constructor
        public PBRMaterialContentAdapter(MaterialContent content)
        {
            _Source = content;
        }

        #endregion

        #region data

        private readonly MaterialContent _Source;

        #endregion

        #region API

        public string PreferredShading => _Source.PreferredShading;

        public void CopyToEffect(EffectTexture2D.Scalar1 dst, string name, float defval, Converter<Object, Texture2D> texFactory)
        {
            dst.Texture = UseChannelTexture(name, texFactory);
            dst.Sampler = UseChannelSampler(name);
            dst.Scale = GetScaler(name, defval);
            dst.SetIndex = GetTextureSet(name);
            dst.Transform = GetTransform(name);
        }

        public void CopyToEffect(EffectTexture2D.Scalar2 dst, string name, Vector2 defval, Converter<Object, Texture2D> texFactory)
        {
            dst.Texture = UseChannelTexture(name, texFactory);
            dst.Sampler = UseChannelSampler(name);
            dst.Scale = GetScaler(name, defval);
            dst.SetIndex = GetTextureSet(name);
            dst.Transform = GetTransform(name);
        }

        public void CopyToEffect(EffectTexture2D.Scalar3 dst, string name, Vector3 defval, Converter<Object, Texture2D> texFactory)
        {
            dst.Texture = UseChannelTexture(name, texFactory);
            dst.Sampler = UseChannelSampler(name);
            dst.Scale = GetScaler(name, defval);
            dst.SetIndex = GetTextureSet(name);
            dst.Transform = GetTransform(name);
        }

        public void CopyToEffect(EffectTexture2D.Scalar4 dst, string name, Vector4 defval, Converter<Object, Texture2D> texFactory)
        {
            dst.Texture = UseChannelTexture(name, texFactory);
            dst.Sampler = UseChannelSampler(name);
            dst.Scale = GetScaler(name, defval);
            dst.SetIndex = GetTextureSet(name);
            dst.Transform = GetTransform(name);
        }

        private (Vector3 u, Vector3 v) GetTransform(string name)
        {
            var channel = _Source.FindChannel(name);
            return channel == null ? (Vector3.UnitX, Vector3.UnitY) : channel.Transform;
        }

        private int GetTextureSet(string name)
        {
            var channel = _Source.FindChannel(name);
            return channel == null ? 0 : channel.VertexIndexSet;
        }

        private float GetScaler(string name, float defval)
        {
            var channel = _Source.FindChannel(name);
            return channel == null ? defval : channel.Value[0];
        }

        private Vector2 GetScaler(string name, Vector2 defval)
        {
            var channel = _Source.FindChannel(name);
            return channel == null ? defval : new Vector2(channel.Value[0], channel.Value[1]);
        }

        private Vector3 GetScaler(string name, Vector3 defval)
        {
            var channel = _Source.FindChannel(name);
            return channel == null ? defval : new Vector3(channel.Value[0], channel.Value[1], channel.Value[2]);
        }

        private Vector4 GetScaler(string name, Vector4 defval)
        {
            var channel = _Source.FindChannel(name);
            return channel == null ? defval : new Vector4(channel.Value[0], channel.Value[1], channel.Value[2], channel.Value[3]);
        }

        private Texture2D UseChannelTexture(string channelName, Converter<Object, Texture2D> texFactory)
        {
            var channel = _Source.FindChannel(channelName);
            if (channel == null) return null;

            if (channel.Texture is Byte[] array) return texFactory(array);

            return null;
        }

        private SamplerState UseChannelSampler(string name)
        {
            var channel = _Source.FindChannel(name);
            if (channel == null) return null;

            var state = channel.Sampler.TryGetPredefinedSampler();
            if (state == null)
            {
                state = channel.Sampler.CreateState();
                //TODO: record for disposing
            }

            return state;
        }

        #endregion
    }
}
