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
    /// <summary>
    /// This sample shows how to draw 3D geometric primitives
    /// such as cubes, spheres, and cylinders.
    /// </summary>
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

            var gltfScene = new VRSceneDemo(this);
            gltfScene.Initialize();
            this.Components.Add(gltfScene);
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
            UpdateXRDevice();

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
            var cameraPosition = new Vector3(0, 1.7f, 0);            

            this.DrawStereo(gameTime, cameraPosition, out var leftView, out var rightView);

            if (true)
            {
                // draw on PC screen
                GraphicsDevice.SetRenderTarget(null);

                var aspect = GraphicsDevice.Viewport.AspectRatio;

                DrawScene(gameTime, leftView, (near, far) => Matrix.CreatePerspectiveFieldOfView(1, aspect, near, far));
            }
        }

        protected override void DrawScene(GameTime gameTime, Matrix view, ProjectionDelegate projection)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.DrawScene(gameTime, view, projection);
        }

        #endregion
    }

    


}
