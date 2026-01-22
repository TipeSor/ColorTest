using System.Runtime.CompilerServices;
using System.Text;

namespace ColorTest
{
    public static partial class ConsoleEx
    {
        public static void Log(
            object value,
            string? format = null,
            IFormatProvider? formatProvider = null,
            [CallerArgumentExpression(nameof(value))] string? expr = null)
        {
            string s = value is IFormattable f ? f.ToString(format, formatProvider) : value.ToString() ?? string.Empty;
            Console.Write($"{expr}: {s}");
        }

        public static void LogColor(Color color)
        {
            SetForeground(color);
            Console.Write("█");
            Reset();
        }

        public static void WriteProgress(int value, int maxValue, string? name = null)
        {
            if (name != null)
                Console.Write($"{name}: ");

            int width = maxValue.ToString().Length;

            Console.Write("[");
            Console.Write((value + 1).ToString($"D{width}"));
            Console.Write("/");
            Console.Write(maxValue);
            Console.Write("]");
        }

        public static void DrawTexture(Texture tex)
        {
            StringBuilder sb = new(tex.Width * tex.Height * 2);

            int lastFg = -1;
            int lastBg = -1;

            for (int y = 0; y < tex.Height; y += 2)
            {
                sb.Append(ANSI.CursorPosition(0, y / 2));

                for (int x = 0; x < tex.Width; x++)
                {
                    Color top = tex[x, y];
                    Color btm = (y + 1 < tex.Height)
                        ? tex[x, y + 1]
                        : Color.Black;

                    int fg = top.ToRgb24();
                    int bg = btm.ToRgb24();

                    if (fg != lastFg)
                    {
                        sb.Append(ANSI.Foreground(fg));
                        lastFg = fg;
                    }

                    if (bg != lastBg)
                    {
                        sb.Append(ANSI.Background(bg));
                        lastBg = bg;
                    }

                    sb.Append('▀');
                }
            }

            sb.Append(ANSI.Reset);
            Console.Write(sb);
        }
    }

    public static partial class ConsoleEx
    {
        public static void SetForeground(Color color)
            => Console.Write(ANSI.Foreground(color.ToRgb24()));

        public static void SetBackground(Color color)
            => Console.Write(ANSI.Background(color.ToRgb24()));

        public static void SetColor(Color fg, Color bg)
        { SetForeground(fg); SetBackground(bg); }

        public static void Reset()
            => Console.Write(ANSI.Reset);
    }

    public static class ANSI
    {
        public static string Foreground(int rgb)
        {
            int r = (rgb >> 16) & 0xFF;
            int g = (rgb >> 8) & 0xFF;
            int b = rgb & 0xFF;
            return $"\u001b[38;2;{r};{g};{b}m";
        }

        public static string Background(int rgb)
        {
            int r = (rgb >> 16) & 0xFF;
            int g = (rgb >> 8) & 0xFF;
            int b = rgb & 0xFF;
            return $"\u001b[48;2;{r};{g};{b}m";
        }

        public static string CursorUp(int amount)
            => $"\u001b[{amount}A";

        public static string CursorDown(int amount)
            => $"\u001b[{amount}B";

        public static string CursorRight(int amount)
            => $"\u001b[{amount}C";

        public static string CursorLeft(int amount)
            => $"\u001b[{amount}D";

        public static string CursorPosition(int left, int top)
            => $"\u001b[{top + 1};{left + 1}H";

        public static readonly string Clear
            = "\u001b[2J";

        public static readonly string ClearToEOL
            = "\u001b[K";

        public static readonly string Reset
            = "\u001b[0m";
    }

    public static class Cursor
    {
        public static int Left
        {
            get => Console.CursorLeft;
            set => Console.CursorLeft = value;
        }

        public static int Top
        {
            get => Console.CursorTop;
            set => Console.CursorTop = value;
        }

        public static (int left, int top) Position
        {
            get => Console.GetCursorPosition();
            set => Console.SetCursorPosition(value.left, value.top);
        }

        public static bool Visible
        {
            get => OperatingSystem.IsWindows() && Console.CursorVisible;
            set => Console.CursorVisible = value;
        }

        public static void MoveLeft(int amount = 1) => Left -= amount;
        public static void MoveRight(int amount = 1) => Left += amount;
        public static void MoveUp(int amount = 1) => Top -= amount;
        public static void MoveDown(int amount = 1) => Top += amount;

        public static void SetPosition(int left, int top) => Position = (left, top);

        public static void Hide() => Visible = false;
        public static void Show() => Visible = true;
    }
}

