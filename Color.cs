using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ColorTest
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Color
    {
        public readonly float R;
        public readonly float G;
        public readonly float B;
        public readonly float A;

        private const float INV_255 = 1.0f / 255.0f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Color(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Color(float r, float g, float b)
            : this(r, g, b, 1.0f) { }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color Rgba(float r, float g, float b, float a)
            => new(r, g, b, a);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color Rgb(float r, float g, float b)
            => new(r, g, b, 1.0f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color Rgba8(byte r, byte g, byte b, byte a)
            => new(
                r * INV_255,
                g * INV_255,
                b * INV_255,
                a * INV_255
            );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color Rgb8(byte r, byte g, byte b)
            => new(
                r * INV_255,
                g * INV_255,
                b * INV_255,
                1
            );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color Rgba32(uint rgba)
            => Rgba8(
                r: (byte)((rgba >> 24) & 0xFF),
                g: (byte)((rgba >> 16) & 0xFF),
                b: (byte)((rgba >> 8) & 0xFF),
                a: (byte)(rgba & 0xFF)
            );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color Rgb24(uint rgb)
            => Rgba8(
                (byte)(rgb >> 16),
                (byte)(rgb >> 8),
                (byte)rgb,
                255
            );


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte ToByte(float v)
            => (byte)Math.Clamp((int)((v * 255.0f) + 0.5f), 0, 255);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void ToRgb8(out byte r, out byte g, out byte b)
        {
            float a = A;
            r = ToByte(R * a);
            g = ToByte(G * a);
            b = ToByte(B * a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void ToRgba8(out byte r, out byte g, out byte b, out byte a)
        {
            r = ToByte(R);
            g = ToByte(G);
            b = ToByte(B);
            a = ToByte(A);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int ToRgb24()
        {
            float a = A;
            return
                (ToByte(R * a) << 16) |
                (ToByte(G * a) << 8) |
                 ToByte(B * a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly uint ToRgba32()
        {
            return
                ((uint)ToByte(R) << 24) |
                ((uint)ToByte(G) << 16) |
                ((uint)ToByte(B) << 8) |
                 ToByte(A);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(Color other) =>
            R == other.R &&
            G == other.G &&
            B == other.B &&
            A == other.A;

        public override readonly bool Equals(object? obj)
            => obj is Color other && Equals(other);

        public override readonly int GetHashCode()
            => HashCode.Combine(R, G, B, A);

        public static bool operator ==(Color a, Color b) => a.Equals(b);
        public static bool operator !=(Color a, Color b) => !a.Equals(b);

        public static readonly Color Black = Rgba8(0, 0, 0, 255);
        public static readonly Color White = Rgba8(255, 255, 255, 255);
        public static readonly Color Red = Rgba8(255, 0, 0, 255);
        public static readonly Color Green = Rgba8(0, 255, 0, 255);
        public static readonly Color Blue = Rgba8(0, 0, 255, 255);

        public static Color BlendOver(Color src, Color dst)
        {
            float sa = src.A;
            float da = dst.A;

            float outA = sa + (da * (1.0f - sa));

            if (outA <= 0.0f)
                return new Color(0, 0, 0, 0);

            float r =
                ((src.R * sa) + (dst.R * da * (1.0f - sa))) / outA;
            float g =
                ((src.G * sa) + (dst.G * da * (1.0f - sa))) / outA;
            float b =
                ((src.B * sa) + (dst.B * da * (1.0f - sa))) / outA;

            return new Color(r, g, b, outA);
        }

        public static void Pack(
            ReadOnlySpan<Color> src,
            Span<byte> dst)
        {
            {
                for (int i = 0; i < src.Length; i++)
                {
                    int o = i * 4;
                    Color c = src[i];

                    dst[o + 0] = ToByte(c.R);
                    dst[o + 1] = ToByte(c.G);
                    dst[o + 2] = ToByte(c.B);
                    dst[o + 3] = ToByte(c.A);
                }
            }
        }
    }
}
