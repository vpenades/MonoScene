using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics.Effects
{
    public class UnlitEffect : AnimatedEffect
    {
        #region lifecycle
        
        /// <summary>
        /// Creates a new AlphaTestEffect with default parameter settings.
        /// </summary>
        public UnlitEffect(GraphicsDevice device) : base(device, Resources.GetShaderByteCode("Unlit"))
        {
            _BaseColorMap = new EffectTexture2D.ScalarXYZW(this.Parameters, "Primary");
            _EmissiveMap = new EffectTexture2D.ScalarXYZ(this.Parameters, "Emissive");
            _OcclusionMap = new EffectTexture2D.ScalarX(this.Parameters, "Occlusion");
        }

        #endregion

        #region data

        private readonly EffectTexture2D.ScalarXYZW _BaseColorMap;
        private readonly EffectTexture2D.ScalarXYZ _EmissiveMap;
        private readonly EffectTexture2D.ScalarX _OcclusionMap;

        #endregion        

        #region properties - lights

        public float Exposure { get; set; } = 1;

        #endregion

        #region properties - material         

        public EffectTexture2D.ScalarXYZW BaseColorMap => _BaseColorMap;
        public EffectTexture2D.ScalarXYZ EmissiveMap => _EmissiveMap;
        public EffectTexture2D.ScalarX OcclusionMap => _OcclusionMap;

        #endregion

        #region API        

        protected void ApplyUnlit()
        {
            this.ApplyTransforms();

            Resources.GenerateDotTextures(this.GraphicsDevice);

            Parameters["Exposure"].SetValue(this.Exposure);

            _BaseColorMap.Apply();
            _EmissiveMap.Apply();
            _OcclusionMap.Apply();

            // GraphicsDevice.BlendState = BlendState.Opaque;
        }

        protected override void OnApply()
        {
            base.OnApply();

            ApplyUnlit();            

            var shaderIndex = RecalculateAll();

            CurrentTechnique = Techniques[shaderIndex];
        }

        private int RecalculateAll()
        {
            int techniqueIndex = 0;
            if (BoneCount != 0) techniqueIndex += 1;
            
            if (_BaseColorMap != null) techniqueIndex += 4;            
            if (EmissiveMap != null) techniqueIndex += 8;
            if (OcclusionMap != null) techniqueIndex += 16;

            return techniqueIndex;
        }

        #endregion
    }
}
