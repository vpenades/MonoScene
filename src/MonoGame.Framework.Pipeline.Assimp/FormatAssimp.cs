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
            var factory = useBasicEffects ? (MeshFactory)new ClassicMeshFactory(graphics) : new PBRMeshFactory(graphics);

            return ConvertToXna(scene, factory);
        }

        public static ModelCollectionContent ConvertToXna(Assimp.Scene scene, MeshFactory meshFactory)
        {
            if (meshFactory == null) throw new ArgumentNullException();

            // create a mesh decoder for each mesh

            var meshDecoders = scene.Meshes.ToXna(scene.Materials);            

            // build the armatures and models

            var models = new List<ModelTemplate>();
            var armatures = new List<ArmatureTemplate>();

            var armatureFactory = new AssimpArmatureFactory(scene);
            var armature = armatureFactory.CreateArmature();
            armatures.Add(armature);

            var model = armatureFactory.CreateModel(scene, armature, meshDecoders);            

            models.Add(model);

            // convert mesh decoders to actual XNA mesh resources

            var meshCollection = meshFactory.CreateMeshCollection(meshDecoders);

            // coalesce all resources into a container class:

            return new ModelCollectionContent(meshCollection, armatures.ToArray(), models.ToArray(), 0);            
        }
    }
}
