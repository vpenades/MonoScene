using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.WpfCore.MonoGameControls;

namespace MonoGameViewer
{
    public class MainScene : MonoGameViewModel
    {
        SharpGLTF.Runtime.MonoGameDeviceContent<SharpGLTF.Runtime.MonoGameModelTemplate> _ModelTemplate;

        private SharpGLTF.Runtime.MonoGameModelInstance _ModelInstance;

        private Quaternion _Rotation = Quaternion.Identity;

        public void LoadModel(string filePath)
        {
            if (_ModelTemplate != null) { _ModelTemplate.Dispose(); _ModelTemplate = null; }

            var loader = SharpGLTF.Runtime.PBREffectsLoaderContext.CreateLoaderContext(this.GraphicsDevice);
            _ModelTemplate = loader.LoadDeviceModel(filePath);

            _ModelInstance = null;
        }

        public override void UnloadContent()
        {
            base.UnloadContent();

            _ModelTemplate?.Dispose(); _ModelTemplate = null;
        }


        public void RotateModel(float x, float y)
        {
            _Rotation = Quaternion.CreateFromYawPitchRoll(x, y, 0) * _Rotation;
            _Rotation.Normalize();
        }



        public override void Update(GameTime gameTime)
        {
            if (_ModelInstance == null && _ModelTemplate != null) _ModelInstance = _ModelTemplate.Instance.CreateInstance();
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            var lookAt = new Vector3(0, 2, 0);
            var camPos = new Vector3(0, 2, 12);

            var camera = Matrix.CreateWorld(camPos, lookAt - camPos, Vector3.UnitY);

            var ctx = new ModelDrawContext(this.GraphicsDevice, camera);

            var xform = Matrix.CreateFromQuaternion(_Rotation) * Matrix.CreateTranslation(0, 0, -5);

            if (_ModelInstance != null) ctx.DrawModelInstance(_ModelInstance, xform);
        }
    }
}
