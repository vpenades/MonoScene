using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpGLTF.Runtime;

namespace MonoGameScene
{
    /// <summary>
    /// Small helper for rendering MonoGame models.
    /// </summary>
    struct ModelDrawContext
    {
        #region lifecycle

        public ModelDrawContext(GraphicsDevice graphics, Matrix cameraMatrix)
        {
            _Device = graphics;

            _Device.DepthStencilState = DepthStencilState.Default;

            _View = Matrix.Invert(cameraMatrix);

            float fieldOfView = MathHelper.PiOver4;
            float nearClipPlane = 0.01f;
            float farClipPlane = 1000;

            _DistanceComparer = MonoGameModelInstance.GetDistanceComparer(-_View.Translation);

            _Projection = Matrix.CreatePerspectiveFieldOfView(fieldOfView, graphics.Viewport.AspectRatio, nearClipPlane, farClipPlane);
        }

        #endregion

        #region data

        private GraphicsDevice _Device;
        private Matrix _Projection;
        private Matrix _View;
        private IComparer<MonoGameModelInstance> _DistanceComparer;

        private static readonly List<MonoGameModelInstance> _Instances = new List<MonoGameModelInstance>();

        #endregion

        #region API       

        public void DrawModelInstance(MonoGameModelInstance model, PBREnvironment env)
        {
            foreach (var e in model.Template.Effects) env.ApplyTo(e);

            model.Draw(_Projection, _View);
        }

        public void DrawSceneInstances(PBREnvironment env, params MonoGameModelInstance[] models)
        {
            // todo: fustrum culling goes here
            // todo: find the closest lights for each instance.

            _Instances.Clear();
            _Instances.AddRange(models);
            _Instances.Sort(_DistanceComparer);

            // render opaque parts from closest to farthest

            foreach (var model in _Instances)
            {
                foreach (var e in model.Template.Effects) env.ApplyTo(e);
                model.DrawOpaqueParts(_Projection, _View);
            }

            // render translucid parts from farthest to closest

            _Instances.Reverse();

            foreach (var model in _Instances)
            {
                foreach (var e in model.Template.Effects) env.ApplyTo(e);
                model.DrawTranslucidParts(_Projection, _View);
            }
        }

        #endregion
    }
}
