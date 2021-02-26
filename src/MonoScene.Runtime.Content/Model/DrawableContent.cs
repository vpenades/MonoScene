using System;
using System.Collections.Generic;
using System.Text;

using XNAMAT = Microsoft.Xna.Framework.Matrix;

namespace MonoScene.Graphics.Content
{
    public abstract class DrawableContent : BaseContent
    {
        #region lifecycle

        public static DrawableContent CreateRigid(int meshIndex, NodeContent node)
        {
            return new RigidDrawableContent(meshIndex, node);
        }

        public static DrawableContent CreateSkinned(int meshIndex, NodeContent morphNode, (NodeContent, XNAMAT)[] skinNodes)
        {
            return new SkinnedDrawableContent(meshIndex, morphNode, skinNodes);
        }

        protected DrawableContent(DrawableContent other)
            : base(other)
        {
            this._MeshIndex = other._MeshIndex;
        }

        protected DrawableContent(int logicalMeshIndex)            
        {
            _MeshIndex = logicalMeshIndex;
        }

        #endregion

        #region data
        
        private readonly int _MeshIndex;

        #endregion

        #region properties

        /// <summary>
        /// An index into a <see cref="MeshCollectionContent.Meshes"/>
        /// </summary>
        public int MeshIndex => _MeshIndex;

        #endregion

        #region API

        /// <summary>
        /// Creates new <see cref="IMeshTransform"/> instance.
        /// </summary>
        /// <returns></returns>
        public abstract IMeshTransform CreateTransformInstance();

        #endregion
    }

    sealed class RigidDrawableContent : DrawableContent
    {
        #region lifecycle

        protected RigidDrawableContent(RigidDrawableContent other)
            : base(other)
        {
            this._NodeIndex = other._NodeIndex;
        }

        internal RigidDrawableContent(int meshIndex, NodeContent node)
            : base(meshIndex)
        {
            _NodeIndex = node.ThisIndex;
        }

        #endregion

        #region data

        /// <summary>
        /// An index into a <see cref="IArmatureTransform.GetModelMatrix(int)"/>
        /// </summary>
        internal readonly int _NodeIndex;

        #endregion

        #region API

        public override IMeshTransform CreateTransformInstance()
        {
            return new _MeshRigidTransform(this);
        }

        #endregion
    }

    sealed class SkinnedDrawableContent : DrawableContent
    {
        #region lifecycle

        protected SkinnedDrawableContent(SkinnedDrawableContent other)
            : base(other)
        {
            this._MorphNodeIndex = other._MorphNodeIndex;
            this._JointsNodeIndices = other._JointsNodeIndices;
            this._JointsBindMatrices = other._JointsBindMatrices;
        }

        internal SkinnedDrawableContent(int meshIndex, NodeContent morphNode, (NodeContent, XNAMAT)[] skinNodes)
            : base(meshIndex)
        {
            // _MorphNodeIndex = indexFunc(morphNode);

            _JointsNodeIndices = new int[skinNodes.Length];
            _JointsBindMatrices = new XNAMAT[skinNodes.Length];

            for (int i = 0; i < _JointsNodeIndices.Length; ++i)
            {
                var (j, ibm) = skinNodes[i];

                _JointsNodeIndices[i] = j.ThisIndex;
                _JointsBindMatrices[i] = ibm;
            }
        }

        #endregion

        #region data

        /// <summary>
        /// An index into a <see cref="IArmatureTransform.GetMorphState(int)"/>.
        /// </summary>
        internal readonly int _MorphNodeIndex;

        /// <summary>
        /// Bone indices into <see cref="IArmatureTransform.GetModelMatrix(int)"/>.
        /// </summary>
        internal readonly int[] _JointsNodeIndices;

        /// <summary>
        /// Inverse Bind Matrices associated to <see cref="_JointsNodeIndices"/>.
        /// </summary>
        internal readonly XNAMAT[] _JointsBindMatrices;

        #endregion                

        #region API

        public override IMeshTransform CreateTransformInstance()
        {
            return new _MeshSkinTransform(this);
        }

        #endregion
    }
}
