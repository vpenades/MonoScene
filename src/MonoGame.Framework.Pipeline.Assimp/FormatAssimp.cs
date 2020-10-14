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
            AssimpMeshFactory meshFactory = useBasicEffects ? (AssimpMeshFactory)new BasicMeshFactory(graphics) : new PBRMeshFactory(graphics);

            return ConvertToXna(scene, meshFactory);
        }

        public static ModelCollectionContent ConvertToXna(Assimp.Scene scene, AssimpMeshFactory meshFactory)
        {
            if (meshFactory == null) throw new ArgumentNullException();

            var meshCollection = meshFactory.CreateMeshCollection(scene.Meshes, idx => scene.Materials[idx]);
            
            var models = new List<ModelTemplate>();
            var armatures = new List<ArmatureTemplate>();

            var armatureFactory = new AssimpArmatureFactory(scene);
            var armature = armatureFactory.CreateArmature();
            armatures.Add(armature);

            var model = armatureFactory.CreateModel(scene, armature);
            // model.ModelBounds = scene.EvaluateBoundingSphere().ToXna();
            model.ModelBounds = new BoundingSphere(Vector3.Zero, 1000); // temporary hack
            models.Add(model);            

            return new ModelCollectionContent(meshCollection, armatures.ToArray(), models.ToArray(), 0);            
        }
    }
}
