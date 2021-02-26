using System;

namespace MonoScene.Graphics
{
    /// <summary>
    /// Represents a drawable item within a <see cref="ModelInstance.DrawableInstances"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Lifecycle flow:<br/>
    /// <see cref="Content.DrawableContent"/> ➔ <b><see cref="DrawableInstance"/></b>
    /// </para>
    /// </remarks>
    [System.Diagnostics.DebuggerDisplay("{Content.Name} {Content.MeshIndex}")]
    public readonly struct DrawableInstance
    {
        internal DrawableInstance(Content.DrawableContent content)
        {
            Content = content;
            Transform = content.CreateTransformInstance();
        }

        /// <summary>
        /// Defines "what to draw".
        /// </summary>
        public readonly Content.DrawableContent Content;

        /// <summary>
        /// Defines "where to draw it".
        /// </summary>
        public readonly IMeshTransform Transform;
    }
}
