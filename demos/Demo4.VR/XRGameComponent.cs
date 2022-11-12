using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Input.Oculus;

namespace Microsoft.Xna.Framework
{
    internal class XRGameComponent : DrawableGameComponent
    {
        public XRGameComponent(XRGame game)
            : base(game)
        {
            XRGame = game;
        }
         
        public XRGame XRGame { get; }
        private Matrix _EyeView;
        private ProjectionDelegate _EyeProjection;        

        protected HandsState GetHandsState()
        {
            return XRGame.GetHandsState();
        }

        /// <summary>
        /// This method must be called before calling <see cref="Draw(GameTime)"/>
        /// </summary>
        /// <param name="view">the view matrix</param>
        /// <param name="projFunction">the projection function</param>
        internal void SetEyeTransforms(Matrix view, ProjectionDelegate projFunction)
        {
            _EyeView = view;
            _EyeProjection = projFunction;
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public override void Draw(GameTime gameTime)
        {
            Draw(gameTime, _EyeView, _EyeProjection);

            base.Draw(gameTime);
        }

        public virtual void Draw(GameTime gameTime, Matrix view, ProjectionDelegate proj)
        {

        }
    }
}
