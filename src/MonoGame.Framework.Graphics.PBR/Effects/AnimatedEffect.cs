using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    public abstract class AnimatedEffect : Effect, IEffectMatrices, IEffectBones
    {
        #region lifecycle

        static AnimatedEffect()
        {
            _IsOpenGL = Resources.GetShaderProfile() == "ogl";
        }

        private static readonly bool _IsOpenGL;
        
        public AnimatedEffect(GraphicsDevice device, byte[] effectCode) : base(device, effectCode)
        {            
        }

        #endregion

        #region data

        private Matrix _World;
        private Matrix _View;
        private Matrix _Proj;

        private int _BoneCount;
        private readonly Matrix[] _Bones = new Matrix[128];        

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

        #region API

        public void SetBoneTransforms(Matrix[] boneTransforms)
        {
            _BoneCount = boneTransforms?.Length ?? 0; if (_BoneCount == 0) return;
            boneTransforms.CopyTo(_Bones, 0);
        }

        protected void UseTexture(string name, Texture2D tex)
        {
            if (_IsOpenGL) name = name + "Sampler+" + name;

            if (!Parameters.Any(item => item.Name == name)) return;

            Parameters[name].SetValue(tex);
        }

        protected void ApplyTransforms()
        {
            Parameters["World"].SetValue(World);
            Parameters["View"].SetValue(View);
            Parameters["Projection"].SetValue(Projection);
            if (_BoneCount > 0) Parameters["Bones"].SetValue(_Bones);            
        }

        #endregion
    }
}
