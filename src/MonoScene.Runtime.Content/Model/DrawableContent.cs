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
    }

    public class RigidDrawableContent : DrawableContent
    {
        #region lifecycle

        protected RigidDrawableContent(RigidDrawableContent other)
            : base(other)
        {
            this.NodeIndex = other.NodeIndex;
        }

        internal RigidDrawableContent(int meshIndex, NodeContent node)
            : base(meshIndex)
        {
            NodeIndex = node.ThisIndex;
        }

        #endregion

        #region data

        /// <summary>
        /// An index into a <see cref="MeshCollectionContent.Meshes"/>
        /// </summary>
        protected readonly int NodeIndex;

        #endregion
    }

    public class SkinnedDrawableContent : DrawableContent
    {
        #region lifecycle

        protected SkinnedDrawableContent(SkinnedDrawableContent other)
            : base(other)
        {
            this.MorphNodeIndex = other.MorphNodeIndex;
            this.JointsNodeIndices = other.JointsNodeIndices;
            this.JointsBindMatrices = other.JointsBindMatrices;
        }

        internal SkinnedDrawableContent(int meshIndex, NodeContent morphNode, (NodeContent, XNAMAT)[] skinNodes)
            : base(meshIndex)
        {
            // _MorphNodeIndex = indexFunc(morphNode);

            JointsNodeIndices = new int[skinNodes.Length];
            JointsBindMatrices = new XNAMAT[skinNodes.Length];

            for (int i = 0; i < JointsNodeIndices.Length; ++i)
            {
                var (j, ibm) = skinNodes[i];

                JointsNodeIndices[i] = j.ThisIndex;
                JointsBindMatrices[i] = ibm;
            }
        }

        #endregion

        #region data

        /// <summary>
        /// An index into a <see cref="ArmatureContent"/> which holds the morph state.
        /// </summary>
        protected readonly int MorphNodeIndex;

        /// <summary>
        /// Bone indices into <see cref="ArmatureContent"/>.
        /// </summary>
        protected readonly int[] JointsNodeIndices;

        /// <summary>
        /// Inverse Bind Matrices associated to <see cref="JointsNodeIndices"/>.
        /// </summary>
        protected readonly XNAMAT[] JointsBindMatrices;

        #endregion                
    }
}
