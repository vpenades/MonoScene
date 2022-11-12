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
        
        public XRSceneContext SceneContext { get; private set; }

        protected HandsState GetHandsState()
        {
            return XRGame.GetHandsState();
        }

        /// <summary>
        /// This method must be called before calling <see cref="Draw(GameTime)"/>
        /// </summary>
        /// <param name="context">the context containing scene transforms</param>        
        internal void SetSceneContext(XRSceneContext context)
        {
            SceneContext = context;
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public override void Draw(GameTime gameTime)
        {
            Draw(gameTime, SceneContext);

            base.Draw(gameTime);
        }

        public virtual void Draw(GameTime gameTime, XRSceneContext sceneContext)
        {

        }
    }
}
