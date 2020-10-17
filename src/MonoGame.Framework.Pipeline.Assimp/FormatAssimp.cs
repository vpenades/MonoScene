using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public static class FormatAssimp
    {
        public static ModelCollectionContent LoadModel(string filePath, GraphicsDevice graphics, bool useBasicEffects = false)
        {
            var context = new Assimp.AssimpContext();

            var pp = Assimp.PostProcessSteps.FindInvalidData | Assimp.PostProcessSteps.OptimizeGraph;
            var scene = context.ImportFile(filePath, pp);

            return ReadModel(scene, graphics, useBasicEffects);
        }

        public static ModelCollectionContent ReadModel(Assimp.Scene scene, GraphicsDevice graphics, bool useBasicEffects = false)
        {
            MeshFactory meshFactory = new MeshFactory(graphics);

            return ConvertToXna(scene, meshFactory);
        }

        public static ModelCollectionContent ConvertToXna(Assimp.Scene scene, MeshFactory meshFactory)
        {
            if (meshFactory == null) throw new ArgumentNullException();

            var dstMeshes = scene.Meshes.ToXna(scene.Materials);

            var meshCollection = meshFactory.CreateMeshCollection(dstMeshes);
            
            var models = new List<ModelTemplate>();
            var armatures = new List<ArmatureTemplate>();

            var armatureFactory = new AssimpArmatureFactory(scene);
            var armature = armatureFactory.CreateArmature();
            armatures.Add(armature);

            var model = armatureFactory.CreateModel(scene, armature);
            model.ModelBounds = MeshFactory.EvaluateBoundingSphere(model.CreateInstance(), dstMeshes);
            models.Add(model);            

            return new ModelCollectionContent(meshCollection, armatures.ToArray(), models.ToArray(), 0);            
        }
    }
}
