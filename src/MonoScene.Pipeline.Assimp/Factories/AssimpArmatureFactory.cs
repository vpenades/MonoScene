using System;
using System.Collections.Generic;
using System.Linq;

using MonoScene.Graphics.Content;

using XNAV3 = Microsoft.Xna.Framework.Vector3;
using XNAMAT = Microsoft.Xna.Framework.Matrix;
using XNAQUAT = Microsoft.Xna.Framework.Quaternion;


namespace MonoScene.Graphics.Pipeline
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
        protected override object GetTag(Assimp.Node node) { return node.Metadata; }

        protected override IEnumerable<Assimp.Node> GetChildren(Assimp.Node node) { return node.Children; }

        protected override XNAMAT GetLocalMatrix(Assimp.Node node) { return node.Transform.ToXna(); }

        protected override AnimatableProperty<XNAV3> GetScale(Assimp.Node node) { return null; }

        protected override AnimatableProperty<XNAQUAT> GetRotation(Assimp.Node node) { return null; }

        protected override AnimatableProperty<XNAV3> GetTranslation(Assimp.Node node) { return null; }

        #endregion

        #region API

        private static IEnumerable<Assimp.Node> Flatten(Assimp.Node node)
        {
            var flattenedChildren = node.Children.SelectMany(c => Flatten(c));
            return new Assimp.Node[] { node }.Concat(flattenedChildren);
        }

        public ModelContent CreateModelContent(Assimp.Scene scene, int armatureIndex)
        {
            var drawables = Flatten(scene.RootNode)
                .Where(item => item.HasMeshes)
                .SelectMany(item => CreateDrawableContent(item, scene.Meshes))
                .ToArray();

            return new ModelContent(armatureIndex, drawables);
        }

        public IEnumerable<DrawableContent> CreateDrawableContent(Assimp.Node node, IReadOnlyList<Assimp.Mesh> meshes)
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
                yield return CreateRigidDrawableContent(meshIdx, node);
            }

            // process skinned meshes
            foreach (var meshIdx in skinnedIndices)
            {
                var skin = meshes[meshIdx].Bones
                    .Select(item => (rootNode.FindNode(item.Name), item.OffsetMatrix.ToXna()))
                    .ToArray();

                yield return CreateSkinnedDrawableContent(meshIdx, node, skin);
            }
        }

        #endregion
    }
}
