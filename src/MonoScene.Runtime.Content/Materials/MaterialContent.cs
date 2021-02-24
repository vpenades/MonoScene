using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoScene.Graphics.Content
{
    /// <summary>
    /// Represents a material, defined as a collection of <see cref="MaterialChannelContent"/> elements.
    /// </summary>
    /// <remarks>
    /// Unlike MonoGame's native <see cref="Microsoft.Xna.Framework.Graphics.Model"/>,<br/>
    /// this material architecture does not store any <see cref="Microsoft.Xna.Framework.Graphics.Effect"/>,<br/>
    /// which must be created and assigned at load time.
    /// </remarks>
    [System.Diagnostics.DebuggerDisplay("{Name} - {TargetEffectName}")]
    public class MaterialContent : BaseContent
    {
        #region lifecycle
        public MaterialContent() { }

        #endregion

        #region data

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)]
        private readonly List<MaterialChannelContent> _Channels = new List<MaterialChannelContent>();

        #endregion

        #region properties
        
        /// <summary>
        /// This defined the shading profile for this material. Loading from glTF we can find:
        /// - "Unlit"
        /// - "MetallicRoughness"
        /// - "SpecularGlossiness"
        /// </summary>
        /// <remarks>
        /// This value determines the usage of the <see cref="MaterialChannelContent"/> collection.
        /// </remarks>
        public string PreferredShading { get; set; }

        public MaterialBlendMode Mode { get; set; }

        public bool DoubleSided { get; set; }        

        public float AlphaCutoff { get; set; }

        #endregion

        #region API

        public MaterialChannelContent UseChannel(string name)
        {
            var channel = FindChannel(name);

            if (channel == null)
            {
                channel = new MaterialChannelContent(name);
                _Channels.Add(channel);
            }

            return channel;
        }        

        public MaterialChannelContent FindChannel(string name)
        {
            return _Channels.FirstOrDefault(item => item.Target == name);
        }

        #endregion
    }    
}
