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
       

    

    static class DrawableTemplateFactory
    {
        public static IDrawableTemplate UpcastToTemplate(this DrawableContent content)
        {
            if (content is RigidDrawableContent rigid) return new RigidDrawableTemplate(rigid);
            if (content is SkinnedDrawableContent skinned) return new SkinnedDrawableTemplate(skinned);
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Defines a reference to a drawable rigid mesh
    /// </summary>
    sealed class RigidDrawableTemplate : RigidDrawableContent , IDrawableTemplate
    {
        #region lifecycle

        public RigidDrawableTemplate(RigidDrawableContent content)
            : base(content) { }

        #endregion        

        #region API

        public IMeshTransform CreateGeometryTransform() { return new MeshRigidTransform(); }

        public void UpdateGeometryTransform(IMeshTransform rigidTransform, ArmatureInstance armature)
        {
            var node = armature.LogicalNodes[NodeIndex];

            var statxform = (MeshRigidTransform)rigidTransform;
            statxform.Update(node.ModelMatrix);
            // statxform.Update(node.MorphWeights, false);
        }

        #endregion
    }

    /// <summary>
    /// Defines a reference to a drawable skinned mesh
    /// </summary>
    sealed class SkinnedDrawableTemplate : SkinnedDrawableContent, IDrawableTemplate
    {
        #region lifecycle

        public SkinnedDrawableTemplate(SkinnedDrawableContent content)
            : base(content) { }

        #endregion

        #region API

        public IMeshTransform CreateGeometryTransform() { return new MeshSkinTransform(); }

        public void UpdateGeometryTransform(IMeshTransform skinnedTransform, ArmatureInstance armature)
        {
            var skinxform = (MeshSkinTransform)skinnedTransform;

            skinxform.Update(this.JointsNodeIndices.Length
                , idx => this.JointsBindMatrices[idx]
                , idx => armature.LogicalNodes[this.JointsNodeIndices[idx]].ModelMatrix);

            // skinxform.Update(instances[_MorphNodeIndex].MorphWeights, false);
        }

        #endregion
    }
}
