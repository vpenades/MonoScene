using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Runtime.Graphics
{
    public static class FormatAssimp
    {
        public static DeviceModelCollection LoadModel(string filePath, GraphicsDevice graphics, bool useBasicEffects = false)
        {
            var context = new Assimp.AssimpContext();

            var pp = Assimp.PostProcessSteps.FindInvalidData | Assimp.PostProcessSteps.OptimizeGraph;
            var scene = context.ImportFile(filePath, pp);

            return ReadModel(scene, graphics, useBasicEffects);
        }

        public static DeviceModelCollection ReadModel(Assimp.Scene scene, GraphicsDevice graphics, bool useBasicEffects = false)
        {
            var factory = useBasicEffects ? (MeshFactory)new ClassicMeshFactory(graphics) : new PBRMeshFactory(graphics);

            return ConvertToXna(scene, factory);
        }

        public static DeviceModelCollection ConvertToXna(Assimp.Scene scene, MeshFactory meshFactory)
        {
            if (meshFactory == null) throw new ArgumentNullException();

            return ConvertToContent(scene).ToDeviceModelCollection(meshFactory);
        }

        public static ModelCollectionContent ConvertToContent(Assimp.Scene scene)
        {
            // create a mesh decoder for each mesh

            var meshDecoders = scene.Meshes.ToXna(scene.Materials);
            var meshContent = MeshCollectionContent.CreateFromMeshes(meshDecoders);

            // build the armatures and models

            var models = new List<ModelTemplate>();
            var armatures = new List<ArmatureTemplate>();

            var armatureFactory = new AssimpArmatureFactory(scene);
            var armature = armatureFactory.CreateArmature();
            armatures.Add(armature);

            var model = armatureFactory.CreateModel(scene, armature, meshDecoders);

            models.Add(model);            

            // coalesce all resources into a container class:

            return new ModelCollectionContent(meshContent, armatures.ToArray(), models.ToArray(), 0);
        }
    }
}
