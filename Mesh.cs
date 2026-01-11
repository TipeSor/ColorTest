using System.Numerics;

namespace ColorTest
{
    public readonly struct Mesh
    {
        public readonly Vector3[] Vertices;
        public readonly Vector2[] Texcoords;
        public readonly int[] Indices;

        public int VertexCount => Vertices.Length;
        public int TriangleCount => Indices.Length / 3;

        public Mesh(
            Vector3[] vertices,
            int[] indices,
            Vector2[]? texcoords = null)
        {
            if (vertices == null || vertices.Length == 0)
                throw new ArgumentException("Mesh must have vertices.");

            if (indices == null || indices.Length % 3 != 0)
                throw new ArgumentException("Indices must be a multiple of 3.");

            if (texcoords != null && texcoords.Length != vertices.Length)
                throw new ArgumentException("Texcoords must match vertex count.");

            Vertices = vertices;
            Indices = indices;
            Texcoords = texcoords ?? [];
        }
    }
}
