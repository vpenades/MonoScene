using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    public class SpecularGlossinessEffect : PBREffect
    {
        #region lifecycle
        
        public SpecularGlossinessEffect(GraphicsDevice device) : base(device, Resources.GetShaderByteCode("SpecularGlossinessEffect"))
        {
            _DiffuseMap = new EffectTexture2D.ScalarXYZW(device, this.Parameters, "Primary", 1);
            _SpecularGlossinessMap = new EffectTexture2D.ScalarXYZW(device, this.Parameters, "Secondary", 2);
        }

        #endregion

        #region data

        private readonly EffectTexture2D.ScalarXYZW _DiffuseMap;
        private readonly EffectTexture2D.ScalarXYZW _SpecularGlossinessMap;

        #endregion

        #region properties - material

        public EffectTexture2D.ScalarXYZW DiffuseMap => _DiffuseMap;
        public EffectTexture2D.ScalarXYZW SpecularGlossinessMap => _SpecularGlossinessMap;

        #endregion

        #region API

        protected override void OnApply()
        {
            base.OnApply();            

            ApplyPBR();

            _DiffuseMap.Apply();
            _SpecularGlossinessMap.Apply();            

            var shaderIndex = RecalculateAll();
            CurrentTechnique = Techniques[shaderIndex];
        }

        private int RecalculateAll()
        {
            int techniqueIndex = 0;
            if (BoneCount != 0) techniqueIndex += 1;

            if (NormalMap.Texture != null) techniqueIndex += 4;
            if (DiffuseMap.Texture != null) techniqueIndex += 8;
            if (SpecularGlossinessMap.Texture != null) techniqueIndex += 16;
            if (EmissiveMap.Texture != null) techniqueIndex += 32;
            if (OcclusionMap.Texture != null) techniqueIndex += 64;

            return techniqueIndex;
        }

        #endregion
    }
}
