using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ColorTest
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Context
    {
        public int Width;
        public int Height;

        public Color Background;

        public float TargetMs;

        public Texture Screen;
        public Camera Camera;
        public Matrix4x4 Viewport;

        public Mesh Obj_Mesh;
        public Transform Obj_Transform;
        public Texture Obj_Texture;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FrameContext
    {
        public int TotalFrames;
        public int Frame;

        public readonly int Progress => Frame / TotalFrames;
    }

    public class Program
    {
        public static int top = 0;
        public static Stopwatch total_sw = new();
        public static Stopwatch sw = new();

        public static void CleanUp()
        {
            Console.Write(ANSI.Reset);
            Cursor.SetPosition(0, int.Min(50, Console.BufferHeight-2));
            Cursor.Show();
        }

        public static void Setup()
        {
            Console.Clear();
            Cursor.Hide();
            Console.CancelKeyPress += static (_, _) => CleanUp();
        }

        public static Context SetupContext()
        {
            // screen
            int width = 100;
            int height = 100;

            Texture screen = new(width, height);

            // camera
            Camera camera = new Camera
            {
                Position = new(0, 0, 2),
                Target = new(0, 0, 0),
                Up = Vector3.UnitY,
                Fovy = 90
            };

            // viewport
#pragma warning disable IDE0055
            Matrix4x4 viewport = new(
                width * 0.5f,              0, 0, 0,
                           0, -height * 0.5f, 0, 0,
                           0,              0, 1, 0,
                width * 0.5f,  height * 0.5f, 0, 1
            );
#pragma warning restore IDE0055

            // obj
            Mesh obj_mesh = ObjLoader.Load("res/test.obj");
            Texture obj_texture = TGA.Load("res/test.tga");
            Transform obj_transform = new Transform
            {
                Position = Vector3.Zero,
                Rotation = Quaternion.Identity,
                Scale = Vector3.One,
            };

            return new Context
            {
                Width = 100,
                Height = 100,
                Background = Color.Rgb8(25, 25, 25),

                TargetMs = 16.67f,

                Screen = screen,
                Camera = camera,
                Viewport = viewport,

                Obj_Mesh = obj_mesh,
                Obj_Transform = obj_transform,
                Obj_Texture = obj_texture,
            };
        }

        public static void Main()
        {
            Setup();
            Context ctx = SetupContext();            

            total_sw.Start();
            sw.Start();
            
            FrameContext fctx = new FrameContext { TotalFrames = 600, Frame = 0 };
            for (int frame = 0; frame < fctx.TotalFrames; frame++)
            {
                top = 0;
                sw.Restart();

                fctx = fctx with { Frame = frame };

                Draw(ctx, fctx);
                DrawUI(ctx, fctx);

                // wait
                double elapsed = sw.Elapsed.TotalMilliseconds;
                if (elapsed < ctx.TargetMs)
                    Thread.Sleep((int)(ctx.TargetMs - elapsed));
            }

            CleanUp();
        }

        public static void Draw(
            Context ctx, 
            FrameContext fctx)
        {
            ctx.Screen.Clear(ctx.Background);
            ctx.Screen.DrawMesh(
                ctx.Obj_Texture, 
                ctx.Obj_Mesh,
                ctx.Obj_Transform, 
                ctx.Camera, 
                ctx.Viewport);
            
            Shaders.SimpleLight(ctx.Screen, 2f, 4f);

            ConsoleEx.DrawTexture(ctx.Screen);
        }

        public static void DrawUI(
            Context ctx,
            FrameContext fctx)
        {
            Cursor.SetPosition(100, top++);
            ConsoleEx.WriteProgress(fctx.Frame, fctx.TotalFrames, "Frame");
            Console.Write(ANSI.ClearToEOL);

            Cursor.SetPosition(100, top++);
            double frame_time = sw.Elapsed.TotalMilliseconds;
            ConsoleEx.Log(frame_time, "00.000ms");
            Console.Write(ANSI.ClearToEOL);

            Cursor.SetPosition(100, top++);
            double fps = fctx.Frame / total_sw.Elapsed.TotalSeconds;
            ConsoleEx.Log(fps, "00.000");
            Console.Write(ANSI.ClearToEOL);
        }
    }
}
