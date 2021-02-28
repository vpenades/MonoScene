using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MonoScene.Graphics.Content;

using GLTFNODE = SharpGLTF.Schema2.Node;

using XNAV3 = Microsoft.Xna.Framework.Vector3;
using XNAMAT = Microsoft.Xna.Framework.Matrix;
using XNAQUAT = Microsoft.Xna.Framework.Quaternion;


namespace MonoScene.Graphics.Pipeline
{
    /// <summary>
    /// Helper class used to create an <see cref="ArmatureTemplate"/> and help in creating the <see cref="IDrawableTemplate"/> objects.
    /// </summary>
    class GLTFArmatureFactory : ArmatureFactory<GLTFNODE>
    {
        #region constructor

        public GLTFArmatureFactory(SharpGLTF.Schema2.Scene scene, Converter<SharpGLTF.Schema2.ExtraProperties, Object> tagConverter)
        {
            AddSceneRoot(scene);
            _TagConverter = tagConverter;
        }

        #endregion

        #region data

        private readonly Converter<SharpGLTF.Schema2.ExtraProperties, Object> _TagConverter;

        #endregion

        #region overrides

        protected override string GetName(GLTFNODE node) { return node.Name; }
        protected override object GetTag(GLTFNODE node) { return _TagConverter?.Invoke(node); }

        protected override IEnumerable<GLTFNODE> GetChildren(GLTFNODE node) { return node.VisualChildren; }

        protected override XNAMAT GetLocalMatrix(GLTFNODE node) { return node.LocalMatrix; }        

        protected override AnimatableProperty<XNAV3> GetScale(GLTFNODE node)
        {
            var lxform = node.LocalTransform.ToXna();

            var s = new AnimatableProperty<XNAV3>(lxform.Scale);

            foreach (var anim in node.LogicalParent.LogicalAnimations)
            {
                var sAnim = anim
                    .FindScaleChannel(node)
                    ?.GetScaleSampler()
                    ?.CreateCurveSampler(true)
                    .ToXna();

                if (sAnim != null) s.SetCurve(anim.LogicalIndex, sAnim);
            }

            return s;
        }

        protected override AnimatableProperty<XNAQUAT> GetRotation(GLTFNODE node)
        {
            var lxform = node.LocalTransform.ToXna();

            var r = new AnimatableProperty<XNAQUAT>(lxform.Rotation);

            foreach (var anim in node.LogicalParent.LogicalAnimations)
            {
                var rAnim = anim
                    .FindRotationChannel(node)
                    ?.GetRotationSampler()
                    ?.CreateCurveSampler(true)
                    .ToXna();

                if (rAnim != null) r.SetCurve(anim.LogicalIndex, rAnim);
            }

            return r;
        }

        protected override AnimatableProperty<XNAV3> GetTranslation(GLTFNODE node)
        {
            var lxform = node.LocalTransform.ToXna();

            var t = new AnimatableProperty<XNAV3>(lxform.Translation);

            foreach (var anim in node.LogicalParent.LogicalAnimations)
            {
                var tAnim = anim
                    .FindTranslationChannel(node)
                    ?.GetTranslationSampler()
                    ?.CreateCurveSampler(true)
                    .ToXna();

                if (tAnim != null) t.SetCurve(anim.LogicalIndex, tAnim);
            }

            return t;
        }

        #endregion

        #region API

        public void AddSceneRoot(SharpGLTF.Schema2.Scene scene)
        {
            foreach (var root in scene.VisualChildren)
            {
                AddRoot(root);
            }
        }

        public ModelContent CreateModelContent(SharpGLTF.Schema2.Scene scene, int armatureIndex)
        {
            var drawables = GLTFNODE.Flatten(scene)
                .Where(item => item.Mesh != null)
                .Select(item => CreateDrawableContent(item))
                .ToArray();

            var model = new ModelContent(armatureIndex, drawables);

            return model;
        }

        public DrawableContent CreateDrawableContent(GLTFNODE node)
        {
            if (node == null) throw new ArgumentNullException(nameof(GLTFNODE));
            if (node.Mesh == null) throw new ArgumentNullException(nameof(GLTFNODE.Mesh));

            if (node.Skin == null)
            {
                return CreateRigidDrawableContent(node.Mesh.LogicalIndex, node);
            }
            else
            {
                var bones = new (GLTFNODE, XNAMAT)[node.Skin.JointsCount];
                for (int i = 0; i < bones.Length; ++i)
                {
                    var (joint, inverseBindMatrix) = node.Skin.GetJoint(i);

                    bones[i] = (joint, inverseBindMatrix);
                }

                return CreateSkinnedDrawableContent(node.Mesh.LogicalIndex, node, bones);
            }
        }

        #endregion
    }
}
