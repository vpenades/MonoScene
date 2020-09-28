using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Helper class for rendering <see cref="ModelLayerInstance"/> models.
    /// </summary>
    public struct ModelDrawingContext
    {
        #region lifecycle

        public ModelDrawingContext(GraphicsDevice graphics)
        {
            _Device = graphics;

            _Device.DepthStencilState = DepthStencilState.Default;            

            float fieldOfView = MathHelper.PiOver4;
            float nearClipPlane = 0.01f;
            float farClipPlane = 1000;            

            _Projection = Matrix.CreatePerspectiveFieldOfView(fieldOfView, graphics.Viewport.AspectRatio, nearClipPlane, farClipPlane);

            _View = Matrix.Invert(Matrix.Identity);
            _DistanceComparer = ModelLayerInstance.GetDistanceComparer(-_View.Translation);
        }

        #endregion

        #region data

        private GraphicsDevice _Device;
        private Matrix _Projection;
        private Matrix _View;
        private IComparer<ModelLayerInstance> _DistanceComparer;

        private static readonly HashSet<Effect> _SceneEffects = new HashSet<Effect>();
        private static readonly List<ModelLayerInstance> _SceneInstances = new List<ModelLayerInstance>();

        #endregion

        #region API

        public void SetCamera(Matrix cameraMatrix)
        {
            _View = Matrix.Invert(cameraMatrix);

            _DistanceComparer = ModelLayerInstance.GetDistanceComparer(-_View.Translation);
        }

        public void SetProjection(Matrix projectionMatrix)
        {
            _Projection = projectionMatrix;
        }

        public void DrawMesh(PBREnvironment environment, RuntimeModelMesh mesh, Matrix worldMatrix)
        {
            foreach (var e in mesh.OpaqueEffects)
            {               
                ModelLayerInstance.UpdateProjViewTransforms(e, _Projection, _View);
                ModelLayerInstance.UpdateWorldTransforms(e, worldMatrix);
                environment.ApplyTo(e);
            }

            mesh.DrawOpaque();

            foreach (var e in mesh.TranslucidEffects)
            {                
                ModelLayerInstance.UpdateProjViewTransforms(e, _Projection, _View);
                ModelLayerInstance.UpdateWorldTransforms(e, worldMatrix);
                environment.ApplyTo(e);
            }

            mesh.DrawTranslucid();
        }

        /// <summary>
        /// Draw a single model instance
        /// </summary>
        /// <param name="environment">Defines the athmospheric and lighting environment to use for the render.</param>
        /// <param name="modelInstance">Defines the instance that is going to be rendered.</param>
        /// <remarks>
        /// Rendering models one by one is accepted, but some features like translucent parts sortings will not work
        /// unless you manually render the models in the correct order.
        /// </remarks>
        public void DrawModelInstance(PBREnvironment environment, ModelLayerInstance modelInstance)
        {
            foreach (var e in modelInstance.Template.SharedEffects)
            {
                environment.ApplyTo(e);
                ModelLayerInstance.UpdateProjViewTransforms(e, _Projection, _View);
            }

            modelInstance.Draw(_Projection, _View);
        }

        /// <summary>
        /// Draws a batch of model instances.
        /// </summary>
        /// <param name="environment">Defines the athmospheric and lighting environment to use for the render.</param>
        /// <param name="modelInstances">A batch of model instances.</param>
        /// <remarks>
        /// Rendering multiple models in a batch has a number of advantages over rendering models one by one:
        /// - It allows splitting the rendering between opaque and translucent parts, which are rendered in the correct
        ///   order to preserve rendering correctness.
        /// - Less redundant calls.
        /// - Futher optimizations are possible, like batching instances that share the same template model in a single
        ///   drawing call.
        /// - Possibility to add shadows, where some instances cast shadows over others.
        /// </remarks>
        public void DrawSceneInstances(PBREnvironment environment, params ModelLayerInstance[] modelInstances)
        {
            // todo: fustrum culling goes here

            _SceneInstances.Clear();
            _SceneInstances.AddRange(modelInstances);
            _SceneInstances.Sort(_DistanceComparer);

            // gather all effects from all visible instances.
            _SceneEffects.Clear();
            _SceneEffects.UnionWith(_SceneInstances.SelectMany(item => item.Template.SharedEffects));

            // set Projection & View on all visible effects.

            foreach (var e in _SceneEffects)
            {
                ModelLayerInstance.UpdateProjViewTransforms(e, _Projection, _View);
                // todo: set env.Exposure
                // todo: set env.AmbientLight
            }

            // todo: find the closest lights for each visible instance.

            // render opaque parts from closest to farthest

            foreach (var instance in _SceneInstances)
            {
                foreach (var e in instance.Template.SharedEffects) environment.ApplyTo(e);
                instance.DrawOpaqueParts();
            }

            // render translucid parts from farthest to closest

            _SceneInstances.Reverse();

            foreach (var instance in _SceneInstances)
            {
                foreach (var e in instance.Template.SharedEffects) environment.ApplyTo(e);
                instance.DrawTranslucidParts();
            }
        }

        #endregion
    }    
}
