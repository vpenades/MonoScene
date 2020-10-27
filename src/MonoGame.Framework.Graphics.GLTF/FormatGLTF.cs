using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

using SharpGLTF.Runtime;

namespace Microsoft.Xna.Framework.Content.Runtime.Graphics
{
    public static class FormatGLTF
    {
        public static DeviceModelCollection LoadModelFromEmbeddedResource(Assembly assembly, string resourcePath, GraphicsDevice graphics, bool useBasicEffects = false)
        {
            resourcePath = assembly
                .GetManifestResourceNames()
                .FirstOrDefault(item => item.EndsWith(resourcePath));

            using(var s = assembly.GetManifestResourceStream(resourcePath))
            {
                return ReadModel(s, graphics, useBasicEffects);
            }            
        }

        /// <summary>
        /// Loads a glTF or a glb from the file system.
        /// </summary>
        public static DeviceModelCollection LoadModel(string filePath, GraphicsDevice graphics, bool useBasicEffects = false)
        {
            var model = SharpGLTF.Schema2.ModelRoot.Load(filePath, SharpGLTF.Validation.ValidationMode.TryFix);

            return ReadModel(model, graphics, useBasicEffects);
        }

        /// <summary>
        /// Loads a glb from the file system (no, plain glTFs can't be loaded from stream)
        /// </summary>
        public static DeviceModelCollection ReadModel(System.IO.Stream glbModelStream, GraphicsDevice graphics, bool useBasicEffects = false)
        {
            var model = SharpGLTF.Schema2.ModelRoot.ReadGLB(glbModelStream, SharpGLTF.Validation.ValidationMode.TryFix);

            return ReadModel(model, graphics, useBasicEffects);
        }

        public static DeviceModelCollection LoadModel(System.IO.FileInfo finfo, GraphicsDevice graphics, bool useBasicEffects = false)
        {
            var model = SharpGLTF.Schema2.ModelRoot.Load(finfo.FullName, SharpGLTF.Validation.ValidationMode.TryFix);

            return ReadModel(model, graphics, useBasicEffects);
        }

        public static DeviceModelCollection ReadModel(SharpGLTF.Schema2.ModelRoot model, GraphicsDevice graphics, bool useBasicEffects = false)
        {
            var factory = useBasicEffects ? (MeshFactory)new ClassicMeshFactory(graphics) : new PBRMeshFactory(graphics);

            return ConvertToXna(model, factory);
        }

        public static DeviceModelCollection ConvertToXna(SharpGLTF.Schema2.ModelRoot srcModel, MeshFactory meshFactory)
        {
            if (meshFactory == null) throw new ArgumentNullException();

            return ConvertToContent(srcModel).ToDeviceModelCollection(meshFactory);
        }

        public static ModelCollectionContent ConvertToContent(SharpGLTF.Schema2.ModelRoot srcModel)
        {
            // create a mesh decoder for each mesh

            var meshDecoders = srcModel.LogicalMeshes.ToXnaDecoders();
            var meshContent = MeshCollectionContent.CreateFromMeshes(meshDecoders);

            // build the armatures and models

            var armatures = new List<ArmatureTemplate>();
            var models = new List<ModelTemplate>();

            foreach (var scene in srcModel.LogicalScenes)
            {
                var armatureFactory = new GLTFArmatureFactory(scene);

                for (int i = 0; i < srcModel.LogicalAnimations.Count; ++i)
                {
                    var track = srcModel.LogicalAnimations[i];
                    armatureFactory.SetAnimationTrack(i, track.Name, track.Duration);
                }

                // TODO: check if we can share armatures
                var armature = armatureFactory.CreateArmature();
                armatures.Add(armature);

                var model = armatureFactory.CreateModel(scene, armature, meshDecoders);
                models.Add(model);
            }
            
            // coalesce all resources into a container class:

            return new ModelCollectionContent(meshContent, armatures.ToArray(), models.ToArray(), srcModel.DefaultScene.LogicalIndex);
        }              

        public static MeshCollectionContent ReadMeshContent(IEnumerable<SharpGLTF.Schema2.Mesh> meshes)
        {
            var meshDecoders = meshes.ToXnaDecoders();

            return MeshCollectionContent.CreateFromMeshes(meshDecoders);
        }
    }
        
}
