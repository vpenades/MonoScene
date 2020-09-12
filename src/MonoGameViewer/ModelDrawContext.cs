using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SharpGLTF.Runtime;

namespace MonoGameViewer
{
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

            _Projection = Matrix.CreatePerspectiveFieldOfView(fieldOfView, graphics.Viewport.AspectRatio, nearClipPlane, farClipPlane);            
        }

        #endregion

        #region data

        private GraphicsDevice _Device;
        private Matrix _Projection;
        private Matrix _View;        

        #endregion

        #region API       

        public void DrawModelInstance(MonoGameModelInstance model, PBREnvironment env)
        {
            foreach (var e in model.Template.Effects) env.ApplyTo(e);

            model.Draw(_Projection, _View);
        }        

        #endregion
    }
}
