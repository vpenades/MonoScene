using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Oculus;

namespace Primitives3D
{
    public class VRGameDemo : Microsoft.Xna.Framework.XRGame
    {
        #region Lifecycle
        public VRGameDemo()
        {
            Content.RootDirectory = "Content";            
        }

        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            var component1 = new VRSceneDemo(this);
            component1.Initialize();
            this.Components.Add(component1);

            var component2 = new VRHandsLayer(this);
            component2.Initialize();
            this.Components.Add(component2);
        }

        #endregion                

        #region Fields        

        KeyboardState currentKeyboardState;
        KeyboardState lastKeyboardState;
        GamePadState currentGamePadState;
        GamePadState lastGamePadState;
        TouchControllerState currentTouchControllerState;
        TouchControllerState lastTouchControllerState;        

        #endregion

        #region Update


        /// <inheritdoc/>
        protected override void Update(GameTime gameTime)
        {
            UpdateXRDevice(Matrix.CreateTranslation(0, 1.7f, 0));

            HandleInput();

            base.Update(gameTime);
        }

        #endregion

        #region Handle Input

        /// <summary>
        /// Handles input for quitting or changing settings.
        /// </summary>
        void HandleInput()
        {
            lastKeyboardState = currentKeyboardState;
            lastGamePadState = currentGamePadState;
            lastTouchControllerState = currentTouchControllerState;

            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);
            currentTouchControllerState = TouchController.GetState(TouchControllerType.Touch);
            //if (ovrDevice.IsConnected)
            var handsState = this.GetHandsState();

            // Check for exit.
            if (IsPressed(Keys.Escape, Buttons.Back))
            {
                Exit();
            }

            // Change primitive?
            if (IsPressed(Keys.A, Buttons.A))
            {
                // currentPrimitiveIndex = (currentPrimitiveIndex + 1) % primitives.Count;
            }

            // Change color?
            if (IsPressed(Keys.B, Buttons.B))
            {
                // currentColorIndex = (currentColorIndex + 1) % colors.Count;
            }

            // Toggle wireframe?
            if (IsPressed(Keys.Y, Buttons.Y))
            {
                // isWireframe = !isWireframe;
            }

        }

        /// <summary>
        /// Checks whether the specified key or button has been pressed.
        /// </summary>
        bool IsPressed(Keys key, Buttons button)
        {
            return (currentKeyboardState.IsKeyDown(key) &&
                    lastKeyboardState.IsKeyUp(key)) ||
                   (currentGamePadState.IsButtonDown(button) &&
                    lastGamePadState.IsButtonUp(button)) ||
                   (currentTouchControllerState.IsButtonPressed(button) &&
                    !lastTouchControllerState.IsButtonPressed(button));
        }

        #endregion

        #region Draw


        /// <inheritdoc/>        
        protected override void Draw(GameTime gameTime)
        {
            this.DrawStereo(gameTime, out var leftView, out var rightView);

            if (true)
            {
                // draw on PC screen
                GraphicsDevice.SetRenderTarget(null);

                var aspect = GraphicsDevice.Viewport.AspectRatio;

                var context = new XRSceneContext(leftView, (near, far) => Matrix.CreatePerspectiveFieldOfView(1, aspect, near, far));

                DrawScene(gameTime, context);
            }
        }

        protected override void DrawScene(GameTime gameTime, XRSceneContext sceneContext)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.DrawScene(gameTime, sceneContext);
        }

        #endregion
    }
}
