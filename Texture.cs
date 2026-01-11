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

        public Span<Color> AsSpan()
            => Pixels.AsSpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BlendPixel(int index, Color src)
        {
            Pixels[index] = Color.BlendOver(src, Pixels[index]);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3 ToScreen(Vector4 v)
        {
            float invW = 1f / v.W;
            return new Vector3(
                v.X * invW,
                v.Y * invW,
                v.Z
            );
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

        public void DrawMesh(
            Texture tex,
            Mesh mesh,
            Transform transform,
            Camera camera,
            Matrix4x4 viewport)
        {
            Matrix4x4 m = transform.GetMatrix() * 
                            camera.GetViewMatrix() * 
                            camera.GetProjectionMatrix(Width / Height) *
                            viewport;

            for (int i = 0; i < mesh.TriangleCount; i++)
            {
                int baseIndex = i * 3;

                int i0 = mesh.Indices[baseIndex];
                int i1 = mesh.Indices[baseIndex + 1];
                int i2 = mesh.Indices[baseIndex + 2];

                Vector4 c0 = Vector4.Transform(new Vector4(mesh.Vertices[i0], 1), m);
                Vector4 c1 = Vector4.Transform(new Vector4(mesh.Vertices[i1], 1), m);
                Vector4 c2 = Vector4.Transform(new Vector4(mesh.Vertices[i2], 1), m);

                Vector3 s0 = ToScreen(c0);
                Vector3 s1 = ToScreen(c1);
                Vector3 s2 = ToScreen(c2);

                DrawTrig(
                    tex,
                    s0, s1, s2,
                    mesh.Texcoords[i0],
                    mesh.Texcoords[i1],
                    mesh.Texcoords[i2]
                );
            }
        }

        public void DrawTrig(
            Texture texture,
            Vector3 v1, Vector3 v2, Vector3 v3,
            Vector2 uv1, Vector2 uv2, Vector2 uv3)
        {
            int minX = Math.Max(0, (int)MathF.Floor(Min(v1.X, v2.X, v3.X)));
            int minY = Math.Max(0, (int)MathF.Floor(Min(v1.Y, v2.Y, v3.Y)));
            int maxX = Math.Min(Width - 1, (int)MathF.Ceiling(Max(v1.X, v2.X, v3.X)));
            int maxY = Math.Min(Height - 1, (int)MathF.Ceiling(Max(v1.Y, v2.Y, v3.Y)));

            float area = Edge(v1, v2, v3.X, v3.Y);
            if (area == 0) return;

            float invZ1 = 1f / v1.Z;
            float invZ2 = 1f / v2.Z;
            float invZ3 = 1f / v3.Z;

            Vector2 uv1z = uv1 * invZ1;
            Vector2 uv2z = uv2 * invZ2;
            Vector2 uv3z = uv3 * invZ3;

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float px = x + 0.5f;
                    float py = y + 0.5f;

                    float w1 = Edge(v2, v3, px, py);
                    float w2 = Edge(v3, v1, px, py);
                    float w3 = Edge(v1, v2, px, py);

                    if ((w1 < 0 || w2 < 0 || w3 < 0) &&
                        (w1 > 0 || w2 > 0 || w3 > 0))
                        continue;

                    w1 /= area;
                    w2 /= area;
                    w3 /= area;

                    float invZ =
                        (w1 * invZ1) +
                        (w2 * invZ2) +
                        (w3 * invZ3);

                    if (invZ <= 0) continue;

                    float depth = 1f / invZ;

                    int idx = Idx(x, y);

                    if (depth >= Depth[idx])
                        continue;

                    Depth[idx] = depth;

                    Vector2 uv =
                        ((w1 * uv1z) + (w2 * uv2z) + (w3 * uv3z)) * depth;

                    int tx = (int)(uv.X * (texture.Width - 1));
                    int ty = (int)(uv.Y * (texture.Height - 1));

                    tx = Math.Clamp(tx, 0, texture.Width - 1);
                    ty = Math.Clamp(ty, 0, texture.Height - 1);

                    BlendPixel(idx, texture[tx, ty]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Edge(Vector3 a, Vector3 b, float x, float y)
        {
            // (y1 − y0)x + (x0 − x1)y + (x1y0 − x0y1)
            return ((b.Y - a.Y) * x) +
                   ((a.X - b.X) * y) +
                   ((b.X * a.Y) - (a.X * b.Y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Min(float a, float b, float c) =>
            float.Min(float.Min(a, b), c);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Max(float a, float b, float c) =>
            float.Max(float.Max(a, b), c);
    }
}
