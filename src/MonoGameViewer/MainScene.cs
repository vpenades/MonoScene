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

        private readonly ClientLight[] _Lights = new ClientLight[] { ClientLight.CreateDefault(0), ClientLight.CreateDefault(1), ClientLight.CreateDefault(2) };

        #endregion

        #region properties

        public ClientLight[] Lights => _Lights;

        #endregion

        #region API

        public void LoadModel(string filePath, bool useBasicEffects)
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

            var loader = useBasicEffects
                ? new SharpGLTF.Runtime.BasicEffectsLoaderContext(this.GraphicsDevice)
                : SharpGLTF.Runtime.LoaderContext.CreateLoaderContext(this.GraphicsDevice);            

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
            ctx.SetLight(0, _Lights[0].ToPBR());
            ctx.SetLight(1, _Lights[1].ToPBR());
            ctx.SetLight(2, _Lights[2].ToPBR());

            var xform = Matrix.CreateFromQuaternion(_Rotation);

            if (_ModelInstance != null) ctx.DrawModelInstance(_ModelInstance, xform);
        }

        #endregion
    }

    public class ClientLight
    {
        public static ClientLight CreateDefault(int idx)
        {
            var l = new ClientLight();
            l.Intensity = 15;
            l.Color = System.Windows.Media.Colors.White;

            switch(idx)
            {
                case 0:
                    l.DirectionAngle = 60;
                    l.ElevationAngle = 30;
                    l.Intensity = 80;
                    break;

                case 1:
                    l.DirectionAngle = -70;
                    l.ElevationAngle = 60;
                    l.Color = System.Windows.Media.Colors.LightBlue;
                    break;

                case 2:
                    l.DirectionAngle = 20;
                    l.ElevationAngle = -50;
                    l.Color = System.Windows.Media.Colors.LightBlue;
                    break;
            }            

            return l;
        }

        [PropertyTools.DataAnnotations.Slidable(-180,180)]
        [PropertyTools.DataAnnotations.WideProperty]
        public int DirectionAngle { get; set; }

        [PropertyTools.DataAnnotations.Slidable(-90, 90)]
        [PropertyTools.DataAnnotations.WideProperty]
        public int ElevationAngle { get; set; }
        public System.Windows.Media.Color Color { get; set; }


        [PropertyTools.DataAnnotations.Slidable(0,100)]
        [PropertyTools.DataAnnotations.WideProperty]
        public int Intensity { get; set; }

        public PBRLight ToPBR()
        {
            float yaw = (float)(DirectionAngle * Math.PI) / 180.0f;
            float pitch = (float)(ElevationAngle * Math.PI) / 180.0f;
            var xform = Matrix.CreateFromYawPitchRoll(yaw + 3.141592f, pitch, 0);
            var dir = Vector3.Transform(Vector3.UnitZ, xform);

            var color = new Vector3(Color.ScR, Color.ScG, Color.ScB);

            return PBRLight.Directional(dir, color, (float)Intensity / 10.0f);
        }
    }
}
