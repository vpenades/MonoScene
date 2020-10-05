using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    public interface IMeshCollection : IReadOnlyList<Mesh>
    {
        Effect[] GetSharedEffects(IEnumerable<int> meshIndices);
    }

    [System.Diagnostics.DebuggerDisplay("{Count} Meshes {SharedEffects.Count} Shared effects.")]
    public class MeshCollection : IDisposable, IMeshCollection
    {
        #region lifecycle

        internal MeshCollection(Mesh[] meshes, GraphicsResource[] disposables)
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

        private readonly Mesh[] _Meshes;

        private readonly Effect[] _SharedEffects;

        #endregion

        #region properties

        public int Count => _Meshes.Length;

        public Mesh this[int index] => _Meshes[index];
        
        #endregion

        #region API

        public IEnumerator<Mesh> GetEnumerator() { return (IEnumerator<Mesh>)_Meshes.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return _Meshes.GetEnumerator(); }

        public Effect[] GetSharedEffects(IEnumerable<int> meshIndices)
        {
            // gather all effects used by the meshes indexed by meshIndices.

            return meshIndices
                .Select(item => _Meshes[item])
                .SelectMany(item => item.OpaqueEffects.Concat(item.TranslucidEffects))
                .Distinct()
                .ToArray();
        }

        #endregion
    }
}
