using System.Diagnostics;

namespace ColorTest
{
    public class Program
    {
        public static int top = 0;
        public static Stopwatch total_sw = new();
        public static Stopwatch sw = new();

        public const int Width = 200;
        public const int Height = 200;

        public static void CleanUp()
        {
            Console.Write(ANSI.Reset);
            Cursor.Show();
        }

        public static void Setup()
        {
            Console.Clear();
            Cursor.Hide();
            Console.CancelKeyPress += static (_, _) => CleanUp();
        }

        public static void Main()
        {
            Setup();

            CleanUp();
        }
    }
}
