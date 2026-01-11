using System.Numerics;

namespace ColorTest
{
    public static class Other
    {
        public static Mesh CreateCube()
        {
            Vector3[] vertices = [
                // Front (+Z)
                new Vector3( 1.00f,  1.00f,  1.00f),
                new Vector3(-1.00f,  1.00f,  1.00f),
                new Vector3(-1.00f, -1.00f,  1.00f),
                new Vector3( 1.00f, -1.00f,  1.00f),

                // Back (-Z)
                new Vector3(-1.00f,  1.00f, -1.00f),
                new Vector3( 1.00f,  1.00f, -1.00f),
                new Vector3( 1.00f, -1.00f, -1.00f),
                new Vector3(-1.00f, -1.00f, -1.00f),

                // Left (-X)
                new Vector3(-1.00f,  1.00f,  1.00f),
                new Vector3(-1.00f,  1.00f, -1.00f),
                new Vector3(-1.00f, -1.00f, -1.00f),
                new Vector3(-1.00f, -1.00f,  1.00f),

                // Right (+X)
                new Vector3( 1.00f,  1.00f, -1.00f),
                new Vector3( 1.00f,  1.00f,  1.00f),
                new Vector3( 1.00f, -1.00f,  1.00f),
                new Vector3( 1.00f, -1.00f, -1.00f),

                // Top (+Y)
                new Vector3( 1.00f,  1.00f, -1.00f),
                new Vector3(-1.00f,  1.00f, -1.00f),
                new Vector3(-1.00f,  1.00f,  1.00f),
                new Vector3( 1.00f,  1.00f,  1.00f),

                // Bottom (-Y)
                new Vector3( 1.00f, -1.00f,  1.00f),
                new Vector3(-1.00f, -1.00f,  1.00f),
                new Vector3(-1.00f, -1.00f, -1.00f),
                new Vector3( 1.00f, -1.00f, -1.00f),
            ];

            int[] indices = [
                 0,  1,  2,   0,  2,  3,   // Front
                 4,  5,  6,   4,  6,  7,   // Back
                 8,  9, 10,   8, 10, 11,   // Left
                12, 13, 14,  12, 14, 15,   // Right
                16, 17, 18,  16, 18, 19,   // Top
                20, 21, 22,  20, 22, 23    // Bottom
            ];

            Vector2[] texcoords = [
                // Front
                new Vector2(1, 0),
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),

                // Back
                new Vector2(1, 0),
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),

                // Left
                new Vector2(1, 0),
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),

                // Right
                new Vector2(1, 0),
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),

                // Top
                new Vector2(1, 0),
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),

                // Bottom
                new Vector2(1, 0),
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),
            ];

            return new Mesh(vertices, indices, texcoords);
        }
    }
}
