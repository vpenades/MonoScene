using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    public abstract class EffectTexture2D
    {
        #region lifecycle

        static EffectTexture2D()
        {
            _IsOpenGL = Resources.GetShaderProfile() == "ogl";
        }

        private static readonly bool _IsOpenGL;

        internal EffectTexture2D(GraphicsDevice gd, EffectParameterCollection parameters, string name, int samplerIdx)
        {
            _Device = gd;

            var texName = name + "Texture";
            if (_IsOpenGL) texName = texName + "Sampler+" + texName;
            _TextureMap = parameters[texName];
            _SamplerIndex = samplerIdx;

            _TextureScale = parameters[name + "Scale"];
            _TextureSet = parameters[name + "TextureIdx"];
            _TextureTransformU = parameters[name + "TransformU"];
            _TextureTransformV = parameters[name + "TransformV"];
        }

        #endregion

        #region data

        private GraphicsDevice _Device;

        private EffectParameter _TextureMap;
        internal EffectParameter _TextureScale;
        internal EffectParameter _TextureSet;
        internal EffectParameter _TextureTransformU;
        internal EffectParameter _TextureTransformV;

        private Texture2D _Texture;

        private int _SamplerIndex;
        private SamplerState _Sampler = SamplerState.LinearWrap;

        private Vector4 _Scalar;

        private Int32 _TextureSetIndex;
        private Vector3 _TransformU = Vector3.UnitX;
        private Vector3 _TransformV = Vector3.UnitY;        

        #endregion

        #region public

        public Texture2D Texture
        {
            get => _Texture;
            set => _Texture = value;
        }
        
        public SamplerState Sampler
        {
            get => _Sampler;
            set => _Sampler = value;
        }

        public int SetIndex
        {
            get => _TextureSetIndex;
            set => _TextureSetIndex = value;
        }

        public (Vector3, Vector3) Transform
        {
            get => (_TransformU,_TransformV);
            set { _TransformU = value.Item1; _TransformV = value.Item2; }
        }        

        #endregion

        #region API

        internal virtual void Apply()
        {
            if (_TextureMap == null) return;

            _TextureMap.SetValue(_Texture);

            if (_Sampler != null) _Device.SamplerStates[_SamplerIndex] = _Sampler;

            _TextureSet.SetValue(_TextureSetIndex);
            _TextureTransformU.SetValue(_TransformU);
            _TextureTransformV.SetValue(_TransformV);
        }

        #endregion

        #region nested

        public sealed class ScalarX : EffectTexture2D
        {
            internal ScalarX(GraphicsDevice gd, EffectParameterCollection parameters, string name, int samplerIdx) : base(gd, parameters, name, samplerIdx) { }
            public float Scale { get; set; }
            internal override void Apply() { base.Apply(); _TextureScale.SetValue(Scale); }
        }

        public sealed class ScalarXY : EffectTexture2D
        {
            internal ScalarXY(GraphicsDevice gd, EffectParameterCollection parameters, string name, int samplerIdx) : base(gd, parameters, name, samplerIdx) { }
            public Vector2 Scale { get; set; }
            internal override void Apply() { base.Apply(); _TextureScale.SetValue(Scale); }
        }

        public sealed class ScalarXYZ : EffectTexture2D
        {
            internal ScalarXYZ(GraphicsDevice gd, EffectParameterCollection parameters, string name, int samplerIdx) : base(gd, parameters, name, samplerIdx) { }
            public Vector3 Scale { get; set; }
            internal override void Apply() { base.Apply(); _TextureScale.SetValue(Scale); }
        }

        public sealed class ScalarXYZW : EffectTexture2D
        {
            internal ScalarXYZW(GraphicsDevice gd, EffectParameterCollection parameters, string name, int samplerIdx) : base(gd, parameters, name, samplerIdx) { }
            public Vector4 Scale { get; set; }
            internal override void Apply() { base.Apply(); _TextureScale.SetValue(Scale); }
        }

        #endregion
    }

    



}
