using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XYZW = Microsoft.Xna.Framework.Vector4;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Represents 4 vertex influences, defined as 4 bone indices and 4 weights.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{Indices} {Weights}")]
    public struct VertexInfluences
    {
        public static VertexInfluences FromCollection(IEnumerable<(int Index, float Weight)> jointWeights)
        {
            if (jointWeights == null || !jointWeights.Any()) return default;

            int index = 0;

            var indices = Vector4.Zero;
            var weights = Vector4.Zero;
            var wsum = 0f;

            foreach (var (i, w) in jointWeights.OrderByDescending(item => item.Weight).Take(4))
            {
                switch (index)
                {
                    case 0: { indices.X = i; weights.X = w; wsum += w; break; }
                    case 1: { indices.Y = i; weights.Y = w; wsum += w; break; }
                    case 2: { indices.Z = i; weights.Z = w; wsum += w; break; }
                    case 3: { indices.W = i; weights.W = w; wsum += w; break; }
                }

                ++index;
            }

            return new VertexInfluences(indices, weights / wsum);
        }

        public static readonly VertexInfluences Default = new VertexInfluences(XYZW.Zero, XYZW.UnitX);

        public VertexInfluences(XYZW indices, XYZW weights)
        {
            Indices = new PackedVector.Short4(indices);
            Weights = weights;
        }

        public PackedVector.Short4 Indices;
        public XYZW Weights;

        public float WeightSum => Weights.X + Weights.Y + Weights.Z + Weights.W;

        public IEnumerable<(int,float)> GetIndexedWeights()
        {
            var indices = Indices.ToVector4();
            yield return ((int)indices.X, Weights.X);
            yield return ((int)indices.Y, Weights.Y);
            yield return ((int)indices.Z, Weights.Z);
            yield return ((int)indices.W, Weights.W);
        }
    }
}
