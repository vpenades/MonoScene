using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

using MonoScene.Graphics.Content;

using XNAV3 = Microsoft.Xna.Framework.Vector3;

using TEXTURELOOKUP = System.Converter<int, Microsoft.Xna.Framework.Graphics.Texture2D>;

namespace MonoScene.Graphics.Pipeline
{
    /// <summary>
    /// Helper object used to convert a <see cref="MaterialContent"/><br/>
    /// to a <see cref="BasicEffect"/> or <see cref="SkinnedEffect"/>
    /// </summary>
    readonly struct MaterialContentConverter
    {
        #region constructor
        public MaterialContentConverter(MaterialContent content, TEXTURELOOKUP texLookup)
        {
            _MaterialSource = content;
            _TextureSource = texLookup;
        }

        #endregion

        #region data

        private readonly MaterialContent _MaterialSource;
        private readonly TEXTURELOOKUP _TextureSource;

        #endregion

        #region properties

        public MaterialContent Source => _MaterialSource;

        public string Name => _MaterialSource.Name;

        public bool IsUnlit => _MaterialSource.PreferredShading == "Unlit";

        public float AlphaCutoff => _MaterialSource.AlphaCutoff;

        public float AlphaLevel
        {
            get
            {
                if (_MaterialSource.Mode == MaterialBlendMode.Opaque) return 1;

                var baseColor = _MaterialSource.FindChannel("BaseColor");
                if (baseColor == null) baseColor = _MaterialSource.FindChannel("Diffuse");
                if (baseColor == null) return 1;

                return baseColor.Value[3];
            }
        }

        public XNAV3 DiffuseColor
        {
            get
            {
                var diffuse = _MaterialSource.FindChannel("Diffuse");
                if (diffuse == null) diffuse = _MaterialSource.FindChannel("BaseColor");
                if (diffuse == null) return XNAV3.One;

                return new XNAV3(diffuse.Value[0], diffuse.Value[1], diffuse.Value[2]);
            }
        }

        public XNAV3 SpecularColor
        {
            get
            {
                if (IsUnlit) return XNAV3.Zero;

                var mr = _MaterialSource.FindChannel("MetallicRoughness");

                if (mr == null) return XNAV3.One; // default value 16

                var diffuse = DiffuseColor;
                var metallic = mr.Value[0];
                var roughness = mr.Value[1];

                var k = XNAV3.Zero;
                k += XNAV3.Lerp(diffuse, XNAV3.Zero, roughness);
                k += XNAV3.Lerp(diffuse, XNAV3.One, metallic);
                k *= 0.5f;

                return k;
            }
        }

        public Single SpecularPower
        {
            get
            {
                if (IsUnlit) return 16;

                var mr = _MaterialSource.FindChannel("MetallicRoughness");

                if (mr == null) return 16; // default value = 16

                var metallic = mr.Value[0];
                var roughness = mr.Value[1];

                return 4 + 16 * metallic;
            }
        }

        public XNAV3 EmissiveColor
        {
            get
            {
                if (IsUnlit) return DiffuseColor;

                var emissive = _MaterialSource.FindChannel("Emissive");

                if (emissive == null) return XNAV3.Zero;

                return new XNAV3(emissive.Value[0], emissive.Value[1], emissive.Value[2]);
            }
        }

        public Texture2D DiffuseTexture
        {
            get
            {
                var diffuse = _MaterialSource.FindChannel("Diffuse");
                if (diffuse == null) diffuse = _MaterialSource.FindChannel("BaseColor");

                return _TextureSource(diffuse?.TextureIndex ?? -1);
            }
        }

        #endregion

        #region API        

        public Effect CreateEffect(GraphicsDevice device, bool mustSupportSkinning)
        {
            return mustSupportSkinning ? CreateSkinnedEffect(device) : CreateRigidEffect(device);
        }

        private Effect CreateRigidEffect(GraphicsDevice device)
        {
            var dstMaterial = this.Source.Mode == MaterialBlendMode.Mask
                ? CreateAlphaTestEffect(device)
                : CreateBasicEffect(device);

            return dstMaterial;
        }

        private Effect CreateBasicEffect(GraphicsDevice device)
        {
            var dstMaterial = new BasicEffect(device);

            dstMaterial.Name = this.Name;

            dstMaterial.Name = this.Name;
            dstMaterial.Alpha = this.AlphaLevel;
            dstMaterial.DiffuseColor = this.DiffuseColor;
            dstMaterial.SpecularColor = this.SpecularColor;
            dstMaterial.SpecularPower = this.SpecularPower;
            dstMaterial.EmissiveColor = this.EmissiveColor;
            
            dstMaterial.Texture = this.DiffuseTexture;
            dstMaterial.TextureEnabled = dstMaterial.Texture != null;

            dstMaterial.PreferPerPixelLighting = true;

            return dstMaterial;
        }

        private Effect CreateAlphaTestEffect(GraphicsDevice device)
        {
            var dstMaterial = new AlphaTestEffect(device);

            dstMaterial.Name = this.Name;

            dstMaterial.Alpha = this.AlphaLevel;
            //dstMaterial.AlphaFunction = CompareFunction.GreaterEqual;
            dstMaterial.ReferenceAlpha = (int)(this.AlphaCutoff * 255);

            dstMaterial.DiffuseColor = this.DiffuseColor;
            dstMaterial.Texture = this.DiffuseTexture;

            return dstMaterial;
        }

        private Effect CreateSkinnedEffect(GraphicsDevice device)
        {
            var dstMaterial = new SkinnedEffect(device);

            dstMaterial.Name = this.Name;
            dstMaterial.Alpha = this.AlphaLevel;
            dstMaterial.DiffuseColor = this.DiffuseColor;
            dstMaterial.SpecularColor = this.SpecularColor;
            dstMaterial.SpecularPower = this.SpecularPower;
            dstMaterial.EmissiveColor = this.EmissiveColor;

            dstMaterial.Texture = this.DiffuseTexture;

            // apparently, SkinnedEffect does not support disabling textures, so we set a white texture here.
            if (dstMaterial.Texture == null) dstMaterial.Texture = _TextureSource(65536*256);

            dstMaterial.WeightsPerVertex = 4;
            dstMaterial.PreferPerPixelLighting = true;

            return dstMaterial;
        }        

        #endregion
    }    
}
