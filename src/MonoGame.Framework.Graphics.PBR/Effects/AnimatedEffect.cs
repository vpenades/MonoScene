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

        private Matrix _World = Matrix.Identity;
        private Matrix _View = Matrix.Identity;
        private Matrix _Proj = Matrix.Identity;

        
        private bool _WorldIsMirror = false;

        private int _BoneCount;
        private readonly Matrix[] _Bones = new Matrix[128];

        #endregion

        #region properties - material

        public bool AlphaBlend { get; set; }
        public float AlphaCutoff { get; set; }

        #endregion

        #region properties - transform

        public int MaxBones => _Bones.Length;

        public int BoneCount => _BoneCount;

        public Matrix World
        {
            get => _World;
            set
            {
                _World = value;
                _WorldIsMirror = _World.Determinant() < 0;
            }
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

        // True if world matrix is a mirror matrix and requires the RasterizerState to reverse face culling.
        public bool WorldIsMirror => _WorldIsMirror;

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


        protected override void OnApply()
        {
            base.OnApply();

            Parameters["World"].SetValue(World);
            Parameters["View"].SetValue(View);
            Parameters["Projection"].SetValue(Projection);
            if (_BoneCount > 0) Parameters["Bones"].SetValue(_Bones);

            Parameters["CameraPosition"].SetValue(-View.Translation);

            Parameters["AlphaTransform"].SetValue(AlphaBlend ? Vector2.UnitX : Vector2.UnitY);
            Parameters["AlphaCutoff"].SetValue(AlphaCutoff);
        }
        
        #endregion
    }
}
