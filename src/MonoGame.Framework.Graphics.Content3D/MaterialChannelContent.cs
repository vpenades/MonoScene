using System;
using System.Collections.Generic;
using System.Text;
          
namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    [System.Diagnostics.DebuggerDisplay("{Target}")]
    public class MaterialChannelContent
    {
        #region lifecycle
        internal MaterialChannelContent(string name)
        {
            _Target = name;
            _Value = _GetDefaultValue(name);
        }

        private static float[] _GetDefaultValue(string key)
        {
            switch (key)
            {
                case "Emissive": return new float[] { 0, 0, 0 };

                case "Normal":                
                case "Occlusion":
                    return new float[] { 1 };

                case "BaseColor":
                case "Diffuse":
                    return new float[] { 1, 1, 1, 1 };

                case "MetallicRoughness":
                    return new float[] { 1, 1 };

                case "SpecularGlossiness":
                    return new float[] { 1, 1, 1, 1 };
                
                default: throw new NotImplementedException();
            }
        }

        #endregion

        #region data

        private readonly string _Target;

        private int _VertexIndexSet;

        private Vector3 _TransformU = Vector3.UnitX;
        private Vector3 _TransformV = Vector3.UnitY;

        private float[] _Value;

        private SamplerStateContent _Sampler;

        private Object _Texture;

        #endregion

        #region properties

        public string Target => _Target;

        public int VertexIndexSet
        {
            get => _VertexIndexSet;
            set => _VertexIndexSet = value;
        }

        public (Vector3 U, Vector3 V) Transform
        {
            get => (_TransformU, _TransformV);
            set { _TransformU = value.U; _TransformV = value.V; }
        }

        public SamplerStateContent Sampler
        {
            get => _Sampler;
            set => _Sampler = value;
        }

        /// <summary>
        /// An array with a minimum of 1 element and a maximum of 4 elements.
        /// </summary>
        /// <remarks>
        /// If this channel does not have a texture, this represents the
        /// overall value of this channel. Else, the texture samples must
        /// be scaled by this value
        /// </remarks>
        public float[] Value
        {
            get => _Value;
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                if (value.Length < 1 || value.Length > 4) throw new ArgumentOutOfRangeException(nameof(value));
                _Value = value;
            }
        }

        /// <summary>
        /// An object representing a serializable texture object
        /// </summary>
        /// <remarks>
        /// This can be a Byte[] array with an image (PNG, DDS, etc)
        /// </remarks>
        public Object Texture
        {
            get => _Texture;
            set => _Texture = value;
        }

        #endregion
    }
}
