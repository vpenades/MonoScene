using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    
    public abstract class EffectTexture2D
    {
        #region

        protected virtual string DebuggerDisplay()
        {
            var txt = string.Empty;

            if (_Texture != null) txt += $"UVSet{_TextureSetIndex}-Tex({_Texture.Width},{_Texture.Height})";

            return txt;
        }

        #endregion

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
        
        private Int32 _TextureSetIndex;

        // this should be defined as a Matrix3x2, but the type is missing in monogame, so...
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

        // this should be defined as a Matrix3x2, but the type is missing in monogame, so...
        public (Vector3 U, Vector3 V) Transform
        {
            get => (_TransformU,_TransformV);
            set { _TransformU = value.U; _TransformV = value.V; }
        }        

        #endregion

        #region API

        internal static int GetMinimumVertexUVSets(EffectTexture2D a, EffectTexture2D b, EffectTexture2D c)
        {
            int count = 0;
            count = Math.Max(count, a.Texture == null ? 0 : a.SetIndex);
            count = Math.Max(count, b.Texture == null ? 0 : b.SetIndex);
            count = Math.Max(count, c.Texture == null ? 0 : c.SetIndex);
            return count;
        }

        internal static int GetMinimumVertexUVSets(EffectTexture2D a, EffectTexture2D b, EffectTexture2D c, EffectTexture2D d, EffectTexture2D e)
        {
            int count = 0;
            count = Math.Max(count, a.Texture == null ? 0 : a.SetIndex);
            count = Math.Max(count, b.Texture == null ? 0 : b.SetIndex);
            count = Math.Max(count, c.Texture == null ? 0 : c.SetIndex);
            count = Math.Max(count, d.Texture == null ? 0 : d.SetIndex);
            count = Math.Max(count, e.Texture == null ? 0 : e.SetIndex);
            return count;
        }

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

        [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay(),nq}")]
        public sealed class Scalar1 : EffectTexture2D
        {
            protected override string DebuggerDisplay() { return base.DebuggerDisplay() + $" x {Scale}"; }

            internal Scalar1(GraphicsDevice gd, EffectParameterCollection parameters, string name, int samplerIdx) : base(gd, parameters, name, samplerIdx) { }
            public float Scale { get; set; }
            internal override void Apply() { base.Apply(); _TextureScale.SetValue(Scale); }
        }

        [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay(),nq}")]
        public sealed class Scalar2 : EffectTexture2D
        {
            protected override string DebuggerDisplay() { return base.DebuggerDisplay() + $" x <{Scale.X},{Scale.Y}>"; }

            internal Scalar2(GraphicsDevice gd, EffectParameterCollection parameters, string name, int samplerIdx) : base(gd, parameters, name, samplerIdx) { }
            public Vector2 Scale { get; set; }
            internal override void Apply() { base.Apply(); _TextureScale.SetValue(Scale); }
        }

        [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay(),nq}")]
        public sealed class Scalar3 : EffectTexture2D
        {
            protected override string DebuggerDisplay() { return base.DebuggerDisplay() + $" x <{Scale.X},{Scale.Y},{Scale.Z}>"; }
            internal Scalar3(GraphicsDevice gd, EffectParameterCollection parameters, string name, int samplerIdx) : base(gd, parameters, name, samplerIdx) { }
            public Vector3 Scale { get; set; }
            internal override void Apply() { base.Apply(); _TextureScale.SetValue(Scale); }
        }

        [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay(),nq}")]
        public sealed class Scalar4 : EffectTexture2D
        {
            protected override string DebuggerDisplay() { return base.DebuggerDisplay() + $" x <{Scale.X},{Scale.Y},{Scale.Z},{Scale.W}>"; }
            internal Scalar4(GraphicsDevice gd, EffectParameterCollection parameters, string name, int samplerIdx) : base(gd, parameters, name, samplerIdx) { }
            public Vector4 Scale { get; set; }
            internal override void Apply() { base.Apply(); _TextureScale.SetValue(Scale); }
        }

        #endregion
    }
}
