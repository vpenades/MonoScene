using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

using MonoScene.Graphics.Content;

using XNAMAT = Microsoft.Xna.Framework.Matrix;

namespace MonoScene.Graphics
{
    /// <summary>
    /// Defines a drawable object within a model.
    /// </summary>
    /// <remarks>
    /// A <see cref="ModelTemplate"/> has a collection of <see cref="IDrawableTemplate"/> that can be
    /// seen as a sequence of drawing commands. So to some degree, it binds a visual resource (a mesh)
    /// with a scene location (the node transform).
    /// </remarks>
    public interface IDrawableTemplate
    {
        /// <summary>
        /// Typically this is the name of the content node that contained the mesh.
        /// </summary>
        string Name { get; }

        Object Tag { get; }

        /// <summary>
        /// An index into <see cref="ModelTemplate.Meshes"/>
        /// </summary>
        int MeshIndex { get; }

        IMeshTransform CreateGeometryTransform();

        void UpdateGeometryTransform(IMeshTransform rigidTransform, ArmatureInstance armature);
    }
       

    /// <summary>
    /// Defines a reference to a drawable mesh
    /// </summary>
    /// <remarks>
    /// This class is the 'glue' that binds a mesh with a <see cref="NodeTemplate"/> so we
    /// can calculate the local transform matrix of the mesh we want to render.
    /// </remarks>    
    abstract class DrawableTemplate : BaseContent, IDrawableTemplate
    {
        #region lifecycle

        protected DrawableTemplate(string name, int logicalMeshIndex)
            : base(name)
        {            
            _LogicalMeshIndex = logicalMeshIndex;            
        }

        #endregion

        #region data

        private readonly int _LogicalMeshIndex;

        #endregion

        #region properties
        
        /// <summary>
        /// An index into a <see cref="MeshCollection"/>
        /// </summary>
        public int MeshIndex => _LogicalMeshIndex;

        #endregion

        #region API

        public abstract IMeshTransform CreateGeometryTransform();

        public abstract void UpdateGeometryTransform(IMeshTransform geoxform, ArmatureInstance armature);

        #endregion
    }

    /// <summary>
    /// Defines a reference to a drawable rigid mesh
    /// </summary>
    sealed class RigidDrawableTemplate : DrawableTemplate
    {
        #region lifecycle

        public RigidDrawableTemplate(int meshIndex, NodeContent node)
            : base(node.Name, meshIndex)
        {
            _NodeIndex = node.ThisIndex;
        }

        #endregion

        #region data

        private readonly int _NodeIndex;

        #endregion

        #region API

        public override IMeshTransform CreateGeometryTransform() { return new MeshRigidTransform(); }

        public override void UpdateGeometryTransform(IMeshTransform rigidTransform, ArmatureInstance armature)
        {
            var node = armature.LogicalNodes[_NodeIndex];

            var statxform = (MeshRigidTransform)rigidTransform;
            statxform.Update(node.ModelMatrix);
            // statxform.Update(node.MorphWeights, false);
        }

        #endregion
    }

    /// <summary>
    /// Defines a reference to a drawable skinned mesh
    /// </summary>
    sealed class SkinnedDrawableTemplate : DrawableTemplate
    {
        #region lifecycle

        public SkinnedDrawableTemplate(int meshIndex, NodeContent morphNode, string ownerNname, (NodeContent, XNAMAT)[] skinNodes)
            : base(ownerNname, meshIndex)
        {
            // _MorphNodeIndex = indexFunc(morphNode);

            _JointsNodeIndices = new int[skinNodes.Length];
            _BindMatrices = new XNAMAT[skinNodes.Length];

            for (int i = 0; i < _JointsNodeIndices.Length; ++i)
            {
                var (j, ibm) = skinNodes[i];

                _JointsNodeIndices[i] = j.ThisIndex;
                _BindMatrices[i] = ibm;
            }
        }

        #endregion

        #region data

        private readonly int _MorphNodeIndex;
        private readonly int[] _JointsNodeIndices;
        private readonly XNAMAT[] _BindMatrices;

        #endregion

        #region API

        public override IMeshTransform CreateGeometryTransform() { return new MeshSkinTransform(); }

        public override void UpdateGeometryTransform(IMeshTransform skinnedTransform, ArmatureInstance armature)
        {
            var skinxform = (MeshSkinTransform)skinnedTransform;

            skinxform.Update(_JointsNodeIndices.Length, idx => _BindMatrices[idx], idx => armature.LogicalNodes[_JointsNodeIndices[idx]].ModelMatrix);

            // skinxform.Update(instances[_MorphNodeIndex].MorphWeights, false);
        }

        #endregion
    }
}
