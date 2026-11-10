using System.Collections.Generic;
using UnityEngine;

namespace EasyBackrooms
{
    // Sits on every built chunk
    [DisallowMultipleComponent]
    public sealed class ChunkView : MonoBehaviour
    {
        public Vector2Int Coord;
        public List<Mesh> Meshes = new List<Mesh>();

        public void DestroyMeshes()
        {
            foreach (var mesh in Meshes)
            {
                if (mesh == null) continue;
                if (Application.isPlaying) Destroy(mesh);
                else DestroyImmediate(mesh);
            }
            Meshes.Clear();
        }
    }
}
