using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ColorTest
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Camera
    {
        public Vector3 Position;
        public Vector3 Target;
        public Vector3 Up;
        public float Fovy;

        public readonly Vector3 Forward => Vector3.Normalize(Target - Position);
        public readonly Vector3 Right => Vector3.Normalize(Vector3.Cross(Forward, Up));

        public readonly Matrix4x4 GetViewMatrix()
            => Matrix4x4.CreateLookAt(
                Position,
                Target,
                Up
            );

        public readonly Matrix4x4 GetProjectionMatrix(float aspect)
            => Matrix4x4.CreatePerspectiveFieldOfView(
                    MathF.PI / 180f * Fovy,
                    aspect,
                    0.1f,
                    1000f
                );
    }

    public class Program
    {
        public const int ScreenWidth = 100;
        public const int ScreenHeight = 100;
        public static readonly Color Background = Color.Rgb24(0x181818);

        public static int top = 0;

        public static void CleanUp()
        {
            Console.Write(ANSI.Reset);
            Cursor.SetPosition(0, 32 + 18);
            Cursor.Show();
        }

        public static void Main()
        {
            // setup console
            Console.Clear();
            Cursor.Hide();
            Console.CancelKeyPress += static (_, _) => CleanUp();

            // setup textures
            Texture screen = new(ScreenWidth, ScreenHeight);
            Texture sushi = TGA.Load("res/test.tga");

            Vector3 start = new(0, 0, 2);
            Vector3 end = new(0, 0, 6);

            Camera camera = new Camera
            {
                Position = start,
                Target = new(0, 0, 0),
                Up = Vector3.UnitY,
                Fovy = 90
            };

#pragma warning disable IDE0055
            Matrix4x4 viewport = new(
                ScreenWidth * 0.5f, 0                   , 0, 0,
                0                 , -ScreenHeight * 0.5f, 0, 0,
                0                 , 0                   , 1, 0,
                ScreenWidth * 0.5f, ScreenHeight * 0.5f , 0, 1
            );
#pragma warning restore IDE0055


            // setup mesh
            // Mesh mesh = Other.CreateCube();
            Mesh mesh = ObjLoader.Load("res/test.obj");

            Stopwatch total_sw = Stopwatch.StartNew();
            Stopwatch sw = Stopwatch.StartNew();

            const double target_ms = 1f / 60f * 1000f;

            int total_frames = 600;
            for (int frame = 0; frame < total_frames; frame++)
            {
                top = 0;
                sw.Restart();

                // math stuff
                float progress = frame / (float)total_frames;
                screen.Clear(Background);

                // camera.Position = Vector3.Lerp(start, end, progress);

                Matrix4x4 world =
                    Matrix4x4.CreateScale(1.0f) *
                    Matrix4x4.CreateFromQuaternion(
                        Quaternion.CreateFromAxisAngle(Vector3.UnitX, progress * float.Pi) *
                        Quaternion.CreateFromAxisAngle(Vector3.UnitY, progress * float.Pi * 4) *
                        Quaternion.CreateFromAxisAngle(Vector3.UnitZ, progress * float.Pi)
                    ) *
                    Matrix4x4.CreateTranslation(0f, 0f, 0f);

                Matrix4x4 mvp = world * camera.GetViewMatrix() * camera.GetProjectionMatrix(ScreenWidth / ScreenHeight);
                Matrix4x4 mvpScreen = mvp * viewport;

                screen.DrawMesh(sushi, mesh, mvpScreen);
                Shade(screen);

                ConsoleEx.DrawTexture(screen);

                // ui
                Cursor.SetPosition(100, top++);
                ConsoleEx.WriteProgress(frame, total_frames, "Frame");
                Console.Write(ANSI.ClearToEOL);

                Cursor.SetPosition(100, top++);
                double frame_time = sw.Elapsed.TotalMilliseconds;
                ConsoleEx.Log(frame_time, "00.000ms");
                Console.Write(ANSI.ClearToEOL);

                Cursor.SetPosition(100, top++);
                double fps = frame / total_sw.Elapsed.TotalSeconds;
                ConsoleEx.Log(fps, "00.000");
                Console.Write(ANSI.ClearToEOL);

                // wait
                double elapsed = sw.Elapsed.TotalMilliseconds;
                if (elapsed < target_ms)
                    Thread.Sleep((int)(target_ms - elapsed));
            }

            CleanUp();
        }

        public static void Shade(Texture tex)
        {
            float min = 0;
            float max = 2;

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


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RgbShade(Texture tex)
        {
            RgbShade(tex, 1.0f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IRgbShade(Texture tex)
        {
            IRgbShade(tex, 1.0f);
        }

        public static void RgbShade(Texture tex, float a)
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

        public static void IRgbShade(Texture tex, float a)
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
