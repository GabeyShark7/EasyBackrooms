using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyBackrooms
{
    // Sits on every built chunk
    [DisallowMultipleComponent]
    public sealed class ChunkView : MonoBehaviour
    {
        [Serializable]
        public sealed class SurfacePart
        {
            public GameObject GameObject;
            public MeshFilter Filter;
            public MeshRenderer Renderer;
            public MeshCollider Collider;
            public Mesh Mesh;
        }

        public Vector2Int Coord;

        [SerializeField]
        List<SurfacePart> parts = new List<SurfacePart>();

        public IReadOnlyList<SurfacePart> Parts => parts;

        internal SurfacePart GetPart(int index, string name)
        {
            while (parts.Count <= index) parts.Add(CreatePart());

            var part = parts[index];
            if (part.GameObject == null)
            {
                part = CreatePart();
                parts[index] = part;
            }

            part.GameObject.name = name;
            part.Mesh.name = $"{gameObject.name} {name}";
            return part;
        }

        internal void HidePartsFrom(int index)
        {
            for (int i = index; i < parts.Count; i++)
            {
                if (parts[i].GameObject != null) parts[i].GameObject.SetActive(false);
            }
        }

        SurfacePart CreatePart()
        {
            var go = new GameObject("Surface");
            go.transform.SetParent(transform, false);

            var part = new SurfacePart
            {
                GameObject = go,
                Filter = go.AddComponent<MeshFilter>(),
                Renderer = go.AddComponent<MeshRenderer>(),
                Mesh = new Mesh()
            };
            part.Filter.sharedMesh = part.Mesh;
            return part;
        }

        public void DestroyMeshes()
        {
            foreach (var part in parts)
            {
                if (part.Mesh == null) continue;
                if (Application.isPlaying) Destroy(part.Mesh);
                else DestroyImmediate(part.Mesh);
            }
            parts.Clear();
        }
    }
}
