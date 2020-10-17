using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

using SharpGLTF.Runtime;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public static class FormatGLTF
    {
        public static ModelCollectionContent LoadModelFromEmbeddedResource(Assembly assembly, string resourcePath, GraphicsDevice graphics, bool useBasicEffects = false)
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
        public static ModelCollectionContent LoadModel(string filePath, GraphicsDevice graphics, bool useBasicEffects = false)
        {
            var model = SharpGLTF.Schema2.ModelRoot.Load(filePath, SharpGLTF.Validation.ValidationMode.TryFix);

            return ReadModel(model, graphics, useBasicEffects);
        }

        /// <summary>
        /// Loads a glb from the file system (no, plain glTFs can't be loaded from stream)
        /// </summary>
        public static ModelCollectionContent ReadModel(System.IO.Stream glbModelStream, GraphicsDevice graphics, bool useBasicEffects = false)
        {
            var model = SharpGLTF.Schema2.ModelRoot.ReadGLB(glbModelStream, SharpGLTF.Validation.ValidationMode.TryFix);

            return ReadModel(model, graphics, useBasicEffects);
        }

        public static ModelCollectionContent LoadModel(System.IO.FileInfo finfo, GraphicsDevice graphics, bool useBasicEffects = false)
        {
            var model = SharpGLTF.Schema2.ModelRoot.Load(finfo.FullName, SharpGLTF.Validation.ValidationMode.TryFix);

            return ReadModel(model, graphics, useBasicEffects);
        }

        public static ModelCollectionContent ReadModel(SharpGLTF.Schema2.ModelRoot model, GraphicsDevice graphics, bool useBasicEffects = false)
        {
            var factory = useBasicEffects ? (MeshFactory)new ClassicMeshFactory(graphics) : new PBRMeshFactory(graphics);

            return ConvertToXna(model, factory);
        }

        public static MeshCollection ConvertToXna(IEnumerable<SharpGLTF.Schema2.Mesh> meshes, MeshFactory meshFactory)
        {
            if (meshFactory == null) throw new ArgumentNullException();

            // build the meshes

            var meshDecoders = meshes.ToXna();
            return meshFactory.CreateMeshCollection(meshDecoders);
        }

        public static ModelCollectionContent ConvertToXna(SharpGLTF.Schema2.ModelRoot srcModel, MeshFactory meshFactory)
        {
            if (meshFactory == null) throw new ArgumentNullException();

            // create a mesh decoder for each mesh

            var meshDecoders = srcModel.LogicalMeshes.ToXna();            

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

            // convert mesh decoders to actual XNA mesh resources

            var meshCollection = meshFactory.CreateMeshCollection(meshDecoders);

            // coalesce all resources into a container class:

            return new ModelCollectionContent(meshCollection, armatures.ToArray(), models.ToArray(), srcModel.DefaultScene.LogicalIndex);
        }       
    }
}
