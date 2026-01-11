using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

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
            for (int y = 0; y < tex.Height; y++)
            {
                for (int x = 0; x < tex.Width; x++)
                {
                    tex[x, y] = Color.Rgba(
                        r: (float)y / tex.Height,
                        g: (float)x / tex.Width,
                        b: 0.0f,
                        a: a
                    );
                }
            }
        }

        public static void IGradient(Texture tex, float a = 1.0f)
        {
            for (int y = 0; y < tex.Height; y++)
            {
                for (int x = 0; x < tex.Width; x++)
                {
                    tex[x, y] = Color.Rgba(
                        r: 1.0f - ((float)y / tex.Height),
                        g: 1.0f - ((float)x / tex.Width),
                        b: 0.0f,
                        a: a
                    );
                }
            }
        }
    }
}
