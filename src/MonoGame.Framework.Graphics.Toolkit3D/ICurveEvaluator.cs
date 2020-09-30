using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Interface over a curve generalization.
    /// </summary>
    /// <typeparam name="T">Any type that can be interpolated, like:
    /// - <see cref="Single"/>
    /// - <see cref="Vector3"/>
    /// - <see cref="Quaternion"/>
    /// - etc
    /// </typeparam>
    /// <remarks>
    /// 1- This interface could be incorported into Monogame, and implemented in monogame's <see cref="Curve"/> class.
    /// 
    /// 2- Notice that a Vector3 curve can be simulated by wrapping 3 monogame's <see cref="Curve"/> instances.
    /// but, for Quaternion curves, we need a full implementation.
    /// 
    /// 3- In general, curves can be implemented in so many ways that demand an abstract interface.
    /// After all, animatable objects only need to evaluate the curve at a given time and don't care
    /// about the internal logic of the curve.
    /// 
    /// For example: an extreme case of curve implementation is the case of Collada, which has curves controlled
    /// by actual math formulas. As far as I know, it reached the collada schema, but I don't think nobody ever used it...
    /// </remarks>
    public interface ICurveEvaluator<T>
    {        
        T Evaluate(Single position);
    }
}
