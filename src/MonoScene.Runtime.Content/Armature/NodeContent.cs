using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using XNAV3 = Microsoft.Xna.Framework.Vector3;
using XNAMAT = Microsoft.Xna.Framework.Matrix;
using XNAQUAT = Microsoft.Xna.Framework.Quaternion;
using SPARSE8 = Microsoft.Xna.Framework.Vector4;

namespace MonoScene.Graphics.Content
{
    /// <summary>
    /// Represents a hierarchical element within a <see cref="ArmatureContent"/>.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("[{LogicalNodeIndex}] {Name}")]
    public class NodeContent : BaseContent
    {
        #region lifecycle

        public NodeContent(int thisIndex, int parentIndex, int[] childIndices)
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

        private XNAMAT _LocalMatrix;

        private bool _UseAnimatedTransforms;        

        private AnimatableProperty<XNAV3> _LocalScale;
        private AnimatableProperty<XNAQUAT> _LocalRotation;
        private AnimatableProperty<XNAV3> _LocalTranslation;
        // private AnimatableProperty<SPARSE8> _LocalMorphing;

        #endregion

        #region properties

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

        /// <summary>
        /// If true, use <see cref="LocalScale"/>, <see cref="LocalRotation"/> and <see cref="LocalTranslation"/><br/>
        /// instead of <see cref="LocalMatrix"/>.
        /// </summary>
        public bool UseAnimatedTransforms => _UseAnimatedTransforms;
        public XNAMAT LocalMatrix => _LocalMatrix;        
        public AnimatableProperty<XNAV3> LocalScale => _LocalScale;
        public AnimatableProperty<XNAQUAT> LocalRotation => _LocalRotation;
        public AnimatableProperty<XNAV3> LocalTranslation => _LocalTranslation;

        #endregion

        #region API

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

        #endregion
    }
}
