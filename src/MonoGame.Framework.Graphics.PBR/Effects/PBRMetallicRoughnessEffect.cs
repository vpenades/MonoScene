using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    public class PBRMetallicRoughnessEffect : PBREffect
    {
        #region lifecycle
        
        public PBRMetallicRoughnessEffect(GraphicsDevice device) : base(device, Resources.GetShaderByteCode("MetallicRoughnessEffect"))
        {
            _BaseColorMap = new EffectTexture2D.Scalar4(device, this.Parameters, "Primary", 1);
            _MetalRoughnessMap = new EffectTexture2D.Scalar2(device, this.Parameters, "Secondary", 2);
        }

        #endregion

        #region data

        private readonly EffectTexture2D.Scalar4 _BaseColorMap;
        private readonly EffectTexture2D.Scalar2 _MetalRoughnessMap;

        #endregion

        #region properties - material

        public EffectTexture2D.Scalar4 BaseColorMap => _BaseColorMap;
        public EffectTexture2D.Scalar2 MetalRoughnessMap => _MetalRoughnessMap;

        #endregion

        #region API

        protected override void OnApply()
        {
            base.OnApply();
            
            _BaseColorMap.Apply();
            _MetalRoughnessMap.Apply();            

            var shaderIndex = RecalculateAll();
            CurrentTechnique = Techniques[shaderIndex];
        }

        private int RecalculateAll()
        {
            int techniqueIndex = 0;
            if (BoneCount != 0) techniqueIndex += 1;

            if (NormalMap.Texture != null) techniqueIndex += 4;
            if (BaseColorMap.Texture != null) techniqueIndex += 8;
            if (MetalRoughnessMap.Texture != null) techniqueIndex += 16;
            if (EmissiveMap.Texture != null) techniqueIndex += 32;
            if (OcclusionMap.Texture != null) techniqueIndex += 64;

            return techniqueIndex;
        }

        #endregion
    }
}
