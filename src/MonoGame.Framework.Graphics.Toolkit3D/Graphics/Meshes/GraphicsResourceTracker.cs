using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    // tracks all the disposable objects of a model;
    // vertex buffers, index buffers, effects and textures.
    class GraphicsResourceTracker
    {
        #region data

        private readonly HashSet<GraphicsResource> _Disposables = new HashSet<GraphicsResource>();

        #endregion

        #region properties

        public IEnumerable<GraphicsResource> Disposables => _Disposables;

        #endregion

        #region API        
        public void AddDisposable(GraphicsResource resource)
        {
            if (resource == null) throw new ArgumentNullException();

            if (Object.ReferenceEquals(resource, BlendState.Opaque)) throw new ArgumentException("Static");
            if (Object.ReferenceEquals(resource, BlendState.AlphaBlend)) throw new ArgumentException("Static");

            if (Object.ReferenceEquals(resource, SamplerState.LinearWrap)) throw new ArgumentException("Static");


            if (_Disposables.Contains(resource)) throw new ArgumentException("Already Added");
            _Disposables.Add(resource);
        }

        #endregion
    }
}
