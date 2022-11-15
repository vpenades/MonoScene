using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Oculus;


namespace Microsoft.Xna.Framework
{
    /// <inheritdoc/>    
    public class XRGame : Game
    {
        #region Lifecycle

        public XRGame()
        {
            graphics = new GraphicsDeviceManager(this);

            // 90Hz Frame rate for oculus
            TargetElapsedTime = TimeSpan.FromTicks(111111);
            IsFixedTimeStep = true;
            graphics.SynchronizeWithVerticalRetrace = false;

            // we don't care is the main window is Focuses or not
            // because we render on the Oculus surface.
            InactiveSleepTime = TimeSpan.FromSeconds(0);

            // OVR requirees at least DX feature level 10.0
            graphics.GraphicsProfile = GraphicsProfile.FL10_0;

            // create oculus device
            ovrDevice = new OvrDevice(graphics);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { ovrDevice?.Dispose(); }

            base.Dispose(disposing);
        }

        #endregion                

        #region Fields

        GraphicsDeviceManager graphics;

        OvrDevice ovrDevice;

        public Matrix HeadMatrix { get; private set; }

        #endregion

        #region Update        

        public HandsState GetHandsState()
        {
            return ovrDevice.GetHandsState();
        }           

        protected void UpdateXRDevice(Matrix headMatrix)
        {
            this.HeadMatrix = headMatrix;

            if (!ovrDevice.IsConnected)
            {
                try
                {
                    // Initialize Oculus VR
                    int ovrCreateResult = ovrDevice.CreateDevice();
                    if (ovrCreateResult == 0) { }
                }
                catch (Exception ovre)
                {
                    System.Diagnostics.Debug.WriteLine(ovre.Message);
                }
            }
        }

        #endregion

        #region Draw

        protected virtual void DrawStereo(GameTime gameTime, out Matrix leftView, out Matrix rightView)
        {
            leftView = Matrix.Identity;
            rightView = Matrix.Identity;

            if (!ovrDevice.IsConnected) return;

            // draw on VR headset            
            if (ovrDevice.BeginFrame() < 0) return;

            var headsetState = ovrDevice.GetHeadsetState();

            // draw each eye on a rendertarget
            for (int eye = 0; eye < 2; eye++)
            {
                RenderTarget2D rt = ovrDevice.GetEyeRenderTarget(eye);
                GraphicsDevice.SetRenderTarget(rt);                

                // VR eye view and projection
                var view = headsetState.GetEyeView(eye);

                Matrix globalWorld = this.HeadMatrix;
                view = Matrix.Invert(globalWorld) * view;

                if (eye == 0) leftView = view;
                if (eye == 1) rightView = view;

                var context = new XRSceneContext(view, (near, far) => ovrDevice.CreateProjection(eye, near, far));

                DrawScene(gameTime, context);

                // Resolve eye rendertarget
                GraphicsDevice.SetRenderTarget(null);
                // submit eye rendertarget
                ovrDevice.CommitRenderTarget(eye, rt);
            }

            // submit frame
            int result = ovrDevice.EndFrame();            

            return;
        }        

        protected virtual void DrawScene(GameTime gameTime, XRSceneContext sceneContext)
        {
            // prepare XR drawable components
            foreach(var component in this.Components.OfType<XRGameComponent>())
            {
                component.SetSceneContext(sceneContext);
            }

            // draw any drawable components
            base.Draw(gameTime);
        }

        #endregion
    }
}
