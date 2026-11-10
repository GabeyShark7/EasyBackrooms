using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace EasyBackrooms
{
    // Builders add quads and boxes here
    public sealed class MeshData
    {
        public readonly List<Vector3> Vertices = new List<Vector3>();
        public readonly List<Vector3> Normals = new List<Vector3>();
        public readonly List<Vector2> Uvs = new List<Vector2>();
        public readonly List<int> Triangles = new List<int>();

        Vector3 uvOrigin;
        float tileSize = 1f;

        public bool IsEmpty => Vertices.Count == 0;

        public void Clear()
        {
            Vertices.Clear();
            Normals.Clear();
            Uvs.Clear();
            Triangles.Clear();
        }

        public void SetUvMapping(Vector3 worldOrigin, float tile)
        {
            tileSize = Mathf.Max(0.01f, tile);
            uvOrigin = new Vector3(
                Mathf.Repeat(worldOrigin.x, tileSize),
                Mathf.Repeat(worldOrigin.y, tileSize),
                Mathf.Repeat(worldOrigin.z, tileSize));
        }

        // Pass corners in order around it
        public void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal)
        {
            if (Vector3.Dot(Vector3.Cross(b - a, c - a), normal) < 0f) (b, d) = (d, b);

            int i = Vertices.Count;
            AddVertex(a, normal);
            AddVertex(b, normal);
            AddVertex(c, normal);
            AddVertex(d, normal);

            Triangles.Add(i);
            Triangles.Add(i + 1);
            Triangles.Add(i + 2);
            Triangles.Add(i);
            Triangles.Add(i + 2);
            Triangles.Add(i + 3);
        }

        public void AddBox(Vector3 min, Vector3 max, bool top, bool bottom)
        {
            AddQuad(new Vector3(min.x, min.y, min.z), new Vector3(min.x, max.y, min.z),
                    new Vector3(min.x, max.y, max.z), new Vector3(min.x, min.y, max.z), Vector3.left);
            AddQuad(new Vector3(max.x, min.y, min.z), new Vector3(max.x, max.y, min.z),
                    new Vector3(max.x, max.y, max.z), new Vector3(max.x, min.y, max.z), Vector3.right);
            AddQuad(new Vector3(min.x, min.y, min.z), new Vector3(max.x, min.y, min.z),
                    new Vector3(max.x, max.y, min.z), new Vector3(min.x, max.y, min.z), Vector3.back);
            AddQuad(new Vector3(min.x, min.y, max.z), new Vector3(max.x, min.y, max.z),
                    new Vector3(max.x, max.y, max.z), new Vector3(min.x, max.y, max.z), Vector3.forward);

            if (top)
                AddQuad(new Vector3(min.x, max.y, min.z), new Vector3(min.x, max.y, max.z),
                        new Vector3(max.x, max.y, max.z), new Vector3(max.x, max.y, min.z), Vector3.up);
            if (bottom)
                AddQuad(new Vector3(min.x, min.y, min.z), new Vector3(min.x, min.y, max.z),
                        new Vector3(max.x, min.y, max.z), new Vector3(max.x, min.y, min.z), Vector3.down);
        }

        void AddVertex(Vector3 position, Vector3 normal)
        {
            Vertices.Add(position);
            Normals.Add(normal);
            Uvs.Add(Project(position + uvOrigin, normal) / tileSize);
        }

        // World UVs so chunks tile seamlessly
        static Vector2 Project(Vector3 p, Vector3 n)
        {
            if (Mathf.Abs(n.y) > 0.5f) return new Vector2(p.x, p.z);
            var right = Vector3.Cross(n, Vector3.up);
            return new Vector2(Vector3.Dot(p, right), p.y);
        }

        public Mesh ToMesh(string name)
        {
            var mesh = new Mesh { name = name };
            mesh.indexFormat = Vertices.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.SetVertices(Vertices);
            mesh.SetNormals(Normals);
            mesh.SetUVs(0, Uvs);
            mesh.SetTriangles(Triangles, 0);
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
