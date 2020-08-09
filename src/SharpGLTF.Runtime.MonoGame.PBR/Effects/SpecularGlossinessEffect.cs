using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SharpGLTF.Runtime.Effects
{
    public class SpecularGlossinessEffect : PBREffect
    {
        #region lifecycle

        /// <summary>
        /// Creates a new AlphaTestEffect with default parameter settings.
        /// </summary>
        public SpecularGlossinessEffect(GraphicsDevice device) : base(device, Resources.GetShaderByteCode("SpecularGlossinessEffect"))
        {

        }

        #endregion

        #region data       

        private Vector4 _DiffuseScale = Vector4.One;
        private Texture2D _DiffuseMap;

        private Vector4 _SpecularGlossinessScale = Vector4.One;
        private Texture2D _SpecularGlossinessMap;

        #endregion

        #region properties - material

        public Vector4 DiffuseScale { get => _DiffuseScale; set => _DiffuseScale = value; }

        public Texture2D DiffuseMap { get => _DiffuseMap; set => _DiffuseMap = value; }

        public Vector4 SpecularGlossinessScale { get => _SpecularGlossinessScale; set => _SpecularGlossinessScale = value; }

        public Texture2D SpecularGlossinessMap { get => _SpecularGlossinessMap; set => _SpecularGlossinessMap = value; }

        #endregion

        #region API

        protected override void OnApply()
        {
            base.OnApply();

            var shaderIndex = RecalculateAll();

            CurrentTechnique = Techniques[shaderIndex];

            ApplyPBR();

            Parameters["PrimaryScale"].SetValue(_DiffuseScale);
            UseTexture("PrimaryTexture", _DiffuseMap ?? Resources.WhiteDotTexture);

            Parameters["SecondaryScale"].SetValue(_SpecularGlossinessScale);
            UseTexture("SecondaryTexture", _SpecularGlossinessMap ?? Resources.WhiteDotTexture);            
        }

        private int RecalculateAll()
        {
            int techniqueIndex = 0;
            if (BoneCount != 0) techniqueIndex += 1;

            if (NormalMap != null)
                techniqueIndex += 2;

            return techniqueIndex;
        }

        #endregion
    }
}
