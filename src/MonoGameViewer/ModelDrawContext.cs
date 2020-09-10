using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SharpGLTF.Runtime;

namespace MonoGameViewer
{
    class ModelDrawContext
    {
        #region lifecycle                

        public ModelDrawContext(GraphicsDeviceManager graphics, Matrix cameraMatrix) : this(graphics.GraphicsDevice, cameraMatrix)
        {
            
        }

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

        private float _Exposure = 25;
        private Vector3 _AmbientLight = Vector3.Zero;
        private readonly PBRPunctualLight[] _PunctualLights = new PBRPunctualLight[3];

        #endregion

        #region API
        
        public void SetExposure(float exposure) { _Exposure = exposure; }

        public void SetAmbientLight(Vector3 color) { _AmbientLight = color; }

        public void SetPunctualLight(int idx, PBRPunctualLight l) { _PunctualLights[idx] = l; }        

        public void DrawModelInstance(MonoGameModelInstance model, Matrix world)
        {
            foreach (var e in model.Template.Effects) UpdateMaterial(e);

            model.Draw(_Projection, _View, world);
        }

        public void UpdateMaterial(Effect effect)
        {
            if (effect is IEffectFog fog)
            {
                fog.FogEnabled = false;
            }

            if (effect is IEffectLights classicLights)
            {
                // let's try to approximate PBR lights to classic lights...

                classicLights.LightingEnabled = true;

                var expSigma4 = 4f - 4f / (1f + _Exposure);

                classicLights.AmbientLightColor = _AmbientLight * expSigma4;

                _PunctualLights[0].ApplyTo(classicLights.DirectionalLight0, _Exposure);
                _PunctualLights[1].ApplyTo(classicLights.DirectionalLight1, _Exposure);
                _PunctualLights[2].ApplyTo(classicLights.DirectionalLight2, _Exposure);
            }

            if (effect is PBRPunctualLight.IEffect pbrLights)
            {
                pbrLights.Exposure = _Exposure;
                pbrLights.AmbientLightColor = _AmbientLight;

                for (int i = 0; i < _PunctualLights.Length; ++i)
                {
                    pbrLights.SetPunctualLight(i, _PunctualLights[i]);
                }
            }
        }

        #endregion
    }
}
