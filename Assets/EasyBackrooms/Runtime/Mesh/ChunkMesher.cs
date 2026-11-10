using UnityEngine;

namespace EasyBackrooms
{
    public static class ChunkMesher
    {
        static readonly MeshData Shared = new MeshData();

        // Fills a new or reused chunk view
        public static void Build(ChunkView view, ChunkLayout layout, BackroomsProfile profile, BackroomsTheme theme,
            ChunkEdges ownedEdges, ChunkEdges sealedEdges)
        {
            float size = profile.ChunkWorldSize;
            var origin = new Vector3(layout.Coord.x * size, 0f, layout.Coord.y * size);

            view.name = $"Chunk {layout.Coord.x} {layout.Coord.y}";
            view.Coord = layout.Coord;
            view.transform.localPosition = origin;

            var context = new BuildContext(layout, profile.cellSize, profile.wallHeight, origin, ownedEdges, sealedEdges);
            var data = Shared;
            int used = 0;

            foreach (var builder in theme.builders)
            {
                if (builder == null || !builder.Active) continue;

                data.Clear();
                data.SetUvMapping(origin, builder.tileSize);
                builder.Build(context, data);

                var part = view.GetPart(used++, builder.name);
                part.GameObject.SetActive(!data.IsEmpty);
                if (data.IsEmpty) continue;

                data.ApplyTo(part.Mesh);
                part.Renderer.sharedMaterial = builder.material;
                part.Renderer.shadowCastingMode = builder.shadows;

                if (builder.addCollider)
                {
                    if (part.Collider == null) part.Collider = part.GameObject.AddComponent<MeshCollider>();
                    part.Collider.sharedMesh = null;
                    part.Collider.sharedMesh = part.Mesh;
                    part.Collider.enabled = true;
                }
                else if (part.Collider != null)
                {
                    part.Collider.enabled = false;
                }
            }

            view.HidePartsFrom(used);
        }
    }
}
