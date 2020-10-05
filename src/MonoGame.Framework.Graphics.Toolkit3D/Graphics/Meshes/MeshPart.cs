using System;
using System.Collections.Generic;
using System.Linq;



namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Replaces <see cref="ModelMeshPart"/>.
    /// </summary>    
    public sealed class MeshPart
    {
        #region lifecycle

        internal MeshPart(Mesh parent)
        {
            _Parent = parent;
        }        

        #endregion

        #region data

        private readonly Mesh _Parent;

        private Effect _Effect;
        private BlendState _Blend = BlendState.Opaque;

        private IMeshGeometry _Geometry;

        #endregion

        #region properties        

        public Effect Effect
        {
            get => _Effect;
            set
            {
                if (_Effect == value) return;
                _Effect = value;
                _Parent.InvalidateEffectCollection(); // if we change this property, we need to invalidate the parent's effect collection.
            }
        }

        public BlendState Blending
        {
            get => _Blend;
            set => _Blend = value;
        }
        
        public IMeshGeometry Geometry
        {
            get => _Geometry;
            set => _Geometry = value;
        }

        #endregion

        #region API

        public void Draw(GraphicsDevice device)
        {
            // check if world matrix is a mirror matrix and requires the face culling to be reversed.
            bool isMirrorTransform = false;
            if (_Effect is IEffectMatrices ematrices) isMirrorTransform = ematrices.World.Determinant() < 0;

            _Geometry.Bind(device, isMirrorTransform);            

            device.BlendState = _Blend;            

            foreach(var pass in _Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                _Geometry.Draw(device);
            }            
        }

        #endregion
    }    
}
