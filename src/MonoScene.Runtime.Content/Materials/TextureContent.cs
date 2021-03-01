using System;
using System.Collections.Generic;
using System.Text;

namespace MonoScene.Graphics.Content
{
    public abstract class TextureContent : BaseContent
    {
        public abstract int GetContentHashCode();

        public static IEqualityComparer<TextureContent> ContentComparer => _ContentComparer._Instance;

        private sealed class _ContentComparer : IEqualityComparer<TextureContent>
        {
            internal static readonly _ContentComparer _Instance = new _ContentComparer();

            public int GetHashCode(TextureContent obj) { return obj.GetContentHashCode(); }

            public bool Equals(TextureContent x, TextureContent y)
            {
                if (x is ImageContent ximg && y is ImageContent yimg)
                {
                    return ImageContent.AreEqualByContent(ximg, yimg);
                }

                return false;
            }

            
        }
    }

    public sealed class ImageContent : TextureContent
    {
        public ImageContent(Byte[] data) { _ImageData = data; }

        private Byte[] _ImageData; // a jpg, png or dds

        public Byte[] Data => _ImageData;

        public override int GetContentHashCode()
        {
            return _ImageData.Length;
        }

        public static bool AreEqualByContent(ImageContent a, ImageContent b)
        {
            return a._ImageData.AsSpan().SequenceEqual(b._ImageData);
        }
    }
}
