using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.WpfCore.MonoGameControls;

using SharpGLTF.Validation;

namespace MonoGameViewer
{
    public class MainScene : MonoGameViewModel
    {
        #region data

        SharpGLTF.Runtime.MonoGameDeviceContent<SharpGLTF.Runtime.MonoGameModelTemplate> _ModelTemplate;
        BoundingSphere _ModelBounds;

        private SharpGLTF.Runtime.MonoGameModelInstance _ModelInstance;

        private Quaternion _Rotation = Quaternion.Identity;

        #endregion

        #region API

        public void LoadModel(string filePath)
        {
            SharpGLTF.Schema2.ModelRoot model = null;

            if (filePath.ToLower().EndsWith(".zip"))
            {
                model = SharpGLTF.IO.ZipReader.LoadSchema2(filePath, ValidationMode.TryFix);
            }
            else
            {
                model = SharpGLTF.Schema2.ModelRoot.Load(filePath, ValidationMode.TryFix);
            }


            if (_ModelTemplate != null) { _ModelTemplate.Dispose(); _ModelTemplate = null; }            

            var loader = SharpGLTF.Runtime.PBREffectsLoaderContext.CreateLoaderContext(this.GraphicsDevice);
            _ModelTemplate = loader.CreateDeviceModel(model);
            _ModelBounds = _ModelTemplate.Instance.Bounds;


            var points = SharpGLTF.Schema2.Toolkit.EvaluateTriangles(model.DefaultScene)
                .SelectMany(item => new[] { item.A.GetGeometry().GetPosition(), item.B.GetGeometry().GetPosition(), item.C.GetGeometry().GetPosition() })
                .Distinct()
                .Select(item => new Vector3(item.X, item.Y, item.Z))
                .ToList();

            _ModelBounds = BoundingSphere.CreateFromPoints(points);

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

            if (_ModelInstance == null) return;

            _ModelInstance.Controller.SetAnimationFrame(0, (float)gameTime.TotalGameTime.TotalSeconds);

            var bounds = _ModelBounds;

            var lookAt = bounds.Center;
            var camPos = bounds.Center + new Vector3(0, 0, bounds.Radius * 3);

            var camera = Matrix.CreateWorld(camPos, lookAt - camPos, Vector3.UnitY);

            var ctx = new ModelDrawContext(this.GraphicsDevice, camera);

            var xform = Matrix.CreateFromQuaternion(_Rotation);

            if (_ModelInstance != null) ctx.DrawModelInstance(_ModelInstance, xform);
        }

        #endregion
    }
}
