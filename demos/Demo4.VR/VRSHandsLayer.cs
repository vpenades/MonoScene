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
    class VRHandsLayer : XRGameComponent
    {
        #region lifecycle
        public VRHandsLayer(XRGame game) : base(game)
        {
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            
            var gltfFactory = new MonoScene.Graphics.Pipeline.GltfModelFactory(this.GraphicsDevice);

            _LeftTemplate = gltfFactory.LoadModel("Content\\OculusTouchForQuestAndRiftS\\OculusTouchForQuestAndRiftS_Left.gltf");
            _RightTemplate = gltfFactory.LoadModel("Content\\OculusTouchForQuestAndRiftS\\OculusTouchForQuestAndRiftS_Right.gltf");
        }

        protected override void UnloadContent()
        {
            _LeftTemplate?.Dispose();
            _RightTemplate?.Dispose();

            base.UnloadContent();            
        }
        #endregion

        #region data

        private PBREnvironment _LightsAndFog = PBREnvironment.CreateDefault();
        
        private DeviceModelCollection _LeftTemplate;
        private DeviceModelCollection _RightTemplate;

        private ModelInstance _LeftHand;
        private ModelInstance _RightHand;       

        #endregion

        #region update

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _LeftHand ??= _LeftTemplate.DefaultModel.CreateInstance();
            _RightHand ??= _RightTemplate.DefaultModel.CreateInstance();

            var hands = this.GetHandsState();            

            var controllerXForm = Matrix.CreateScale(0.01f) * Matrix.CreateRotationY((float)Math.PI);            

            _LeftHand.WorldMatrix = controllerXForm * hands.LHandTransform * this.XRGame.HeadMatrix;
            _RightHand.WorldMatrix = controllerXForm * hands.RHandTransform * this.XRGame.HeadMatrix;
        }

        #endregion

        #region draw

        public override void Draw(GameTime gameTime, XRSceneContext sceneContext)
        {
            var dc = new ModelDrawingContext(this.GraphicsDevice);
            
            dc.SetCamera(Matrix.Invert(sceneContext.ViewMatrix));            
            dc.SetProjectionMatrix(sceneContext.GetProjectionMatrix(0.1f, 100));

            dc.DrawSceneInstances(_LightsAndFog, _LeftHand, _RightHand);

            base.Draw(gameTime, sceneContext);            
        }

        #endregion
    }
}
