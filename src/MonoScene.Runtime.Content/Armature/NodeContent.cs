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
    public partial class NodeContent : BaseContent
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
    }
}
