using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VERTEXINFLUENCES = Microsoft.Xna.Framework.Graphics.VertexInfluences;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    readonly struct _MeshDecoder<TMaterial> : IMeshDecoder<TMaterial>
        where TMaterial : class
    {
        #region constructor
        public _MeshDecoder(Assimp.Mesh mesh, TMaterial material)
        {
            _Name = mesh.Name;
            _Source = new _MeshPrimitiveDecoder<TMaterial>(mesh, material);
        }

        #endregion

        #region data

        private readonly string _Name;
        private readonly IMeshPrimitiveDecoder<TMaterial> _Source;

        #endregion

        #region properties

        public string Name => _Name;

        public IReadOnlyList<IMeshPrimitiveDecoder<TMaterial>> Primitives => new [] { _Source };

        public object Tag => null; // might return Extras.

        #endregion
    }

    readonly struct _MeshPrimitiveDecoder<TMaterial> : IMeshPrimitiveDecoder<TMaterial>
    where TMaterial : class
    {
        #region constructor
        public static IMeshPrimitiveDecoder<TMaterial> Create(Assimp.Mesh mesh, TMaterial material)
        {
            return new _MeshPrimitiveDecoder<TMaterial>(mesh, material);
        }
        public _MeshPrimitiveDecoder(Assimp.Mesh mesh, TMaterial material)
        {
            _Source = mesh;
            _Material = material;

            _ColorCount = 0;
            if (mesh.HasVertexColors(0)) _ColorCount = 1;
            if (mesh.HasVertexColors(1)) _ColorCount = 2;

            _TexCoordCount = 0;
            if (mesh.HasTextureCoords(0)) _TexCoordCount = 1;
            if (mesh.HasTextureCoords(1)) _TexCoordCount = 2;

            if (mesh.HasBones)
            {
                var influences = new List<(int bone, float weight)>[_Source.VertexCount];

                for (int i=0; i < mesh.BoneCount; ++i)
                {
                    var bone = mesh.Bones[i];

                    foreach(var vertex in bone.VertexWeights)
                    {
                        var influence = influences[vertex.VertexID];
                        if (influence == null) influence = influences[vertex.VertexID] = new List<(int bone, float weight)>();
                        influence.Add((i, vertex.Weight));
                    }
                }

                _Skinning = influences
                    .Select(item => VERTEXINFLUENCES.FromCollection(item))
                    .ToArray();
                
            }
            else
            {
                _Skinning = null;
            }
        }

        #endregion

        #region data

        private readonly Assimp.Mesh _Source;
        private readonly TMaterial _Material;
        private readonly int _ColorCount;
        private readonly int _TexCoordCount;
        private readonly VERTEXINFLUENCES[] _Skinning;

        #endregion

        #region properties

        public TMaterial Material => _Material;
        public int VertexCount => _Source.VertexCount;
        public int MorphTargetsCount => 0;
        public int ColorsCount => _ColorCount;
        public int TexCoordsCount => _TexCoordCount;
        public int JointsWeightsCount => _Source.HasBones ? 4 : 0;
        public IEnumerable<(int A, int B)> LineIndices => Enumerable.Empty<(int A, int B)>();
        public IEnumerable<(int A, int B, int C)> TriangleIndices => _EvaluateTriangles();

        #endregion

        #region vertex API

        private IEnumerable<(int A, int B, int C)> _EvaluateTriangles()
        {
            foreach(var face in _Source.Faces)
            {
                // evaluate the face polygon as a triangle fan.
                for(int i=2; i < face.IndexCount; ++i)
                {
                    yield return (face.Indices[0], face.Indices[i - 1], face.Indices[i]);
                }
            }
        }
        public Vector3 GetPosition(int vertexIndex)
        {
            return _Source.Vertices[vertexIndex].ToXna();
        }
        public Vector3 GetNormal(int vertexIndex)
        {
            if (!_Source.HasNormals) return Vector3.Zero;
            return _Source.Normals[vertexIndex].ToXna();
        }
        public Vector4 GetTangent(int vertexIndex)
        {
            if (!_Source.HasTangentBasis) return Vector4.Zero;

            var n = _Source.Normals[vertexIndex].ToXna();
            var u = _Source.Tangents[vertexIndex].ToXna();
            var v = _Source.BiTangents[vertexIndex].ToXna();

            var x = Vector3.Cross(u, v);
            var s = Math.Sign(Vector3.Dot(n, x));           

            return new Vector4(u, s);
        }       
        public Vector4 GetColor(int vertexIndex, int colorSetIndex)
        {
            if (_Source.VertexColorChannelCount <= colorSetIndex) return Vector4.One;
            return _Source.VertexColorChannels[colorSetIndex][vertexIndex].ToXnaVector();
        }
        public Vector2 GetTextureCoord(int vertexIndex, int textureSetIndex)
        {
            if (_Source.TextureCoordinateChannelCount <= textureSetIndex) return Vector2.Zero;
            var uvw = _Source.TextureCoordinateChannels[textureSetIndex][vertexIndex].ToXna();
            return new Vector2(uvw.X, uvw.Y);
        }
        public VERTEXINFLUENCES GetSkinWeights(int vertexIndex)
        {
            return _Skinning != null ? _Skinning[vertexIndex] : VERTEXINFLUENCES.Default;
        }
        public IReadOnlyList<Vector3> GetNormalDeltas(int vertexIndex) { throw new NotImplementedException(); }
        public IReadOnlyList<Vector3> GetPositionDeltas(int vertexIndex) { throw new NotImplementedException(); }
        public IReadOnlyList<Vector3> GetTangentDeltas(int vertexIndex) { throw new NotImplementedException(); }

        #endregion
    }
}
