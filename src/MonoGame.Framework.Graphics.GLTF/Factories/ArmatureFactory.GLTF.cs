using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

using GLTFNODE = SharpGLTF.Schema2.Node;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Helper class used to create an <see cref="ArmatureTemplate"/> and help in creating the <see cref="IDrawableTemplate"/> objects.
    /// </summary>
    class GLTFArmatureFactory : ArmatureFactory<GLTFNODE>
    {
        #region constructor

        public GLTFArmatureFactory(SharpGLTF.Schema2.Scene scene)
        {
            AddSceneRoot(scene);
        }

        #endregion

        #region overrides

        protected override string GetName(GLTFNODE node) { return node.Name; }

        protected override IEnumerable<GLTFNODE> GetChildren(GLTFNODE node) { return node.VisualChildren; }

        protected override Matrix GetLocalMatrix(GLTFNODE node) { return node.LocalMatrix.ToXna(); }        

        protected override AnimatableProperty<Vector3> GetScale(GLTFNODE node)
        {
            var lxform = node.LocalTransform.ToXna();

            var s = new AnimatableProperty<Vector3>(lxform.Scale);

            foreach (var anim in node.LogicalParent.LogicalAnimations)
            {
                var sAnim = anim.FindScaleSampler(node)?.CreateCurveSampler(true).ToXna();
                if (sAnim != null) s.SetCurve(anim.LogicalIndex, sAnim);
            }

            return s;
        }

        protected override AnimatableProperty<Quaternion> GetRotation(GLTFNODE node)
        {
            var lxform = node.LocalTransform.ToXna();

            var r = new AnimatableProperty<Quaternion>(lxform.Rotation);

            foreach (var anim in node.LogicalParent.LogicalAnimations)
            {
                var rAnim = anim.FindRotationSampler(node)?.CreateCurveSampler(true).ToXna();
                if (rAnim != null) r.SetCurve(anim.LogicalIndex, rAnim);
            }

            return r;
        }

        protected override AnimatableProperty<Vector3> GetTranslation(GLTFNODE node)
        {
            var lxform = node.LocalTransform.ToXna();

            var t = new AnimatableProperty<Vector3>(lxform.Translation);

            foreach (var anim in node.LogicalParent.LogicalAnimations)
            {
                var tAnim = anim.FindTranslationSampler(node)?.CreateCurveSampler(true).ToXna();
                if (tAnim != null) t.SetCurve(anim.LogicalIndex, tAnim);
            }

            return t;
        }

        #endregion

        #region API

        public ModelTemplate CreateModel(SharpGLTF.Schema2.Scene scene, ArmatureTemplate armature, IReadOnlyList<IMeshDecoder<MaterialContent>> meshDecoders)
        {
            var model = CreateModel(scene, armature);
            model.ModelBounds = MeshFactory.EvaluateBoundingSphere(model.CreateInstance(), meshDecoders);
            return model;
        }

        public ModelTemplate CreateModel(SharpGLTF.Schema2.Scene scene, ArmatureTemplate armature)
        {
            var drawables = GLTFNODE.Flatten(scene)
                .Where(item => item.Mesh != null)
                .Select(item => CreateDrawable(item))
                .ToArray();

            return new ModelTemplate(scene.Name, armature, drawables);
        }

        public void AddSceneRoot(SharpGLTF.Schema2.Scene scene)
        {
            foreach (var root in scene.VisualChildren)
            {
                AddRoot(root);
            }
        }

        public IDrawableTemplate CreateDrawable(GLTFNODE node)
        {
            if (node == null) throw new ArgumentNullException(nameof(GLTFNODE));
            if (node.Mesh == null) throw new ArgumentNullException(nameof(GLTFNODE.Mesh));

            if (node.Skin == null)
            {
                return CreateRigidDrawable(node.Mesh.LogicalIndex, node);
            }
            else
            {
                var bones = new (GLTFNODE, Matrix)[node.Skin.JointsCount];
                for (int i = 0; i < bones.Length; ++i)
                {
                    var (joint, inverseBindMatrix) = node.Skin.GetJoint(i);

                    bones[i] = (joint, inverseBindMatrix.ToXna());
                }

                return CreateSkinnedDrawable(node.Mesh.LogicalIndex, node, bones);
            }
        }        

        #endregion
    }
}
