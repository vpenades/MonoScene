using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoScene.Graphics.Content
{
    /// <summary>
    /// Defines an animatable property with a default value and a collection of animation curve tracks.
    /// </summary>
    /// <typeparam name="T">A type that can be interpolated with <see cref="ICurveEvaluator{T}"/></typeparam>
    [System.Diagnostics.DebuggerDisplay("{Value} with {CurveCount} curves.")]
    public sealed class AnimatableProperty<T>
       where T : struct
    {
        #region lifecycle

        public AnimatableProperty(T defaultValue)
        {
            Value = defaultValue;
        }

        #endregion

        #region data

        private List<Microsoft.Xna.Framework.ICurveEvaluator<T>> _Curves;

        /// <summary>
        /// Gets the default value of this instance.
        /// When animations are disabled, or there's no animation track available, this will be the returned value.
        /// </summary>
        public T Value { get; set; }

        #endregion

        #region properties

        public bool IsAnimated => _Curves == null ? false : _Curves.Count > 0;

        public int CurveCount => _Curves.Count;

        #endregion

        #region API

        /// <summary>
        /// Evaluates the value of this <see cref="AnimatableProperty{T}"/> at a given <paramref name="offset"/> for a given <paramref name="curveIndex"/>.
        /// </summary>
        /// <param name="curveIndex">The index of the animation track</param>
        /// <param name="offset">The time offset within the curve</param>
        /// <returns>The evaluated value taken from the animation <paramref name="curveIndex"/>, or <see cref="Value"/> if a track was not found.</returns>
        public T GetValueAt(int curveIndex, float offset)
        {
            if (_Curves == null) return this.Value;

            if (curveIndex < 0 || curveIndex >= _Curves.Count) return this.Value;

            return _Curves[curveIndex]?.Evaluate(offset) ?? this.Value;
        }

        public void SetCurve(int curveIndex, Microsoft.Xna.Framework.ICurveEvaluator<T> sampler)
        {
            if (curveIndex < 0) throw new ArgumentOutOfRangeException(nameof(curveIndex));

            if (_Curves == null) _Curves = new List<Microsoft.Xna.Framework.ICurveEvaluator<T>>();

            while (_Curves.Count <= curveIndex) _Curves.Add(null);

            _Curves[curveIndex] = sampler ?? throw new ArgumentNullException(nameof(sampler));
        }

        #endregion
    }
}
