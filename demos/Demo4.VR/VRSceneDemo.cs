using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoScene.Graphics;

namespace Primitives3D
{
    class VRSceneDemo : XRGameComponent
    {
        #region lifecycle
        public VRSceneDemo(XRGame game) : base(game)
        {
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            
            var gltfFactory = new MonoScene.Graphics.Pipeline.GltfModelFactory(this.GraphicsDevice);

            _HouseTemplate = gltfFactory.LoadModel("Content\\haunted_house.glb");
            _CharTemplate = gltfFactory.LoadModel("Content\\CesiumMan.glb");
        }

        protected override void UnloadContent()
        {
            _HouseTemplate?.Dispose();
            _CharTemplate?.Dispose();

            base.UnloadContent();            
        }
        #endregion

        #region data

        private PBREnvironment _LightsAndFog = PBREnvironment.CreateDefault();

        private DeviceModelCollection _HouseTemplate;
        private DeviceModelCollection _CharTemplate;

        private ModelInstance _HouseView1;
        private ModelInstance _HouseView2;
        private ModelInstance _HouseView3;
        private ModelInstance _HouseView4;

        private ModelInstance _CharView1;

        #endregion

        #region update

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _HouseView1 ??= _HouseTemplate.DefaultModel.CreateInstance();            
            _HouseView1.WorldMatrix = Matrix.CreateScale(60) * Matrix.CreateTranslation(0, 0, -10);

            _HouseView2 ??= _HouseTemplate.DefaultModel.CreateInstance();
            _HouseView2.WorldMatrix = Matrix.CreateScale(60) * Matrix.CreateTranslation(-12, 0, 1);

            _HouseView3 ??= _HouseTemplate.DefaultModel.CreateInstance();
            _HouseView3.WorldMatrix = Matrix.CreateScale(60) * Matrix.CreateTranslation(11, 0, 0);

            _HouseView4 ??= _HouseTemplate.DefaultModel.CreateInstance();
            _HouseView4.WorldMatrix = Matrix.CreateScale(60) * Matrix.CreateTranslation(0, 0, 15);

            _CharView1 ??= _CharTemplate.DefaultModel.CreateInstance();
            _CharView1.WorldMatrix = Matrix.CreateScale(2) * Matrix.CreateTranslation(0, 0, -3);
            _CharView1.Armature.SetAnimationFrame(0, (float)gameTime.TotalGameTime.TotalSeconds);
        }

        #endregion

        #region draw

        public override void Draw(GameTime gameTime, XRSceneContext sceneContext)
        {
            var dc = new ModelDrawingContext(this.GraphicsDevice);
            
            dc.SetCamera(Matrix.Invert(sceneContext.ViewMatrix));            
            dc.SetProjectionMatrix(sceneContext.GetProjectionMatrix(0.1f, 100));

            dc.DrawSceneInstances(_LightsAndFog, _HouseView1, _HouseView2, _HouseView3, _HouseView4, _CharView1);

            base.Draw(gameTime, sceneContext);
        }

        #endregion
    }
}
