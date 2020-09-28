using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    readonly struct _GltfSamplerVector3 : ICurveEvaluator<Vector3>
    {
        public _GltfSamplerVector3(SharpGLTF.Animations.ICurveSampler<System.Numerics.Vector3> source) { _Source = source; }

        private readonly SharpGLTF.Animations.ICurveSampler<System.Numerics.Vector3> _Source;
        public Vector3 Evaluate(float offset) { return _Source.GetPoint(offset).ToXna(); }
    }

    readonly struct _GltfSamplerQuaternion : ICurveEvaluator<Quaternion>
    {
        public _GltfSamplerQuaternion(SharpGLTF.Animations.ICurveSampler<System.Numerics.Quaternion> source) { _Source = source; }

        private readonly SharpGLTF.Animations.ICurveSampler<System.Numerics.Quaternion> _Source;
        public Quaternion Evaluate(float offset) { return _Source.GetPoint(offset).ToXna(); }
    }
}
