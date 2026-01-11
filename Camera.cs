using System.Numerics;
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
}
