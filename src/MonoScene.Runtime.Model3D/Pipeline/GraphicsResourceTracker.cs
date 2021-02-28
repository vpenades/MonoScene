using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

namespace MonoScene.Graphics.Pipeline
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

        private static bool IsStaticResource(GraphicsResource resource)
        {
            if (Object.ReferenceEquals(resource, BlendState.Opaque)) return true;
            if (Object.ReferenceEquals(resource, BlendState.Additive)) return true;
            if (Object.ReferenceEquals(resource, BlendState.AlphaBlend)) return true;
            if (Object.ReferenceEquals(resource, BlendState.NonPremultiplied)) return true;

            if (Object.ReferenceEquals(resource, SamplerState.PointWrap)) return true;
            if (Object.ReferenceEquals(resource, SamplerState.PointClamp)) return true;
            if (Object.ReferenceEquals(resource, SamplerState.LinearWrap)) return true;
            if (Object.ReferenceEquals(resource, SamplerState.LinearClamp)) return true;
            if (Object.ReferenceEquals(resource, SamplerState.AnisotropicWrap)) return true;
            if (Object.ReferenceEquals(resource, SamplerState.AnisotropicClamp)) return true;

            return false;
        }

        public void AddDisposable(GraphicsResource resource)
        {
            if (resource == null) throw new ArgumentNullException();

            if (_Disposables.Contains(resource)) return;

            if (IsStaticResource(resource)) return;      

            _Disposables.Add(resource);
        }

        public void AddDisposables(IEnumerable<GraphicsResource> resources)
        {
            foreach (var r in resources) AddDisposable(r);
        }

        #endregion
    }
}
