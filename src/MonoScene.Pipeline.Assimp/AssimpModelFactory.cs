using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Graphics;

using MonoScene.Graphics.Content;

namespace MonoScene.Graphics.Pipeline
{
    public class AssimpModelFactory
    {
        #region lifecycle
        public AssimpModelFactory(GraphicsDevice device)
        {
            _Device = device;
        }

        #endregion

        #region data

        private readonly GraphicsDevice _Device;

        #endregion

        #region properties

        public bool UseBasicEffects { get; set; }

        #endregion

        public DeviceModelCollection LoadModel(string filePath)
        {
            var context = new Assimp.AssimpContext();

            var pp = Assimp.PostProcessSteps.FindInvalidData | Assimp.PostProcessSteps.OptimizeGraph;
            var scene = context.ImportFile(filePath, pp);

            return ReadModel(scene);
        }

        public DeviceModelCollection ReadModel(Assimp.Scene scene)
        {
            var factory = UseBasicEffects ? (DeviceMeshFactory)new ClassicMeshFactory(_Device) : new PBRMeshFactory(_Device);

            return ConvertToDevice(scene, factory);
        }

        public static DeviceModelCollection ConvertToDevice(Assimp.Scene srcScene, DeviceMeshFactory meshFactory)
        {
            if (meshFactory == null) throw new ArgumentNullException();

            var content = ConvertToContent(srcScene);

            return DeviceModelCollection.CreateFrom(content, meshFactory.CreateMeshCollection);
        }

        public static ModelCollectionContent ConvertToContent(Assimp.Scene scene)
        {
            // create a mesh decoder for each mesh

            var meshDecoders = scene.Meshes.ToXna(scene.Materials);
            var meshContent = MeshCollectionBuilder.CreateContent(meshDecoders);

            // build the armatures and models

            var models = new List<ModelContent>();
            var armatures = new List<ArmatureContent>();

            var armatureFactory = new AssimpArmatureFactory(scene);
            var armature = armatureFactory.CreateArmature();
            armatures.Add(armature);

            var model = armatureFactory.CreateModelContent(scene, armatures.Count-1);

            model.Name = "AssimpScene";
            model.Tag = scene.Metadata;

            // model.ModelBounds = 

            models.Add(model);            

            // coalesce all resources into a container class:

            var content = new ModelCollectionContent(meshContent, armatures.ToArray(), models.ToArray(), 0);

            content = PostProcessor.Postprocess(content);

            return content;
        }
    }
}
