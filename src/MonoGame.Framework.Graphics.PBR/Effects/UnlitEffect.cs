using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    public class UnlitEffect : AnimatedEffect , IEffectFog
    {
        #region lifecycle
        
        /// <summary>
        /// Creates a new AlphaTestEffect with default parameter settings.
        /// </summary>
        public UnlitEffect(GraphicsDevice device) : base(device, Resources.GetShaderByteCode("Unlit"))
        {
            _BaseColorMap = new EffectTexture2D.Scalar4(device, this.Parameters, "Primary", 1);
            _EmissiveMap = new EffectTexture2D.Scalar3(device, this.Parameters, "Emissive", 3);
            _OcclusionMap = new EffectTexture2D.Scalar1(device, this.Parameters, "Occlusion", 4);

            _Fog = new EffectBasicFog(device, this.Parameters);
        }

        #endregion

        #region data

        private readonly EffectTexture2D.Scalar4 _BaseColorMap;
        private readonly EffectTexture2D.Scalar3 _EmissiveMap;
        private readonly EffectTexture2D.Scalar1 _OcclusionMap;

        private readonly EffectBasicFog _Fog;

        #endregion        

        #region properties - lights

        public float Exposure { get; set; } = 1;

        #endregion

        #region properties - material         

        public EffectTexture2D.Scalar4 BaseColorMap => _BaseColorMap;
        public EffectTexture2D.Scalar3 EmissiveMap => _EmissiveMap;
        public EffectTexture2D.Scalar1 OcclusionMap => _OcclusionMap;

        #endregion

        #region properties - fog
        public Vector3 FogColor { get => _Fog.FogColor; set => _Fog.FogColor = value; }
        public bool FogEnabled { get => _Fog.FogEnabled; set => _Fog.FogEnabled = value; }
        public float FogEnd { get => _Fog.FogEnd; set => _Fog.FogEnd = value; }
        public float FogStart { get => _Fog.FogStart; set => _Fog.FogStart = value; }
        #endregion

        #region API

        protected override void OnApply()
        {
            base.OnApply();

            Parameters["Exposure"].SetValue(this.Exposure);

            _Fog.Apply();

            Resources.GenerateDotTextures(this.GraphicsDevice); // temporary hack

            _BaseColorMap.Apply();
            _EmissiveMap.Apply();
            _OcclusionMap.Apply();

            var shaderIndex = RecalculateAll();

            CurrentTechnique = Techniques[shaderIndex];
        }

        private int RecalculateAll()
        {
            int techniqueIndex = 0;
            if (BoneCount != 0) techniqueIndex += 1;
            
            if (_BaseColorMap.Texture != null) techniqueIndex += 4;            
            if (EmissiveMap.Texture != null) techniqueIndex += 8;
            if (OcclusionMap.Texture != null) techniqueIndex += 16;

            return techniqueIndex;
        }

        #endregion
    }
}
