using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

using SharpGLTF.Runtime;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public static class FormatGLTF
    {
        public static ModelTemplateContent LoadModel(string filePath, GraphicsDevice graphics, bool useBasicEffects = false)
        {
            var model = SharpGLTF.Schema2.ModelRoot.Load(filePath, SharpGLTF.Validation.ValidationMode.TryFix);

            return ReadModel(model, graphics, useBasicEffects);
        }

        public static ModelTemplateContent LoadModel(System.IO.FileInfo finfo, GraphicsDevice graphics, bool useBasicEffects = false)
        {
            var model = SharpGLTF.Schema2.ModelRoot.Load(finfo.FullName, SharpGLTF.Validation.ValidationMode.TryFix);

            return ReadModel(model, graphics, useBasicEffects);
        }

        public static ModelTemplateContent ReadModel(SharpGLTF.Schema2.ModelRoot model, GraphicsDevice graphics, bool useBasicEffects = false)
        {
            var factory = useBasicEffects ? (GLTFMeshFactory)new BasicMeshFactory(graphics) : new PBRMeshFactory(graphics);

            return ConvertToXna(model, factory);
        }

        public static ModelTemplateContent ConvertToXna(SharpGLTF.Schema2.ModelRoot srcModel, GLTFMeshFactory meshFactory)
        {
            if (meshFactory == null) throw new ArgumentNullException();

            var meshCollection = meshFactory.CreateMeshCollection(srcModel.LogicalMeshes);

            var layers = new List<ModelLayerTemplate>();

            foreach (var scene in srcModel.LogicalScenes)
            {
                var armatureFactory = new GLTFArmatureFactory();

                for (int i = 0; i < srcModel.LogicalAnimations.Count; ++i)
                {
                    var track = srcModel.LogicalAnimations[i];
                    armatureFactory.SetAnimationTrack(i, track.Name, track.Duration);
                }

                var layer = armatureFactory.CreateModelLayer(scene);

                layer.ModelBounds = scene.EvaluateBoundingSphere().ToXna();

                layers.Add(layer);
            }

            return new ModelTemplateContent(meshCollection, layers.ToArray(), srcModel.DefaultScene.LogicalIndex);
        }       
    }
}
