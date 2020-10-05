using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Replaces <see cref="ModelMesh"/>
    /// </summary>
    public sealed class Mesh
    {
        #region lifecycle

        public Mesh(GraphicsDevice graphicsDevice)
        {
            this._GraphicsDevice = graphicsDevice;
        }        

        #endregion

        #region data

        internal GraphicsDevice _GraphicsDevice;

        private readonly List<MeshPart> _Primitives = new List<MeshPart>();
        private IReadOnlyList<Effect> _Effects;

        private IReadOnlyList<MeshPart> _OpaquePrimitives;
        private IReadOnlyList<Effect> _OpaqueEffects;

        private IReadOnlyList<MeshPart> _TranslucidPrimitives;
        private IReadOnlyList<Effect> _TranslucidEffects;        

        #endregion

        #region  properties
        public string Name { get; set; }
        public object Tag { get; set; }

        public IReadOnlyCollection<Effect> OpaqueEffects
        {
            get
            {
                if (_OpaqueEffects != null) return _OpaqueEffects;

                // Create the shared effects collection on demand.

                _OpaqueEffects = GetOpaqueParts()
                    .Select(item => item.Effect)
                    .Distinct()
                    .ToArray();

                return _OpaqueEffects;
            }
        }

        public IReadOnlyCollection<Effect> TranslucidEffects
        {
            get
            {
                if (_TranslucidEffects != null) return _TranslucidEffects;

                // Create the shared effects collection on demand.

                _TranslucidEffects = GetTranslucidParts()
                    .Select(item => item.Effect)
                    .Distinct()
                    .ToArray();

                return _TranslucidEffects;
            }
        }        

        #endregion

        #region API

        public MeshPart CreateMeshPart()
        {
            var primitive = new MeshPart(this);

            _Primitives.Add(primitive);

            _OpaquePrimitives = null;
            _TranslucidPrimitives = null;

            InvalidateEffectCollection();            

            return primitive;
        }

        internal void InvalidateEffectCollection()
        {
            _OpaqueEffects = null;
            _TranslucidEffects = null;
        }

        private IReadOnlyList<MeshPart> GetOpaqueParts()
        {
            if (_OpaquePrimitives != null) return _OpaquePrimitives;
            _OpaquePrimitives = _Primitives.Where(item => item.Blending == BlendState.Opaque).ToArray();
            return _OpaquePrimitives;
        }

        private IReadOnlyList<MeshPart> GetTranslucidParts()
        {
            if (_TranslucidPrimitives != null) return _TranslucidPrimitives;
            _TranslucidPrimitives = _Primitives.Where(item => item.Blending != BlendState.Opaque).ToArray();
            return _TranslucidPrimitives;
        }

        public void DrawOpaque()
        {
            _GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            foreach (var part in GetOpaqueParts()) part.Draw(_GraphicsDevice);
        }

        public void DrawTranslucid()
        {
            _GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            foreach (var part in GetTranslucidParts()) part.Draw(_GraphicsDevice);
        }

        #endregion
    }
}
