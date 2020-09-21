using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework.Graphics;

namespace SharpGLTF.Runtime
{
    /// <summary>
    /// Replaces <see cref="ModelMeshPart"/>.
    /// </summary>    
    sealed class RuntimeModelMeshPart
    {
        #region lifecycle

        internal RuntimeModelMeshPart(RuntimeModelMesh parent)
        {
            _Parent = parent;
        }

        #endregion

        #region data

        private readonly RuntimeModelMesh _Parent;

        private Effect _Effect;
        private BlendState _Blend = BlendState.Opaque;        

        // state used for normal rendering
        private RasterizerState _FrontRasterizer = RasterizerState.CullCounterClockwise;

        // state used for mirrored tranform. This must be the same as _FrontRasterizer with reversed CullMode.
        private RasterizerState _BackRasterizer = RasterizerState.CullClockwise;

        private IndexBuffer _SharedIndexBuffer;
        private int _IndexOffset;
        private int _PrimitiveCount;        

        private VertexBuffer _SharedVertexBuffer;
        private int _VertexOffset;
        private int _VertexCount;

        private Microsoft.Xna.Framework.BoundingSphere _Bounds;        

        #endregion

        #region properties

        public GraphicsDevice Device => _Parent._GraphicsDevice;

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

        public RasterizerState FrontRasterizer
        {
            get => _FrontRasterizer;
            set => _FrontRasterizer = value;
        }

        public RasterizerState BackRasterizer
        {
            get => _BackRasterizer;
            set => _BackRasterizer = value;
        }

        #endregion

        #region API

        internal void SetVertexBuffer(VertexBuffer vb, int offset, int count)
        {
            this._SharedVertexBuffer = vb;
            this._VertexOffset = offset;
            this._VertexCount = count;            
        }

        internal void SetIndexBuffer(IndexBuffer ib, int offset, int count)
        {
            this._SharedIndexBuffer = ib;
            this._IndexOffset = offset;
            this._PrimitiveCount = count;            
        }

        public void Draw(GraphicsDevice device)
        {
            bool isMirrorTransform = _Effect is AnimatedEffect animEffect && animEffect.WorldIsMirror;

            if (_PrimitiveCount > 0)
            {
                device.SetVertexBuffer(_SharedVertexBuffer);
                device.Indices = _SharedIndexBuffer;

                device.BlendState = _Blend;
                device.RasterizerState = isMirrorTransform ? _BackRasterizer : _FrontRasterizer;

                foreach(var pass in _Effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, _VertexOffset, _IndexOffset, _PrimitiveCount);
                }
            }
        }

        #endregion
    }

    
}
