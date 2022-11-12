

using SYSV2 = System.Numerics.Vector2;
using SYSV3 = System.Numerics.Vector3;
using SYSV4 = System.Numerics.Vector4;
using SYSQ4 = System.Numerics.Quaternion;
using SYSM4X4 = System.Numerics.Matrix4x4;

using XNAV2 = Microsoft.Xna.Framework.Vector2;
using XNAV3 = Microsoft.Xna.Framework.Vector3;
using XNAV4 = Microsoft.Xna.Framework.Vector4;
using XNAQ4 = Microsoft.Xna.Framework.Quaternion;
using XNAM4X4 = Microsoft.Xna.Framework.Matrix;



namespace System
{
    internal static partial class _PrivateExtensions
    {
        public static XNAV2 ToXNA(this SYSV2 value) { return new XNAV2(value.X, value.Y); }
        public static XNAV3 ToXNA(this SYSV3 value) { return new XNAV3(value.X, value.Y, value.Z); }
        public static XNAV4 ToXNA(this SYSV4 value) { return new XNAV4(value.X, value.Y, value.Z, value.W); }
        public static XNAQ4 ToXNA(this SYSQ4 value) { return new XNAQ4(value.X, value.Y, value.Z, value.W); }

        public static XNAM4X4 ToXNA(this SYSM4X4 value)
        {
            return new XNAM4X4
                (
                value.M11, value.M12, value.M13, value.M14,
                value.M21, value.M22, value.M23, value.M24,
                value.M31, value.M32, value.M33, value.M34,
                value.M41, value.M42, value.M43, value.M44
                );
        }
    }
}
