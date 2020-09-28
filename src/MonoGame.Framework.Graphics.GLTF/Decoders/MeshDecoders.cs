using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    readonly struct _MeshDecoder<TMaterial> : IMeshDecoder<TMaterial>
        where TMaterial : class
    {
        #region constructor
        public _MeshDecoder(SharpGLTF.Runtime.IMeshDecoder<TMaterial> mesh)
        {
            _Source = mesh;
            _Primitives = mesh.Primitives.Select(item => _MeshPrimitiveDecoder<TMaterial>.Create(item)).ToArray();
        }

        #endregion

        #region data

        private readonly SharpGLTF.Runtime.IMeshDecoder<TMaterial> _Source;
        private readonly IMeshPrimitiveDecoder<TMaterial>[] _Primitives;

        #endregion

        #region properties

        public string Name => _Source.Name;       

        public IReadOnlyList<IMeshPrimitiveDecoder<TMaterial>> Primitives => _Primitives;

        public object Tag => null; // might return Extras.

        #endregion
    }

    readonly struct _MeshPrimitiveDecoder<TMaterial> : IMeshPrimitiveDecoder<TMaterial>
    where TMaterial:class
    {
        #region constructor

        public static IMeshPrimitiveDecoder<TMaterial> Create(SharpGLTF.Runtime.IMeshPrimitiveDecoder<TMaterial> primitive)
        {
            return new _MeshPrimitiveDecoder<TMaterial>(primitive);
        }
        public _MeshPrimitiveDecoder(SharpGLTF.Runtime.IMeshPrimitiveDecoder<TMaterial> primitive)
        {
            _Source = primitive;
        }

        #endregion

        #region data

        private readonly SharpGLTF.Runtime.IMeshPrimitiveDecoder<TMaterial> _Source;

        #endregion

        #region properties

        public TMaterial Material => _Source.Material;

        public int VertexCount => _Source.VertexCount;

        public int MorphTargetsCount => 0;

        public int ColorsCount => _Source.ColorsCount;

        public int TexCoordsCount => _Source.TexCoordsCount;

        public int JointsWeightsCount => _Source.JointsWeightsCount == 0 ? 0 : 4;

        public IEnumerable<(int A, int B)> LineIndices => Enumerable.Empty<(int A, int B)>();

        public IEnumerable<(int A, int B, int C)> TriangleIndices => _Source.TriangleIndices;

        #endregion

        #region vertex API

        public Vector3 GetPosition(int vertexIndex) { return _Source.GetPosition(vertexIndex).ToXna(); }
        public Vector3 GetNormal(int vertexIndex) { return _Source.GetNormal(vertexIndex).ToXna(); }
        public Vector4 GetTangent(int vertexIndex) { return _Source.GetTangent(vertexIndex).ToXna(); }
        public VertexSkinning GetSkinWeights(int vertexIndex)
        {
            var sparse = _Source
                .GetSkinWeights(vertexIndex)
                .GetReducedWeights(4);

            var indices = new Vector4(sparse.Index0, sparse.Index1, sparse.Index2, sparse.Index3);
            var weights = new Vector4(sparse.Weight0, sparse.Weight1, sparse.Weight2, sparse.Weight3);

            return new VertexSkinning
            {
                Indices = new Framework.Graphics.PackedVector.Short4(indices),
                Weights = weights
            };
        }

        

        public Vector4 GetColor(int vertexIndex, int colorSetIndex) { return _Source.GetColor(vertexIndex, colorSetIndex).ToXna(); }
        public Vector2 GetTextureCoord(int vertexIndex, int textureSetIndex) { return _Source.GetTextureCoord(vertexIndex, textureSetIndex).ToXna(); }


        public IReadOnlyList<Vector3> GetNormalDeltas(int vertexIndex) { throw new NotImplementedException(); }
        public IReadOnlyList<Vector3> GetPositionDeltas(int vertexIndex) { throw new NotImplementedException(); }
        public IReadOnlyList<Vector3> GetTangentDeltas(int vertexIndex) { throw new NotImplementedException(); }

        #endregion
    }
}
