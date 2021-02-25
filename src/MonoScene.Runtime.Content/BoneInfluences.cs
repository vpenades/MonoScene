using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XNAV4 = Microsoft.Xna.Framework.Vector4;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    /// <summary>
    /// Represents 4 bone influences, defined as 4 indices and 4 weights.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{Indices} {Weights}")]
    public struct BoneInfluences
    {
        public static BoneInfluences FromCollection(IEnumerable<(int Index, float Weight)> jointWeights)
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

            return new BoneInfluences(indices, weights / wsum);
        }

        public static readonly BoneInfluences Default = new BoneInfluences(XNAV4.Zero, XNAV4.UnitX);

        public BoneInfluences(XNAV4 indices, XNAV4 weights)
        {
            Indices = new Short4(indices);
            Weights = weights;
        }

        public BoneInfluences(Short4 indices, XNAV4 weights)
        {
            Indices = indices;
            Weights = weights;
        }

        public Short4 Indices;
        public XNAV4 Weights;

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
