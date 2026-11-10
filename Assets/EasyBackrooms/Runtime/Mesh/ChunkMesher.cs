using UnityEngine;

namespace EasyBackrooms
{
    public static class ChunkMesher
    {
        public static ChunkView Build(ChunkLayout layout, BackroomsProfile profile, BackroomsTheme theme,
            Transform parent, bool ownsEastEdge, bool ownsNorthEdge)
        {
            float size = profile.ChunkWorldSize;
            var origin = new Vector3(layout.Coord.x * size, 0f, layout.Coord.y * size);

            var root = new GameObject($"Chunk {layout.Coord.x} {layout.Coord.y}");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = origin;

            var view = root.AddComponent<ChunkView>();
            view.Coord = layout.Coord;

            var context = new BuildContext(layout, profile.cellSize, profile.wallHeight, origin, ownsEastEdge, ownsNorthEdge);
            var data = new MeshData();

            // One child object per surface builder
            foreach (var builder in theme.builders)
            {
                if (builder == null || !builder.Active) continue;

                data.Clear();
                data.SetUvMapping(origin, builder.tileSize);
                builder.Build(context, data);
                if (data.IsEmpty) continue;

                var go = new GameObject(builder.name);
                go.transform.SetParent(root.transform, false);

                var mesh = data.ToMesh($"{root.name} {builder.name}");
                view.Meshes.Add(mesh);

                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = go.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = builder.material;
                renderer.shadowCastingMode = builder.shadows;

                if (builder.addCollider) go.AddComponent<MeshCollider>().sharedMesh = mesh;
            }

            return view;
        }
    }
}
