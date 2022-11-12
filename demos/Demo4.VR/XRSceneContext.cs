using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework
{
    public readonly struct XRSceneContext
    {
        internal XRSceneContext(in Matrix vm, Func<float,float,Matrix> projFunc)
        {
            ViewMatrix = vm;
            _ProjectionFunction= projFunc;
        }

        private readonly Func<float, float, Matrix> _ProjectionFunction;

        public Matrix ViewMatrix { get; }

        public Matrix GetProjectionMatrix(float near, float far)
        {
            return _ProjectionFunction(near, far);
        }
    }
}
