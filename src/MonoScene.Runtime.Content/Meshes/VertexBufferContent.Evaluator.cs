using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

using XNAV3 = Microsoft.Xna.Framework.Vector3;
using XNAV4 = Microsoft.Xna.Framework.Vector4;

using XNABYTE4 = Microsoft.Xna.Framework.Graphics.PackedVector.Byte4;
using XNAINDICES = Microsoft.Xna.Framework.Graphics.PackedVector.Short4;
using VERTEXINFLUENCES = Microsoft.Xna.Framework.Graphics.PackedVector.BoneInfluences;

namespace MonoScene.Graphics.Content
{
    partial class VertexBufferContent
    {
        public Evaluator GetEvaluator(int vertexOffset, int vertexCount)
        {
            return new Evaluator(_VertexData, vertexOffset * _VertexStride, _VertexStride, vertexCount, _VertexElements);
        }

        public readonly struct Evaluator
        {
            #region constructor

            public Evaluator(Byte[] vertexData, int byteOffset, int byteStride, int vertexCount, VertexElement[] vertexFormat)
            {
                if (vertexData == null) throw new ArgumentNullException(nameof(vertexData));
                if (byteStride < 12) throw new ArgumentOutOfRangeException(nameof(byteStride));
                if (vertexData.Length - byteOffset < byteStride * vertexCount) throw new ArgumentOutOfRangeException(nameof(vertexCount));
                if (vertexFormat == null || vertexFormat.Length == 0) throw new ArgumentNullException(nameof(vertexFormat));

                // initialize data

                _VertexData = vertexData;
                _ByteOffset = byteOffset;
                _ByteStride = byteStride;
                _VertexCount = vertexCount;

                // initialize elements

                _PositionElement = vertexFormat.FirstOrDefault(item => item.VertexElementUsage == VertexElementUsage.Position);
                _BlendIndicesElement = vertexFormat.FirstOrDefault(item => item.VertexElementUsage == VertexElementUsage.BlendIndices);
                _BlendWeightsElement = vertexFormat.FirstOrDefault(item => item.VertexElementUsage == VertexElementUsage.BlendWeight);

                _HasSkinElements = true;
                _HasSkinElements &= _BlendIndicesElement.VertexElementUsage == VertexElementUsage.BlendIndices;
                _HasSkinElements &= _BlendWeightsElement.VertexElementUsage == VertexElementUsage.BlendWeight;

                // initialize decoders                

                var data = _VertexData;

                unsafe T Cast<T>(int offset) where T : unmanaged
                {
                    var span = data.AsSpan(offset);
                    #if DEBUG
                    span = span.Slice(0, sizeof(T));
                    #endif
                    return System.Runtime.InteropServices.MemoryMarshal.Read<T>(span);
                }

                switch (_PositionElement.VertexElementFormat)
                {
                    case VertexElementFormat.Vector3: _PositionDecoder = idx => Cast<XNAV3>(idx); break;
                    case VertexElementFormat.Vector4: _PositionDecoder = idx => Cast<XNAV3>(idx); break;                        
                    default: _PositionDecoder = null; throw new NotSupportedException($"{_PositionElement.VertexElementFormat}");
                }

                _BlendIndicesDecoder = null;
                _BlendWeightsDecoder = null;

                if (_HasSkinElements)
                {
                    switch (_BlendIndicesElement.VertexElementFormat)
                    {
                        case VertexElementFormat.Vector4:
                            _BlendIndicesDecoder = idx => new XNAINDICES(Cast<XNAV4>(idx)); break;
                        case VertexElementFormat.Color:
                        case VertexElementFormat.Byte4:
                            _BlendIndicesDecoder = idx => new XNAINDICES(Cast<XNABYTE4>(idx).ToVector4()); break;
                        case VertexElementFormat.Short4:
                            _BlendIndicesDecoder = idx => Cast<XNAINDICES>(idx); break;
                        default:
                            _HasSkinElements = false; break;
                    }

                    switch (_BlendWeightsElement.VertexElementFormat)
                    {
                        case VertexElementFormat.Vector4:
                            _BlendWeightsDecoder = idx => Cast<XNAV4>(idx); break;
                        case VertexElementFormat.Color:
                        case VertexElementFormat.Byte4:
                            _BlendWeightsDecoder = idx => Cast<Microsoft.Xna.Framework.Graphics.PackedVector.NormalizedByte4>(idx).ToVector4(); break;
                        case VertexElementFormat.NormalizedShort4:
                            _BlendWeightsDecoder = idx => Cast<Microsoft.Xna.Framework.Graphics.PackedVector.NormalizedShort4>(idx).ToVector4(); break;
                        default:
                            _HasSkinElements = false; break;
                    }
                }

                // if blend elements don't exist, or have unsupported format, fall back to default:

                if (!_HasSkinElements)
                {
                    _BlendIndicesDecoder = idx => new XNAINDICES(0, 0, 0, 0);
                    _BlendWeightsDecoder = idx => new XNAV4(1, 0, 0, 0);
                }
            }

            #endregion

            #region data

            private readonly Byte[] _VertexData;
            private readonly int _ByteOffset;
            private readonly int _ByteStride;
            private readonly int _VertexCount;

            private readonly VertexElement _PositionElement;
            private readonly VertexElement _BlendIndicesElement;
            private readonly VertexElement _BlendWeightsElement;

            private readonly bool _HasSkinElements;

            private readonly Converter<int, XNAV3> _PositionDecoder;            
            private readonly Converter<int, XNAINDICES> _BlendIndicesDecoder;           
            private readonly Converter<int, XNAV4> _BlendWeightsDecoder;

            #endregion

            #region API

            private int _GetVertexOffset(int index)
            {
                if (index < 0 || index >= _VertexCount) throw new ArgumentOutOfRangeException(nameof(index));
                index *= _ByteStride;
                index += _ByteOffset;
                return index;
            }

            public XNAV3 GetPosition(int index) { return _PositionDecoder(_GetVertexOffset(index) + _PositionElement.Offset); }
            public XNAINDICES GetBlendIndices(int index) { return _BlendIndicesDecoder(_GetVertexOffset(index) + _BlendIndicesElement.Offset); }
            public XNAV4 GetBlendWeights(int index) { return _BlendWeightsDecoder(_GetVertexOffset(index) + _BlendWeightsElement.Offset); }

            public VERTEXINFLUENCES GetBlend(int index)
            {
                if (!_HasSkinElements) return VERTEXINFLUENCES.Default;

                var indices = GetBlendIndices(index);
                var weights = GetBlendWeights(index);
                return new VERTEXINFLUENCES(indices, weights);
            }

            #endregion
        }
    }
}
