using System;
using System.Collections.Generic;
using System.Text;

namespace MonoScene.Graphics.Content
{
    public abstract class BaseContent
    {
        public BaseContent() { }

        public BaseContent(string name) { Name = name; }

        public BaseContent(string name, Object tag) { Name = name; Tag = tag; }

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
