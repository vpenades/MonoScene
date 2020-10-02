using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using TRANSFORM = Microsoft.Xna.Framework.Matrix;
using SPARSE8 = Microsoft.Xna.Framework.Vector4;

using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Defines a hierarchical transform node of a scene graph tree.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("[{LogicalNodeIndex}] {Name}")]
    public class NodeTemplate
    {
        #region lifecycle

        internal NodeTemplate(int thisIndex, int parentIndex, int[] childIndices)
        {
            if (parentIndex >= thisIndex) throw new ArgumentOutOfRangeException(nameof(parentIndex));
            if (childIndices.Any(item => item <= thisIndex)) throw new ArgumentOutOfRangeException(nameof(childIndices));

            _ThisIndex = thisIndex;
            _ParentIndex = parentIndex;
            _ChildIndices = childIndices;
        }

        #endregion

        #region data

        /// <summary>
        /// the index of this node within <see cref="ModelTemplate._NodeTemplates"/>
        /// </summary>
        private readonly int _ThisIndex;

        /// <summary>
        /// the index of the parent node within <see cref="ModelTemplate._NodeTemplates"/>
        /// </summary>
        private readonly int _ParentIndex;
        private readonly int[] _ChildIndices;

        private TRANSFORM _LocalMatrix;        

        private bool _UseAnimatedTransforms;

        private AffineTransform _LocalTransform;
        private AnimatableProperty<Vector3> _Scale;
        private AnimatableProperty<Quaternion> _Rotation;
        private AnimatableProperty<Vector3> _Translation;

        // private AnimatableProperty<SPARSE8> _Morphing;

        #endregion

        #region properties

        public string Name { get; set; }

        /// <summary>
        /// Gets the index of the source <see cref="Schema2.Node"/> in <see cref="ModelTemplate._NodeTemplates"/>
        /// </summary>
        public int ThisIndex => _ThisIndex;

        /// <summary>
        /// Gets the index of the parent <see cref="NodeTemplate"/> in <see cref="ModelTemplate._NodeTemplates"/>
        /// </summary>
        public int ParentIndex => _ParentIndex;

        /// <summary>
        /// Gets the list of indices of the children <see cref="NodeTemplate"/> in <see cref="ModelTemplate._NodeTemplates"/>
        /// </summary>
        public IReadOnlyList<int> ChildIndices => _ChildIndices;

        public TRANSFORM LocalMatrix => _LocalMatrix;

        #endregion

        #region API

        public void SetLocalMatrix(TRANSFORM matrix)
        {
            _LocalMatrix = matrix;
            _UseAnimatedTransforms = false;
        }

        public void SetLocalTransform(AnimatableProperty<Vector3> s, AnimatableProperty<Quaternion> r, AnimatableProperty<Vector3> t)
        {            
            var ss = s != null && s.IsAnimated;
            var rr = r != null && r.IsAnimated;
            var tt = t != null && t.IsAnimated;

            if (!(ss || rr || tt))
            {
                _UseAnimatedTransforms = false;
                _Scale = null;
                _Rotation = null;
                _Translation = null;
                return;
            }

            _UseAnimatedTransforms = true;
            _Scale = s;
            _Rotation = r;
            _Translation = t;

            _LocalMatrix = AffineTransform.CreateFromAny(null, s.Value, r.Value, t.Value).Matrix;
        }

        /*
        public SPARSE8 GetMorphWeights(int trackLogicalIndex, float time)
        {
            if (trackLogicalIndex < 0) return _Morphing.Value;

            return _Morphing.GetValueAt(trackLogicalIndex, time);
        }
        
        public SPARSE8 GetMorphWeights(ReadOnlySpan<int> track, ReadOnlySpan<float> time, ReadOnlySpan<float> weight)
        {
            if (!_Morphing.IsAnimated) return _Morphing.Value;

            Span<SPARSE8> xforms = stackalloc SPARSE8[track.Length];

            for (int i = 0; i < xforms.Length; ++i)
            {
                xforms[i] = GetMorphWeights(track[i], time[i]);
            }

            return SPARSE8.Blend(xforms, weight);
        }*/

        public AffineTransform GetLocalTransform(int trackIndex, float time)
        {
            if (!_UseAnimatedTransforms || trackIndex < 0) return _LocalTransform;

            var s = _Scale?.GetValueAt(trackIndex, time);
            var r = _Rotation?.GetValueAt(trackIndex, time);
            var t = _Translation?.GetValueAt(trackIndex, time);

            return new AffineTransform(s, r, t);
        }

        public AffineTransform GetLocalTransform(ReadOnlySpan<int> track, ReadOnlySpan<float> time, ReadOnlySpan<float> weight)
        {
            if (!_UseAnimatedTransforms) return _LocalTransform;

            Span<AffineTransform> xforms = stackalloc AffineTransform[track.Length];

            for (int i = 0; i < xforms.Length; ++i)
            {
                xforms[i] = GetLocalTransform(track[i], time[i]);
            }

            return AffineTransform.Blend(xforms, weight);
        }

        public TRANSFORM GetLocalMatrix(int trackIndex, float time)
        {
            if (!_UseAnimatedTransforms || trackIndex < 0) return _LocalMatrix;

            return GetLocalTransform(trackIndex, time).Matrix;
        }

        public TRANSFORM GetLocalMatrix(ReadOnlySpan<int> track, ReadOnlySpan<float> time, ReadOnlySpan<float> weight)
        {
            if (!_UseAnimatedTransforms) return _LocalMatrix;

            return GetLocalTransform(track, time, weight).Matrix;
        }

        #endregion
    }
}
