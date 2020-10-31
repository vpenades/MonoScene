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

            var techniqueIdx = new UnlitTechniqueOld(BoneCount, _BaseColorMap, EmissiveMap, OcclusionMap);

            CurrentTechnique = Techniques[techniqueIdx.Index];
        }        

        #endregion
    }

    readonly struct UnlitTechnique
    {
        public UnlitTechnique(int BoneCount, EffectTexture2D diffuse, EffectTexture2D emissive, EffectTexture2D opacity)
        {
            bool hasSkin = BoneCount > 0;

            bool hasDiffuse = diffuse.Texture != null;
            bool hasEmissive = emissive.Texture != null;
            bool hasOpacity = opacity.Texture != null;

            var uvsets = EffectTexture2D.GetMinimumVertexUVSets(diffuse, emissive, opacity);

            int vrtMats = 0; // 0=Color, 1=UV0, 2=Color+UV0, 3=Color+UV0+UV1
            if (uvsets == 1) vrtMats = 2;
            if (uvsets == 2) vrtMats = 3;

            Index = (hasSkin ? 1 : 0)
                + vrtMats * 2
                + (hasDiffuse ? 8 : 0)
                + (hasEmissive ? 16 : 0)
                + (hasOpacity ? 32 : 0);
        }

        public readonly int Index;
    }

    readonly struct UnlitTechniqueOld
    {
        public UnlitTechniqueOld(int BoneCount, EffectTexture2D diffuse, EffectTexture2D emissive, EffectTexture2D opacity)
        {
            bool hasSkin = BoneCount > 0;
            bool hasDiffuse = diffuse.Texture != null;
            bool hasEmissive = emissive.Texture != null;
            bool hasOpacity = opacity.Texture != null;            

            Index = (hasSkin ? 1 : 0)
                // 2 was reserved for morphing
                + (hasDiffuse ? 4 : 0)
                + (hasEmissive ? 8 : 0)
                + (hasOpacity ? 16 : 0);
        }

        public readonly int Index;
    }
}
