using System.Numerics;
using System.Runtime.InteropServices;

namespace ColorTest
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Rectangle : IEquatable<Rectangle>
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Width;
        public readonly float Height;

        public Rectangle(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Rectangle(Vector2 position, Vector2 scale)
            : this(position.X, position.Y, scale.X, scale.Y) { }

        public readonly Vector2 Position => new(X, Y);
        public readonly Vector2 Center => new(X + (Width / 2f), Y + (Height / 2f));

        public readonly Vector2 Scale => new(Width, Height);

        public bool Equals(Rectangle other)
            => X == other.X &&
               Y == other.Y &&
               Width == other.Width &&
               Height == other.Height;

        public override readonly bool Equals(object? obj)
            => obj is Rectangle other && Equals(other);

        public override readonly int GetHashCode()
            => HashCode.Combine(X, Y, Width, Height);

        public static bool operator ==(Rectangle a, Rectangle b)
            => a.Equals(b);
        public static bool operator !=(Rectangle a, Rectangle b)
            => !a.Equals(b);
    }
}
