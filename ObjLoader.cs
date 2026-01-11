using System.Globalization;
using System.Numerics;

namespace ColorTest
{
    public static class ObjLoader
    {
        public static Mesh Load(string path)
        {
            List<Vector3> positions = [];
            List<Vector2> texcoords = [];

            List<Vector3> outVertices = [];
            List<Vector2> outTexcoords = [];
            List<int> indices = [];

            Dictionary<(int v, int vt), int> vertexMap = [];

            foreach (string rawLine in File.ReadLines(path))
            {
                string line = rawLine.Trim();

                if (line.Length == 0 || line[0] == '#')
                    continue;

                string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                switch (parts[0])
                {
                    case "v":
                        {
                            positions.Add(new Vector3(
                                Parse(parts[1]),
                                Parse(parts[2]),
                                Parse(parts[3])
                            ));
                            break;
                        }

                    case "vt":
                        {
                            texcoords.Add(new Vector2(
                                Parse(parts[1]),
                                1f - Parse(parts[2]) // OBJ is bottom-left origin
                            ));
                            break;
                        }

                    case "f":
                        {
                            // triangulate fan
                            for (int i = 2; i < parts.Length; i++)
                            {
                                AddVertex(parts[1]);
                                AddVertex(parts[i]);
                                AddVertex(parts[i - 1]);
                            }
                            break;
                        }
                    default:
                        break;
                }
            }

            return new Mesh
            (
                [.. outVertices],
                [.. indices],
                [.. outTexcoords]
            );

            // ---- local helpers ----

            void AddVertex(string token)
            {
                // formats:
                // v
                // v/vt
                // v/vt/vn (vn ignored)

                int v = 0;
                int vt = -1;

                string[] elems = token.Split('/');

                v = FixIndex(int.Parse(elems[0]), positions.Count);

                if (elems.Length > 1 && elems[1].Length > 0)
                    vt = FixIndex(int.Parse(elems[1]), texcoords.Count);

                (int v, int vt) key = (v, vt);

                if (!vertexMap.TryGetValue(key, out int index))
                {
                    index = outVertices.Count;
                    vertexMap[key] = index;

                    outVertices.Add(positions[v]);
                    outTexcoords.Add(vt >= 0 ? texcoords[vt] : Vector2.Zero);
                }

                indices.Add(index);
            }
        }

        private static float Parse(string s)
            => float.Parse(s, CultureInfo.InvariantCulture);

        private static int FixIndex(int i, int count)
            => i > 0 ? i - 1 : count + i;
    }
}
