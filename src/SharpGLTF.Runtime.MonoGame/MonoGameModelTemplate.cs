using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MODELMESH = SharpGLTF.Runtime.RuntimeModelMesh;

namespace SharpGLTF.Runtime
{
    /// <summary>
    /// Represents a container of all the resources that make a 3D model. this is the equivalent to a unity's model prefab.
    /// </summary>
    /// <remarks>
    /// - Hierarchy graphs
    /// - Animation curves
    /// - Meshes : vertex buffers, index buffers, etc
    /// - Materials: effects, textures, etc.
    /// </remarks>    
    public class MonoGameModelTemplate
    {
        #region lifecycle

        public static MonoGameDeviceContent<MonoGameModelTemplate> LoadDeviceModel(GraphicsDevice device, string filePath, Content.LoaderContext context = null)
        {
            var model = Schema2.ModelRoot.Load(filePath, Validation.ValidationMode.TryFix);

            return CreateDeviceModel(device, model, context);
        }

        public static MonoGameDeviceContent<MonoGameModelTemplate> CreateDeviceModel(GraphicsDevice device, Schema2.ModelRoot srcModel, Content.LoaderContext context = null)
        {
            if (context == null) context = new Content.BasicEffectsLoaderContext(device);

            context.Reset();

            var templates = srcModel.LogicalScenes
                .Select(item => SceneTemplate.Create(item, true))
                .ToArray();            

            var srcMeshes = templates
                .SelectMany(item => item.LogicalMeshIds)
                .Distinct()
                .Select(idx => srcModel.LogicalMeshes[idx]);            

            foreach(var srcMesh in srcMeshes)
            {
                context._WriteMesh(srcMesh.Decode());
            }

            var dstMeshes = context.CreateRuntimeModels();            

            var sceneBounds = srcModel
                .LogicalScenes
                .Select(item => item.EvaluateBoundingSphere())
                .Select(item => new BoundingSphere(item.Center.ToXna(),item.Radius))
                .ToArray();

            var mdl = new MonoGameModelTemplate(templates, sceneBounds , srcModel.DefaultScene.LogicalIndex, dstMeshes);

            return new MonoGameDeviceContent<MonoGameModelTemplate>(mdl, context.Disposables.ToArray());
        }
        
        internal MonoGameModelTemplate(SceneTemplate[] scenes,BoundingSphere[] sceneBounds, int defaultSceneIndex, IReadOnlyDictionary<int, MODELMESH> meshes)
        {
            _Meshes = meshes;
            _Effects = _Meshes.Values
                .SelectMany(item => item.OpaqueEffects.Concat(item.TranslucidEffects))
                .Distinct()
                .ToArray();

            _Scenes = scenes;
            _ScenesBounds = sceneBounds;

            _DefaultSceneIndex = defaultSceneIndex;
        }

        #endregion

        #region data
        
        /// <summary>
        /// Meshes shared by all the scenes.
        /// </summary>
        internal readonly IReadOnlyDictionary<int, MODELMESH> _Meshes;

        /// <summary>
        /// Effects shared by all the meshes.
        /// </summary>
        private readonly Effect[] _Effects;

        /// <summary>
        /// Scenes available in this template
        /// </summary>
        private readonly SceneTemplate[] _Scenes;
        private readonly BoundingSphere[] _ScenesBounds;

        private readonly int _DefaultSceneIndex;

        #endregion

        #region properties

        public int SceneCount => _Scenes.Length;

        public IReadOnlyList<Effect> Effects => _Effects;

        public BoundingSphere Bounds => GetBounds(_DefaultSceneIndex);
        
        #endregion

        #region API

        public int IndexOfScene(string sceneName) => Array.FindIndex(_Scenes, item => item.Name == sceneName);

        public BoundingSphere GetBounds(int sceneIndex) => _ScenesBounds[sceneIndex];        

        public MonoGameModelInstance CreateInstance() => CreateInstance(_DefaultSceneIndex);

        public MonoGameModelInstance CreateInstance(int sceneIndex)
        {
            return new MonoGameModelInstance(this, sceneIndex, _Scenes[sceneIndex].CreateInstance());
        }        
        
        #endregion        
    }    
}
