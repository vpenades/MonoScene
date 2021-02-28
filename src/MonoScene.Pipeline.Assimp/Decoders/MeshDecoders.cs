using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VERTEXINFLUENCES = Microsoft.Xna.Framework.Graphics.PackedVector.BoneInfluences;

using XNAV2 = Microsoft.Xna.Framework.Vector2;
using XNAV3 = Microsoft.Xna.Framework.Vector3;
using XNAV4 = Microsoft.Xna.Framework.Vector4;

namespace MonoScene.Graphics.Pipeline
{
    readonly struct _MeshDecoder<TMaterial> : IMeshDecoder<TMaterial>        
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
        public object Tag => null;
        public IReadOnlyList<IMeshPrimitiveDecoder<TMaterial>> Primitives => new [] { _Source };        

        #endregion
    }

    readonly struct _MeshPrimitiveDecoder<TMaterial> : IMeshPrimitiveDecoder<TMaterial>    
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
        public XNAV3 GetPosition(int vertexIndex)
        {
            return _Source.Vertices[vertexIndex].ToXna();
        }
        public XNAV3 GetNormal(int vertexIndex)
        {
            if (!_Source.HasNormals) return XNAV3.Zero;
            return _Source.Normals[vertexIndex].ToXna();
        }
        public XNAV4 GetTangent(int vertexIndex)
        {
            if (!_Source.HasTangentBasis) return XNAV4.Zero;

            var n = _Source.Normals[vertexIndex].ToXna();
            var u = _Source.Tangents[vertexIndex].ToXna();
            var v = _Source.BiTangents[vertexIndex].ToXna();

            var x = XNAV3.Cross(u, v);
            var s = Math.Sign(XNAV3.Dot(n, x));           

            return new XNAV4(u, s);
        }       
        public XNAV4 GetColor(int vertexIndex, int colorSetIndex)
        {
            if (_Source.VertexColorChannelCount <= colorSetIndex) return XNAV4.One;
            return _Source.VertexColorChannels[colorSetIndex][vertexIndex].ToXnaVector();
        }
        public XNAV2 GetTextureCoord(int vertexIndex, int textureSetIndex)
        {
            if (_Source.TextureCoordinateChannelCount <= textureSetIndex) return XNAV2.Zero;
            var uvw = _Source.TextureCoordinateChannels[textureSetIndex][vertexIndex].ToXna();
            return new XNAV2(uvw.X, uvw.Y);
        }
        public VERTEXINFLUENCES GetSkinWeights(int vertexIndex)
        {
            return _Skinning != null ? _Skinning[vertexIndex] : VERTEXINFLUENCES.Default;
        }
        public IReadOnlyList<XNAV3> GetNormalDeltas(int vertexIndex) { throw new NotImplementedException(); }
        public IReadOnlyList<XNAV3> GetPositionDeltas(int vertexIndex) { throw new NotImplementedException(); }
        public IReadOnlyList<XNAV3> GetTangentDeltas(int vertexIndex) { throw new NotImplementedException(); }

        #endregion
    }
}
