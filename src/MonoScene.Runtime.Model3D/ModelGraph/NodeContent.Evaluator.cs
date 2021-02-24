using System;
using System.Collections.Generic;
using System.Text;

using XNAV3 = Microsoft.Xna.Framework.Vector3;
using XNAMAT = Microsoft.Xna.Framework.Matrix;
using XNAQUAT = Microsoft.Xna.Framework.Quaternion;
using SPARSE8 = Microsoft.Xna.Framework.Vector4;

namespace MonoScene.Graphics.Content
{
    static class NodeTemplateEvaluator
    {
        public static AffineTransform GetLocalTransform(this NodeContent node)
        {
            var s = node.LocalScale?.Value;
            var r = node.LocalRotation?.Value;
            var t = node.LocalTranslation?.Value;

            return new AffineTransform(s, r, t);
        }

        public static AffineTransform GetLocalTransform(this NodeContent node, int trackIndex, float time)
        {
            if (!node.UseAnimatedTransforms || trackIndex < 0) return node.GetLocalTransform();

            var s = node.LocalScale?.GetValueAt(trackIndex, time);
            var r = node.LocalRotation?.GetValueAt(trackIndex, time);
            var t = node.LocalTranslation?.GetValueAt(trackIndex, time);

            return new AffineTransform(s, r, t);
        }

        public static AffineTransform GetLocalTransform(this NodeContent node, ReadOnlySpan<int> track, ReadOnlySpan<float> time, ReadOnlySpan<float> weight)
        {
            if (!node.UseAnimatedTransforms) return node.GetLocalTransform();

            Span<AffineTransform> xforms = stackalloc AffineTransform[track.Length];

            for (int i = 0; i < xforms.Length; ++i)
            {
                xforms[i] = node.GetLocalTransform(track[i], time[i]);
            }

            return AffineTransform.Blend(xforms, weight);
        }

        public static XNAMAT GetLocalMatrix(this NodeContent node, int trackIndex, float time)
        {
            if (!node.UseAnimatedTransforms || trackIndex < 0) return node.LocalMatrix;

            return node.GetLocalTransform(trackIndex, time).Matrix;
        }

        public static XNAMAT GetLocalMatrix(this NodeContent node, ReadOnlySpan<int> track, ReadOnlySpan<float> time, ReadOnlySpan<float> weight)
        {
            if (!node.UseAnimatedTransforms) return node.LocalMatrix;

            return node.GetLocalTransform(track, time, weight).Matrix;
        }

        /*
        public static SPARSE8 GetMorphWeights(this NodeContent node,int trackLogicalIndex, float time)
        {
            if (trackLogicalIndex < 0) return _Morphing.Value;

            return _Morphing.GetValueAt(trackLogicalIndex, time);
        }
        
        public static SPARSE8 GetMorphWeights(this NodeContent node,ReadOnlySpan<int> track, ReadOnlySpan<float> time, ReadOnlySpan<float> weight)
        {
            if (!_Morphing.IsAnimated) return _Morphing.Value;

            Span<SPARSE8> xforms = stackalloc SPARSE8[track.Length];

            for (int i = 0; i < xforms.Length; ++i)
            {
                xforms[i] = GetMorphWeights(track[i], time[i]);
            }

            return SPARSE8.Blend(xforms, weight);
        }*/
    }
}
