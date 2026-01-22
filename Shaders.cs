using System.Numerics;
using System.Runtime.InteropServices;

namespace ColorTest
{
    public static class Shaders
    {
        public static void SimpleLight(Texture tex, float min, float max)
        {
            for (int y = 0; y < tex.Height; y++)
            {
                for (int x = 0; x < tex.Width; x++)
                {
                    int index = tex.Idx(x, y);
                    float depth = tex.Depth[index];
                    if (depth == float.PositiveInfinity)
                        continue;

                    Color c = Color.Rgba(0, 0, 0, MathF.Pow((depth - min) / (max - min), 2));
                    tex.BlendPixel(index, c);
                }
            }
        }

        public static void Gradient(Texture tex, float a = 1.0f)
        {
            int width = tex.Width;
            int height = tex.Height;

            float invH = 1.0f / tex.Height;
            float invW = 1.0f / tex.Width;

            Span<Vector4> pixels =
                MemoryMarshal.Cast<Color, Vector4>(tex.AsSpan());

            int index = 0;
            for (int y = 0; y < height; y++)
            {
                float g = y * invH;
                for (int x = 0; x < width; x++)
                {
                    pixels[index++] = new Vector4(x * invW, g, 0, a);
                }
            }
        }

        public static void Grayscale(Texture tex)
        {
            int width = tex.Width;
            int height = tex.Height;

            const float Wr = 0.2126f;
            const float Wg = 0.7152f;
            const float Wb = 0.0722f;

            Span<Vector4> pixels =
                MemoryMarshal.Cast<Color, Vector4>(tex.AsSpan());

            for (int index = 0; index < height * width; index++)
            {
                Vector4 p = pixels[index];
                float gray =
                    (p.X * Wr) +
                    (p.Y * Wg) +
                    (p.Z * Wb);
                pixels[index] = new Vector4(gray, gray, gray, p.W);
            }
        }
    }
}
