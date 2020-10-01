using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Demo1
{
    public class Game1 : Game
    {
        #region lifecycle
        public Game1()
        {
            _Graphics = new GraphicsDeviceManager(this);            
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            this.Window.Title = "SharpGLTF - MonoGame Demo 1";
            this.Window.AllowUserResizing = true;
            this.Window.AllowAltF4 = true;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            var gltfModel = SharpGLTF.Schema2.ModelRoot.Load("Content\\WaterBottle.glb");

            var factory = new Microsoft.Xna.Framework.Content.Pipeline.Graphics.PBRMeshFactory(this.GraphicsDevice);

            _MeshCollection = factory.CreateMeshCollection(gltfModel.LogicalMeshes);
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();

            _MeshCollection?.Dispose();
            _MeshCollection = null;            
        }

        #endregion

        #region data

        private GraphicsDeviceManager _Graphics;

        private PBREnvironment _LightsAndFog = PBREnvironment.CreateDefault();

        private MeshCollection _MeshCollection;        

        #endregion

        #region Game Loop

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();            

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);            

            var camPos = Vector3.Zero;
            var mdlPos = new Vector3(0.5f, 0, 0);

            var camX = Matrix.CreateWorld(Vector3.Zero, mdlPos - camPos, Vector3.UnitY);
            var mdlX = Matrix.CreateRotationY(0.25f * (float)gameTime.TotalGameTime.TotalSeconds) * Matrix.CreateTranslation(mdlPos);

            var dc = new ModelDrawingContext(_Graphics.GraphicsDevice);
            dc.NearPlane = 0.1f; // we need to make near plane small because the object is very very close.
            dc.SetCamera(camX);
            dc.DrawMesh(_LightsAndFog, _MeshCollection[0], mdlX);

            base.Draw(gameTime);
        }

        #endregion
    }
}
