﻿using System;
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
            var factory = UseBasicEffects ? (MeshFactory)new ClassicMeshFactory(_Device) : new PBRMeshFactory(_Device);

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
            var armatures = new List<ArmatureContent>();

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
