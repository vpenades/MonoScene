using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    [System.Diagnostics.DebuggerDisplay("{Count} Meshes {SharedEffects.Count} Shared effects.")]
    public class MeshCollection : IDisposable
    {
        #region lifecycle

        internal MeshCollection(RuntimeModelMesh[] meshes, GraphicsResource[] disposables)
        {
            _Disposables = disposables;
            _Meshes = meshes;

            _SharedEffects = _Meshes
                .SelectMany(item => item.OpaqueEffects.Concat(item.TranslucidEffects))
                .Distinct()
                .ToArray();
        }

        public void Dispose()
        {
            if (_Disposables != null)
            {
                foreach (var d in _Disposables) d.Dispose();
            }

            _Disposables = null;            
        }

        #endregion

        #region data

        private GraphicsResource[] _Disposables;

        private readonly RuntimeModelMesh[] _Meshes;

        private readonly Effect[] _SharedEffects;

        #endregion

        #region properties

        public int Count => _Meshes.Length;

        public RuntimeModelMesh this[int index] => _Meshes[index];

        public IReadOnlyCollection<Effect> SharedEffects => _SharedEffects;

        #endregion
    }
}
