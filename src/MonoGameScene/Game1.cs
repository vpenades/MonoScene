using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using SharpGLTF.Runtime;

namespace MonoGameScene
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        #region lifecycle

        public Game1()
        {
            _Graphics = new GraphicsDeviceManager(this);
            _Graphics.GraphicsProfile = GraphicsProfile.HiDef;

            Content.RootDirectory = "Content";
        }
        
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            this.Window.Title = "SharpGLTF - MonoGame Scene";
            this.Window.AllowUserResizing = true;
            this.Window.AllowAltF4 = true;

            base.Initialize();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #endregion

        #region resources

        private readonly GraphicsDeviceManager _Graphics;
        
        // these are the actual hardware resources that represent every model's geometry.        
        
        SharpGLTF.Runtime.MonoGameDeviceContent<SharpGLTF.Runtime.MonoGameModelTemplate> _AvodadoTemplate;
        SharpGLTF.Runtime.MonoGameDeviceContent<SharpGLTF.Runtime.MonoGameModelTemplate> _BrainStemTemplate;
        SharpGLTF.Runtime.MonoGameDeviceContent<SharpGLTF.Runtime.MonoGameModelTemplate> _CesiumManTemplate;
        SharpGLTF.Runtime.MonoGameDeviceContent<SharpGLTF.Runtime.MonoGameModelTemplate> _HauntedHouseTemplate;
        SharpGLTF.Runtime.MonoGameDeviceContent<SharpGLTF.Runtime.MonoGameModelTemplate> _SharkTemplate;

        #endregion

        #region content loading

        protected override void LoadContent()
        {
            var loader = SharpGLTF.Runtime.Content.LoaderContext.CreateLoaderContext(this.GraphicsDevice);

            _AvodadoTemplate = loader.LoadDeviceModel("Models\\Avocado.glb");
            _BrainStemTemplate = loader.LoadDeviceModel("Models\\BrainStem.glb");
            _CesiumManTemplate = loader.LoadDeviceModel( "Models\\CesiumMan.glb");
            _HauntedHouseTemplate = loader.LoadDeviceModel("Models\\haunted_house.glb");
            _SharkTemplate = loader.LoadDeviceModel("Models\\shark.glb");
        }
        
        protected override void UnloadContent()
        {
            _AvodadoTemplate?.Dispose();
            _AvodadoTemplate = null;

            _BrainStemTemplate?.Dispose();
            _BrainStemTemplate = null;

            _CesiumManTemplate?.Dispose();
            _CesiumManTemplate = null;

            _HauntedHouseTemplate?.Dispose();
            _HauntedHouseTemplate = null;

            _SharkTemplate?.Dispose();
            _SharkTemplate = null;
        }

        #endregion

        #region game loop

        private PBREnvironment _LightsAndFog = PBREnvironment.CreateDefault();

        // these are the scene instances we create for every glTF model we want to render on screen.
        // Instances are designed to be as lightweight as possible, so it should not be a problem to
        // create as many of them as you need at runtime.
        private MonoGameModelInstance _HauntedHouse;
        private MonoGameModelInstance _BrainStem;
        private MonoGameModelInstance _Avocado;
        private MonoGameModelInstance _CesiumMan1;
        private MonoGameModelInstance _CesiumMan2;
        private MonoGameModelInstance _CesiumMan3;
        private MonoGameModelInstance _CesiumMan4;
        private MonoGameModelInstance _Shark;

        protected override void Update(GameTime gameTime)
        {
            // For Mobile devices, this logic will close the Game when the Back button is pressed
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            // create as many instances as we need from the templates

            if (_Avocado == null) _Avocado = _AvodadoTemplate.Content.CreateInstance();
            if (_HauntedHouse == null) _HauntedHouse = _HauntedHouseTemplate.Content.CreateInstance();
            if (_BrainStem == null) _BrainStem = _BrainStemTemplate.Content.CreateInstance();

            if (_CesiumMan1 == null) _CesiumMan1 = _CesiumManTemplate.Content.CreateInstance();
            if (_CesiumMan2 == null) _CesiumMan2 = _CesiumManTemplate.Content.CreateInstance();
            if (_CesiumMan3 == null) _CesiumMan3 = _CesiumManTemplate.Content.CreateInstance();
            if (_CesiumMan4 == null) _CesiumMan4 = _CesiumManTemplate.Content.CreateInstance();

            if (_Shark == null) _Shark = _SharkTemplate.Content.CreateInstance();

            // animate each instance individually.

            var animTime = (float)gameTime.TotalGameTime.TotalSeconds;

            _Avocado.WorldMatrix = Matrix.CreateScale(30) * Matrix.CreateRotationY(animTime * 0.3f) * Matrix.CreateTranslation(-4, 4, 1);
            _HauntedHouse.WorldMatrix = Matrix.CreateScale(20) * Matrix.CreateRotationY(1);

            _BrainStem.WorldMatrix = Matrix.CreateTranslation(0, 0.5f, 8);
            _BrainStem.Controller.SetAnimationFrame(0, 0.7f* animTime);

            _CesiumMan1.WorldMatrix = Matrix.CreateTranslation(-3, 0, 5);
            _CesiumMan1.Controller.SetAnimationFrame(0, 0.3f);

            _CesiumMan2.WorldMatrix = Matrix.CreateTranslation(-2, 0, 5);
            _CesiumMan2.Controller.SetAnimationFrame(0, 0.5f * animTime);

            _CesiumMan3.WorldMatrix = Matrix.CreateTranslation(2, 0, 5);
            _CesiumMan3.Controller.SetAnimationFrame(0, 1.0f * animTime);

            _CesiumMan4.WorldMatrix = Matrix.CreateTranslation(3, 0, 5);
            _CesiumMan4.Controller.SetAnimationFrame(0, 1.5f * animTime);

            _Shark.WorldMatrix = Matrix.CreateTranslation(5, 3, -6);
            _Shark.Controller.SetAnimationFrame(0, 1.0f * animTime);

            base.Update(gameTime);
        }        

        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            base.Draw(gameTime);

            // setup drawing context

            var animTime = (float)gameTime.TotalGameTime.TotalSeconds;

            var lookAt = new Vector3(0, 2, 0);
            var camPos = new Vector3((float)Math.Sin(animTime*0.5f) * 2, 2, 12);
            var camera = Matrix.CreateWorld(camPos, lookAt - camPos, Vector3.UnitY);            

            var ctx = new MonoGameDrawingContext(_Graphics.GraphicsDevice);

            ctx.SetCamera(camera);

            // draw all the instances.            

            ctx.DrawSceneInstances
                (

                // environment lights and fog
                _LightsAndFog,

                // all model instances
                _Avocado, _HauntedHouse, _BrainStem, _CesiumMan1, _CesiumMan2, _CesiumMan3, _CesiumMan4, _Shark

                );
        }
        
        #endregion
    }
}
