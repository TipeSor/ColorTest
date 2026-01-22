using System.Numerics;
using System.Runtime.CompilerServices;

namespace ColorTest
{
    public sealed class Texture
    {
        public int Width { get; }
        public int Height { get; }

        public Color[] Pixels { get; }
        public float[] Depth { get; }

        public Texture(int width, int height)
            : this(width, height, Color.Black) { }

        public Texture(int width, int height, Color clear)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentOutOfRangeException();

            Width = width;
            Height = height;
            Pixels = new Color[width * height];
            Depth = new float[width * height];
            Clear(clear);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Idx(int x, int y)
            => x + (y * Width);

        public Color this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Pixels[index];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Pixels[index] = value;
        }

        public Color this[int x, int y]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this[Idx(x, y)];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this[Idx(x, y)] = value;
        }

        public void Clear(Color color)
        {
            Array.Fill(Pixels, color);
            Array.Fill(Depth, float.PositiveInfinity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<Color> AsSpan()
            => Pixels.AsSpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BlendPixel(int index, Color src)
        {
            Pixels[index] = Color.BlendOver(src, Pixels[index]);
        }

        public void DrawPoint(Vector2 p, Color color)
        {
            int x = (int)MathF.Round(p.X);
            int y = (int)MathF.Round(p.Y);

            if ((uint)x < (uint)Width &&
                (uint)y < (uint)Height)
            {
                this[x, y] = color;
            }
        }

        public void DrawLine(Vector2 p0, Vector2 p1, Color color)
        {
            int x0 = (int)MathF.Round(p0.X);
            int y0 = (int)MathF.Round(p0.Y);
            int x1 = (int)MathF.Round(p1.X);
            int y1 = (int)MathF.Round(p1.Y);

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);

            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;

            int err = dx - dy;

            while (true)
            {
                if ((uint)x0 < (uint)Width &&
                    (uint)y0 < (uint)Height)
                {
                    this[x0, y0] = color;
                }

                if (x0 == x1 && y0 == y1)
                    break;

                int e2 = err << 1;

                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }

                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        public void DrawTexture(
            Texture tex,
            Vector2 position,
            Vector2 scale)
        {
            float sx = scale.X;
            float sy = scale.Y;

            int dw = (int)MathF.Abs(tex.Width * sx);
            int dh = (int)MathF.Abs(tex.Height * sy);

            if (dw == 0 || dh == 0)
                return;

            int start_x = int.Max((int)position.X, 0);
            int start_y = int.Max((int)position.Y, 0);

            int end_x = int.Min(start_x + dw, Width);
            int end_y = int.Min(start_y + dh, Height);

            bool flipX = sx < 0f;
            bool flipY = sy < 0f;

            for (int y = start_y; y < end_y; y++)
            {
                for (int x = start_x; x < end_x; x++)
                {
                    float u = (x - start_x + 0.5f) / dw;
                    float v = (y - start_y + 0.5f) / dh;

                    if (flipX) u = 1f - u;
                    if (flipY) v = 1f - v;

                    int tx = Math.Clamp((int)(u * tex.Width), 0, tex.Width - 1);
                    int ty = Math.Clamp((int)(v * tex.Height), 0, tex.Height - 1);

                    BlendPixel(Idx(x, y), tex[tx, ty]);
                }
            }
        }
    }
}
