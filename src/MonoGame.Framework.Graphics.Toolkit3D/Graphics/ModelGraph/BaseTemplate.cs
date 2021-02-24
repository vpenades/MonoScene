using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    public abstract class BaseTemplate
    {
        public BaseTemplate() { }

        public BaseTemplate(string name) { Name = name; }

        public BaseTemplate(string name, Object tag) { Name = name; Tag = tag; }

        public string Name { get; set; }

        /// <summary>
        /// Custom data.
        /// </summary>
        /// <remarks>
        /// If inported from glTF, this property is set with the glTF.extras property.
        /// </remarks>
        public Object Tag { get; set; }
    }
}
