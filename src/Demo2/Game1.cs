using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Demo2
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
            this.Window.Title = "SharpGLTF - MonoGame Demo 2";
            this.Window.AllowUserResizing = true;
            this.Window.AllowAltF4 = true;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _ModelTemplate = Microsoft.Xna.Framework.Content.Pipeline.Graphics.FormatGLTF.LoadModel("Content\\CesiumMan.glb", this.GraphicsDevice);            
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();

            _ModelTemplate?.Dispose();
            _ModelTemplate = null;
        }

        #endregion

        #region data

        private GraphicsDeviceManager _Graphics;

        private PBREnvironment _LightsAndFog = PBREnvironment.CreateDefault();

        private ModelCollectionContent _ModelTemplate;

        #endregion

        #region Game Loop

        private ModelInstance _ModelView1;

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            if (_ModelView1 == null) _ModelView1 = _ModelTemplate.DefaultModel.CreateInstance();

            var mdlPos = new Vector3(3.5f, 0, 0);

            _ModelView1.WorldMatrix = Matrix.CreateRotationY(0.25f * (float)gameTime.TotalGameTime.TotalSeconds) * Matrix.CreateTranslation(mdlPos);
            _ModelView1.Armature.SetAnimationFrame(0, (float)gameTime.TotalGameTime.TotalSeconds);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            var camPos = Vector3.Zero;            

            var camX = Matrix.CreateWorld(Vector3.Zero, _ModelView1.WorldBounds.Center - camPos, Vector3.UnitY);

            var dc = new ModelDrawingContext(_Graphics.GraphicsDevice);

            dc.SetCamera(camX);

            dc.DrawModelInstance(_LightsAndFog, _ModelView1);

            base.Draw(gameTime);
        }

        #endregion
    }
}
