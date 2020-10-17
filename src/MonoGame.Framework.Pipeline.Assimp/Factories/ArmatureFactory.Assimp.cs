using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Helper class used to create an <see cref="ArmatureTemplate"/> and help in creating the <see cref="IDrawableTemplate"/> objects.
    /// </summary>
    class AssimpArmatureFactory : ArmatureFactory<Assimp.Node>
    {
        #region constructor

        public AssimpArmatureFactory(Assimp.Scene scene)
        {
            AddRoot(scene.RootNode);
        }

        #endregion

        #region overrides

        protected override string GetName(Assimp.Node node) { return node.Name; }

        protected override IEnumerable<Assimp.Node> GetChildren(Assimp.Node node) { return node.Children; }

        protected override Matrix GetLocalMatrix(Assimp.Node node) { return node.Transform.ToXna(); }

        protected override AnimatableProperty<Vector3> GetScale(Assimp.Node node) { return null; }

        protected override AnimatableProperty<Quaternion> GetRotation(Assimp.Node node) { return null; }

        protected override AnimatableProperty<Vector3> GetTranslation(Assimp.Node node) { return null; }

        #endregion

        #region API

        public ModelTemplate CreateModel(Assimp.Scene scene, ArmatureTemplate armature, IReadOnlyList<IMeshDecoder<MaterialContent>> meshDecoders)
        {
            var model = CreateModel(scene, armature);
            model.ModelBounds = MeshFactory.EvaluateBoundingSphere(model.CreateInstance(), meshDecoders);
            return model;
        }

        public ModelTemplate CreateModel(Assimp.Scene scene, ArmatureTemplate armature)
        {
            var drawables = Flatten(scene.RootNode)
                .Where(item => item.HasMeshes)
                .SelectMany(item => CreateDrawables(item, scene.Meshes))
                .ToArray();

            return new ModelTemplate(null, armature, drawables);
        }

        private static IEnumerable<Assimp.Node> Flatten(Assimp.Node node)
        {
            var flattenedChildren = node.Children.SelectMany(c => Flatten(c));
            return new Assimp.Node[] { node }.Concat(flattenedChildren);
        }

        public IEnumerable<IDrawableTemplate> CreateDrawables(Assimp.Node node, IReadOnlyList<Assimp.Mesh> meshes)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (!node.HasMeshes) throw new ArgumentNullException(nameof(node.HasMeshes));

            // find root node for node search
            var rootNode = node;
            while (rootNode.Parent != null) rootNode = rootNode.Parent;

            // gather meshes
            var meshParts = node.MeshIndices
                .Select(idx => (idx, meshes[idx]))
                .ToArray();

            // gather mesh indices for meshes WITHOUT bones
            var rigidIndices = meshParts
                .Where(item => !item.Item2.HasBones)
                .Select(item => item.idx)
                .ToArray();

            // gather mesh indices for meshes WITH bones
            var skinnedIndices = meshParts
                .Where(item => item.Item2.HasBones)
                .Select(item => item.idx)
                .ToArray();

            // process rigid meshes
            foreach (var meshIdx in rigidIndices)
            {
                yield return CreateRigidDrawable(meshIdx, node);
            }

            // process skinned meshes
            foreach (var meshIdx in skinnedIndices)
            {
                var skin = meshes[meshIdx].Bones
                    .Select(item => (rootNode.FindNode(item.Name), item.OffsetMatrix.ToXna()))
                    .ToArray();                    

                yield return CreateSkinnedDrawable(meshIdx, node, skin);
            }
        }

        #endregion
    }
}
