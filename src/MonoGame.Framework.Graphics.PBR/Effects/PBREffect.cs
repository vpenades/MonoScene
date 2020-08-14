using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Graphics
{
    public abstract class PBREffect : Effect, IEffectMatrices, IEffectBones, PBRLight.IEffect
    {
        #region lifecycle

        static PBREffect()
        {
            _IsOpenGL = Resources.GetShaderProfile() == "ogl";
        }

        private static readonly bool _IsOpenGL;

        /// <summary>
        /// Creates a new AlphaTestEffect with default parameter settings.
        /// </summary>
        public PBREffect(GraphicsDevice device, byte[] effectCode) : base(device, effectCode)
        {
            // _NormalSampler = SamplerState.LinearWrap;
        }        

        #endregion        

        #region data

        private Matrix _World;
        private Matrix _View;
        private Matrix _Proj;

        private int _BoneCount;
        private readonly Matrix[] _Bones = new Matrix[128];

        private readonly PBRLight[] _Lights = new PBRLight[3];
        private readonly Vector4[] _LightParams0 = new Vector4[3];
        private readonly Vector4[] _LightParams1 = new Vector4[3];
        private readonly Vector4[] _LightParams2 = new Vector4[3];
        private readonly Vector4[] _LightParams3 = new Vector4[3];        

        private float _NormalScale = 1;
        private Texture2D _NormalMap;
        private SamplerState _NormalSampler;

        private Vector3 _EmissiveScale = Vector3.Zero;
        private Texture2D _EmissiveMap;

        private float _OcclusionScale = 1;
        private Texture2D _OcclusionMap; // this is the AO map

        #endregion

        #region properties - transform

        public int MaxBones => _Bones.Length;

        public int BoneCount => _BoneCount;

        public Matrix World
        {
            get => _World;
            set { _World = value; }
        }

        public Matrix View
        {
            get => _View;
            set { _View = value; }
        }

        public Matrix Projection
        {
            get => _Proj;
            set { _Proj = value; }
        }

        #endregion

        #region properties - lights

        public float Exposure { get; set; } = 1;

        public bool LightingEnabled { get; set; }

        public Vector3 AmbientLightColor { get; set; }

        public PBRLight GetLight(int index) => _Lights[index];

        public void SetLight(int index, PBRLight light) => _Lights[index] = light;

        #endregion

        #region properties - material        

        public float NormalScale { get => _NormalScale; set => _NormalScale = value; }
        public Texture2D NormalMap { get => _NormalMap; set => _NormalMap = value; }

        public SamplerState NormalSampler { get => _NormalSampler; set => _NormalSampler = value; }

        public Vector3 EmissiveScale { get => _EmissiveScale; set => _EmissiveScale = value; }
        public Texture2D EmissiveMap { get => _EmissiveMap; set => _EmissiveMap = value; }

        public float OcclusionScale { get => _OcclusionScale; set => _OcclusionScale = value; }
        public Texture2D OcclusionMap { get => _OcclusionMap; set => _OcclusionMap = value; }

        #endregion

        #region API

        public void EnableDefaultLighting() { }

        // Note: setting boneCount to 0 disables Skinning
        public void SetBoneTransforms(Matrix[] boneTransforms, int boneStart, int boneCount)
        {
            _BoneCount = boneCount; if (_BoneCount == 0) return;
            Array.Copy(boneTransforms, boneStart, _Bones, 0, boneCount);
        }


        protected void UseTexture(string name, Texture2D tex)
        {
            if (_IsOpenGL) name = name + "Sampler+" + name;

            if (!Parameters.Any(item => item.Name == name)) return;

            Parameters[name].SetValue(tex);
        }

        protected void ApplyPBR()
        {
            Parameters["World"].SetValue(World);
            Parameters["View"].SetValue(View);
            Parameters["Projection"].SetValue(Projection);
            if (_BoneCount > 0) Parameters["Bones"].SetValue(_Bones);

            Parameters["CameraPosition"].SetValue(-View.Translation);

            Resources.GenerateDotTextures(this.GraphicsDevice);

            Parameters["Exposure"].SetValue(this.Exposure);

            PBRLight.Encode(_Lights, _LightParams0, _LightParams1, _LightParams2, _LightParams3);
            Parameters["LightParam0"].SetValue(_LightParams0);
            Parameters["LightParam1"].SetValue(_LightParams1);
            Parameters["LightParam2"].SetValue(_LightParams2);
            Parameters["LightParam3"].SetValue(_LightParams3);
            
            Parameters["NormalScale"].SetValue(_NormalScale);
            UseTexture("NormalTexture", _NormalMap ?? Resources.WhiteDotTexture);
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            
            Parameters["OcclusionScale"].SetValue(_OcclusionMap == null ? 0f : _OcclusionScale);
            UseTexture("OcclusionTexture", _OcclusionMap ?? Resources.WhiteDotTexture);
            GraphicsDevice.SamplerStates[3] = SamplerState.LinearWrap;

            Parameters["EmissiveScale"].SetValue(_EmissiveScale);            
            UseTexture("EmissiveTexture", _EmissiveMap ?? Resources.WhiteDotTexture);
            GraphicsDevice.SamplerStates[4] = SamplerState.LinearWrap;

            // GraphicsDevice.BlendState = BlendState.Opaque;
        }        

        #endregion
    }

    
}
