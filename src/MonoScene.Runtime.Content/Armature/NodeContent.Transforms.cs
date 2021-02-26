using System;
using System.Collections.Generic;
using System.Text;

using XNAV3 = Microsoft.Xna.Framework.Vector3;
using XNAMAT = Microsoft.Xna.Framework.Matrix;
using XNAQUAT = Microsoft.Xna.Framework.Quaternion;
using SPARSE8 = Microsoft.Xna.Framework.Vector4;

namespace MonoScene.Graphics.Content
{
    partial class NodeContent
    {
        public void SetLocalMatrix(XNAMAT matrix)
        {
            _LocalMatrix = matrix;
            _UseAnimatedTransforms = false;
        }

        public void SetLocalTransform(AnimatableProperty<XNAV3> s, AnimatableProperty<XNAQUAT> r, AnimatableProperty<XNAV3> t)
        {
            var ss = s != null && s.IsAnimated;
            var rr = r != null && r.IsAnimated;
            var tt = t != null && t.IsAnimated;

            if (!(ss || rr || tt))
            {
                _UseAnimatedTransforms = false;
                _LocalScale = null;
                _LocalRotation = null;
                _LocalTranslation = null;
                return;
            }

            _UseAnimatedTransforms = true;
            _LocalScale = s;
            _LocalRotation = r;
            _LocalTranslation = t;

            var m = XNAMAT.Identity;
            if (s != null) m *= XNAMAT.CreateScale(s.Value);
            if (r != null) m *= XNAMAT.CreateFromQuaternion(r.Value);
            if (t != null) m.Translation = t.Value;
            _LocalMatrix = m;
        }

        public AffineTransform GetLocalTransform()
        {
            var s = this.LocalScale?.Value;
            var r = this.LocalRotation?.Value;
            var t = this.LocalTranslation?.Value;

            return new AffineTransform(s, r, t);
        }

        public AffineTransform GetLocalTransform(int trackIndex, float time)
        {
            if (!this.UseAnimatedTransforms || trackIndex < 0) return this.GetLocalTransform();

            var s = this.LocalScale?.GetValueAt(trackIndex, time);
            var r = this.LocalRotation?.GetValueAt(trackIndex, time);
            var t = this.LocalTranslation?.GetValueAt(trackIndex, time);

            return new AffineTransform(s, r, t);
        }

        public AffineTransform GetLocalTransform(ReadOnlySpan<int> track, ReadOnlySpan<float> time, ReadOnlySpan<float> weight)
        {
            if (!this.UseAnimatedTransforms) return this.GetLocalTransform();

            Span<AffineTransform> xforms = stackalloc AffineTransform[track.Length];

            for (int i = 0; i < xforms.Length; ++i)
            {
                xforms[i] = this.GetLocalTransform(track[i], time[i]);
            }

            return AffineTransform.Blend(xforms, weight);
        }

        public XNAMAT GetLocalMatrix(int trackIndex, float time)
        {
            if (!this.UseAnimatedTransforms || trackIndex < 0) return this.LocalMatrix;

            return this.GetLocalTransform(trackIndex, time).Matrix;
        }

        public XNAMAT GetLocalMatrix(ReadOnlySpan<int> track, ReadOnlySpan<float> time, ReadOnlySpan<float> weight)
        {
            if (!this.UseAnimatedTransforms) return this.LocalMatrix;

            return this.GetLocalTransform(track, time, weight).Matrix;
        }

        /*
        public SPARSE8 GetMorphWeights(int trackLogicalIndex, float time)
        {
            if (trackLogicalIndex < 0) return _Morphing.Value;

            return _Morphing.GetValueAt(trackLogicalIndex, time);
        }
        
        public SPARSE8 GetMorphWeights(ReadOnlySpan<int> track, ReadOnlySpan<float> time, ReadOnlySpan<float> weight)
        {
            if (!this._Morphing.IsAnimated) return _Morphing.Value;

            Span<SPARSE8> xforms = stackalloc SPARSE8[track.Length];

            for (int i = 0; i < xforms.Length; ++i)
            {
                xforms[i] = GetMorphWeights(track[i], time[i]);
            }

            return SPARSE8.Blend(xforms, weight);
        }*/
    }
}
