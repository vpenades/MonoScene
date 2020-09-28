using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    [System.Diagnostics.DebuggerDisplay("{Template.Name} {MeshIndex}")]
    public readonly struct DrawableInstance
    {
        internal DrawableInstance(IDrawableTemplate t, IMeshTransform xform)
        {
            Template = t;
            Transform = xform;
        }

        /// <summary>
        /// Defines "what to draw"
        /// </summary>
        public readonly IDrawableTemplate Template;

        /// <summary>
        /// Defines "where to draw"
        /// </summary>
        public readonly IMeshTransform Transform;
    }
}
