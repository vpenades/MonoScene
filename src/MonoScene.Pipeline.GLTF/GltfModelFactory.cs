using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

using MonoScene.Graphics.Content;

namespace MonoScene.Graphics.Pipeline
{
    public class GltfModelFactory
    {
        #region lifecycle
        public GltfModelFactory(GraphicsDevice device)
        {
            _Device = device;

            TagConverter = item => item.Extras.ToJson();
        }

        #endregion

        #region data

        private readonly GraphicsDevice _Device;

        #endregion

        #region properties
        
        public bool UseBasicEffects { get; set; }

        public Converter<SharpGLTF.Schema2.ExtraProperties, Object> TagConverter { get; set; }

        #endregion

        #region API

        public DeviceModelCollection LoadModelFromEmbeddedResource(Assembly assembly, string resourcePath)
        {
            resourcePath = assembly
                .GetManifestResourceNames()
                .FirstOrDefault(item => item.EndsWith(resourcePath));

            using(var s = assembly.GetManifestResourceStream(resourcePath))
            {
                return ReadModel(s);
            }            
        }

        /// <summary>
        /// Loads a glTF or a glb from the file system.
        /// </summary>
        public DeviceModelCollection LoadModel(string filePath)
        {
            var model = SharpGLTF.Schema2.ModelRoot.Load(filePath, SharpGLTF.Validation.ValidationMode.TryFix);

            return ReadModel(model);
        }

        /// <summary>
        /// Loads a glb from the file system (no, plain glTFs can't be loaded from stream)
        /// </summary>
        public DeviceModelCollection ReadModel(System.IO.Stream glbModelStream)
        {
            var model = SharpGLTF.Schema2.ModelRoot.ReadGLB(glbModelStream, SharpGLTF.Validation.ValidationMode.TryFix);

            return ReadModel(model);
        }

        public DeviceModelCollection LoadModel(System.IO.FileInfo finfo)
        {
            var model = SharpGLTF.Schema2.ModelRoot.Load(finfo.FullName, SharpGLTF.Validation.ValidationMode.TryFix);

            return ReadModel(model);
        }

        public DeviceModelCollection ReadModel(SharpGLTF.Schema2.ModelRoot model)
        {
            var factory = UseBasicEffects ? (MeshFactory)new ClassicMeshFactory(_Device) : new PBRMeshFactory(_Device);

            return ConvertToXna(model, factory);
        }

        #endregion

        #region static API

        public DeviceModelCollection ConvertToXna(SharpGLTF.Schema2.ModelRoot srcModel, MeshFactory meshFactory)
        {
            if (meshFactory == null) throw new ArgumentNullException();

            return ConvertToContent(srcModel).ToDeviceModelCollection(meshFactory);
        }

        public ModelCollectionContent ConvertToContent(SharpGLTF.Schema2.ModelRoot srcModel)
        {
            // create a mesh decoder for each mesh

            var meshDecoders = srcModel.LogicalMeshes.ToXnaDecoders(TagConverter);
            var meshContent = MeshCollectionContent.CreateFromMeshes(meshDecoders);

            // build the armatures and models

            var armatures = new List<ArmatureContent>();
            var models = new List<ModelTemplate>();

            foreach (var scene in srcModel.LogicalScenes)
            {
                var armatureFactory = new GLTFArmatureFactory(scene, TagConverter);

                for (int i = 0; i < srcModel.LogicalAnimations.Count; ++i)
                {
                    var track = srcModel.LogicalAnimations[i];
                    armatureFactory.SetAnimationTrack(i, track.Name, TagConverter?.Invoke(track), track.Duration);
                }

                // TODO: check if we can share armatures
                var armature = armatureFactory.CreateArmature();
                armatures.Add(armature);

                var model = armatureFactory.CreateModel(scene, armature, meshDecoders);
                model.Tag = TagConverter?.Invoke(scene);

                models.Add(model);
            }
            
            // coalesce all resources into a container class:

            return new ModelCollectionContent(meshContent, armatures.ToArray(), models.ToArray(), srcModel.DefaultScene.LogicalIndex);
        }              

        public MeshCollectionContent ReadMeshContent(IEnumerable<SharpGLTF.Schema2.Mesh> meshes)
        {
            var meshDecoders = meshes.ToXnaDecoders(TagConverter);

            return MeshCollectionContent.CreateFromMeshes(meshDecoders);
        }

        #endregion
    }
        
}
