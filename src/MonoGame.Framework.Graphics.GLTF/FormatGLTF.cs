using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

using SharpGLTF.Runtime;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public static class FormatGLTF
    {
        public static ModelCollectionContent LoadModel(string filePath, GraphicsDevice graphics, bool useBasicEffects = false)
        {
            var model = SharpGLTF.Schema2.ModelRoot.Load(filePath, SharpGLTF.Validation.ValidationMode.TryFix);

            return ReadModel(model, graphics, useBasicEffects);
        }

        public static ModelCollectionContent LoadModel(System.IO.FileInfo finfo, GraphicsDevice graphics, bool useBasicEffects = false)
        {
            var model = SharpGLTF.Schema2.ModelRoot.Load(finfo.FullName, SharpGLTF.Validation.ValidationMode.TryFix);

            return ReadModel(model, graphics, useBasicEffects);
        }

        public static ModelCollectionContent ReadModel(SharpGLTF.Schema2.ModelRoot model, GraphicsDevice graphics, bool useBasicEffects = false)
        {
            var factory = useBasicEffects ? (GLTFMeshFactory)new BasicMeshFactory(graphics) : new PBRMeshFactory(graphics);

            return ConvertToXna(model, factory);
        }

        public static ModelCollectionContent ConvertToXna(SharpGLTF.Schema2.ModelRoot srcModel, GLTFMeshFactory meshFactory)
        {
            if (meshFactory == null) throw new ArgumentNullException();

            var meshCollection = meshFactory.CreateMeshCollection(srcModel.LogicalMeshes);

            var models = new List<ModelTemplate>();
            var armatures = new List<ArmatureTemplate>();

            

            foreach (var scene in srcModel.LogicalScenes)
            {
                var armatureFactory = new GLTFArmatureFactory(scene);

                for (int i = 0; i < srcModel.LogicalAnimations.Count; ++i)
                {
                    var track = srcModel.LogicalAnimations[i];
                    armatureFactory.SetAnimationTrack(i, track.Name, track.Duration);
                }

                var armature = armatureFactory.CreateArmature();
                armatures.Add(armature);

                var model = armatureFactory.CreateModel(scene, armature);
                model.ModelBounds = scene.EvaluateBoundingSphere().ToXna();
                models.Add(model);
            }

            return new ModelCollectionContent(meshCollection, armatures.ToArray(), models.ToArray(), srcModel.DefaultScene.LogicalIndex);
        }       
    }
}
