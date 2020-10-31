using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    public class PBRSpecularGlossinessEffect : PBREffect
    {
        #region lifecycle
        
        public PBRSpecularGlossinessEffect(GraphicsDevice device) : base(device, Resources.GetShaderByteCode("SpecularGlossinessEffect"))
        {
            _DiffuseMap = new EffectTexture2D.Scalar4(device, this.Parameters, "Primary", 1);
            _SpecularGlossinessMap = new EffectTexture2D.Scalar4(device, this.Parameters, "Secondary", 2);
        }

        #endregion

        #region data

        private readonly EffectTexture2D.Scalar4 _DiffuseMap;
        private readonly EffectTexture2D.Scalar4 _SpecularGlossinessMap;

        #endregion

        #region properties - material

        public EffectTexture2D.Scalar4 DiffuseMap => _DiffuseMap;
        public EffectTexture2D.Scalar4 SpecularGlossinessMap => _SpecularGlossinessMap;

        #endregion

        #region API

        protected override void OnApply()
        {
            base.OnApply();
            
            _DiffuseMap.Apply();
            _SpecularGlossinessMap.Apply();            

            var pbrTechnique = new PBRTechniqueIndexOld(BoneCount, NormalMap, DiffuseMap, SpecularGlossinessMap, EmissiveMap, OcclusionMap);
            CurrentTechnique = Techniques[pbrTechnique.Index];
        }        

        #endregion
    }
}
