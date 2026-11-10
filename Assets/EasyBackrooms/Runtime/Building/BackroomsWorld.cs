using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace EasyBackrooms
{
    public enum WorldMode
    {
        Infinite,
        Fixed
    }

    // The main thing you drop in a scene
    [AddComponentMenu("EasyBackrooms/Backrooms World")]
    public sealed class BackroomsWorld : MonoBehaviour
    {
        [Serializable]
        public sealed class ChunkEvent : UnityEvent<ChunkView> { }

        [Tooltip("Decides the layout.")]
        public BackroomsProfile profile;

        [Tooltip("Decides the look. Holds your materials.")]
        public BackroomsTheme theme;

        [Tooltip("Infinite streams chunks around the target forever. Fixed builds one set area with walls around it.")]
        public WorldMode mode = WorldMode.Infinite;

        [Header("Infinite")]
        [Tooltip("What the chunks follow. Leave empty to use the main camera.")]
        public Transform target;

        [Range(1, 8), Tooltip("How many chunks out from the target stay loaded.")]
        public int loadRadius = 2;

        [Min(1), Tooltip("Most chunks built per frame. Lower is smoother, higher fills in faster.")]
        public int chunksPerFrame = 1;

        [Tooltip("Decides which chunks load. Leave empty for a square around the target.")]
        public ChunkTracker tracker;

        [Header("Fixed")]
        [Tooltip("How many chunks wide and long the area is.")]
        public Vector2Int size = new Vector2Int(3, 3);

        [Tooltip("Wall off the outside so nobody walks into the void.")]
        public bool sealEdges = true;

        [Tooltip("Build on play if nothing is built yet.")]
        public bool buildOnStart = true;

        [Header("Events")]
        public ChunkEvent chunkLoaded = new ChunkEvent();
        public ChunkEvent chunkUnloaded = new ChunkEvent();

        readonly Dictionary<Vector2Int, ChunkView> loaded = new Dictionary<Vector2Int, ChunkView>();
        readonly List<Vector2Int> wanted = new List<Vector2Int>();
        readonly List<Vector2Int> toUnload = new List<Vector2Int>();

        ChunkPool pool;
        Vector2Int lastCenter;
        bool hasCenter;
        int queueIndex;

        public IReadOnlyDictionary<Vector2Int, ChunkView> Loaded => loaded;

        void Start()
        {
            if (!HasAssets()) return;

            if (mode == WorldMode.Fixed)
            {
                if (buildOnStart && GetComponentInChildren<ChunkView>(true) == null) BuildFixed();
                return;
            }

            Clear();
            pool = new ChunkPool(transform);
        }

        void Update()
        {
            if (mode != WorldMode.Infinite || pool == null) return;

            var center = WorldToChunk(ResolveTarget().position);
            if (!hasCenter || center != lastCenter)
            {
                hasCenter = true;
                lastCenter = center;
                Refresh(center);
            }

            BuildQueued();
        }

        void OnDestroy()
        {
            foreach (var view in GetComponentsInChildren<ChunkView>(true)) view.DestroyMeshes();
        }

        public Vector2Int WorldToChunk(Vector3 worldPosition)
        {
            var local = transform.InverseTransformPoint(worldPosition);
            float s = profile.ChunkWorldSize;
            return new Vector2Int(Mathf.FloorToInt(local.x / s), Mathf.FloorToInt(local.z / s));
        }

        // Open floor nearest a chunk middle
        public Vector3 FindSpawnPoint(Vector2Int chunk = default)
        {
            if (profile == null) return transform.position;

            var layout = LayoutGenerator.Generate(profile, chunk);
            int n = layout.Size;
            float c = profile.cellSize;
            float best = float.MaxValue;
            var pick = new Vector2Int(n / 2, n / 2);

            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    if (layout.GetCell(x, y) != CellKind.Open) continue;
                    float d = (new Vector2(x, y) - new Vector2(n * 0.5f, n * 0.5f)).sqrMagnitude;
                    if (d >= best) continue;
                    best = d;
                    pick = new Vector2Int(x, y);
                }
            }

            float size = profile.ChunkWorldSize;
            var local = new Vector3(chunk.x * size + (pick.x + 0.5f) * c, 0.05f, chunk.y * size + (pick.y + 0.5f) * c);
            return transform.TransformPoint(local);
        }

        // Call this after changing the profile
        public void Rebuild()
        {
            if (mode == WorldMode.Fixed)
            {
                BuildFixed();
                return;
            }

            Clear();
            hasCenter = false;
        }

        public void BuildFixed()
        {
            Clear();
            if (!HasAssets()) return;

            int maxX = Mathf.Max(1, size.x) - 1;
            int maxY = Mathf.Max(1, size.y) - 1;

            for (int y = 0; y <= maxY; y++)
            {
                for (int x = 0; x <= maxX; x++)
                {
                    var owned = ChunkEdges.West | ChunkEdges.South;
                    if (x == maxX) owned |= ChunkEdges.East;
                    if (y == maxY) owned |= ChunkEdges.North;

                    var sealedEdges = ChunkEdges.None;
                    if (sealEdges)
                    {
                        if (x == 0) sealedEdges |= ChunkEdges.West;
                        if (x == maxX) sealedEdges |= ChunkEdges.East;
                        if (y == 0) sealedEdges |= ChunkEdges.South;
                        if (y == maxY) sealedEdges |= ChunkEdges.North;
                    }

                    Load(new Vector2Int(x, y), owned, sealedEdges);
                }
            }
        }

        public void Clear()
        {
            foreach (var view in GetComponentsInChildren<ChunkView>(true))
            {
                view.DestroyMeshes();
                if (Application.isPlaying) Destroy(view.gameObject);
                else DestroyImmediate(view.gameObject);
            }

            loaded.Clear();
            pool?.Forget();
        }

        void Refresh(Vector2Int center)
        {
            wanted.Clear();
            if (tracker != null) tracker.GetWanted(center, loadRadius, wanted);
            else SquareTracker.Fill(center, loadRadius, wanted);

            wanted.Sort((a, b) => (a - center).sqrMagnitude.CompareTo((b - center).sqrMagnitude));
            queueIndex = 0;

            // One chunk of slack stops flicker
            int keep = loadRadius + 1;
            toUnload.Clear();
            foreach (var pair in loaded)
            {
                var d = pair.Key - center;
                if (Mathf.Abs(d.x) > keep || Mathf.Abs(d.y) > keep) toUnload.Add(pair.Key);
            }

            foreach (var coord in toUnload) Unload(coord);
        }

        void BuildQueued()
        {
            int built = 0;
            while (queueIndex < wanted.Count && built < chunksPerFrame)
            {
                var coord = wanted[queueIndex++];
                if (loaded.ContainsKey(coord)) continue;

                Load(coord, ChunkEdges.West | ChunkEdges.South, ChunkEdges.None);
                built++;
            }
        }

        void Load(Vector2Int coord, ChunkEdges owned, ChunkEdges sealedEdges)
        {
            var layout = LayoutGenerator.Generate(profile, coord);
            var view = pool != null ? pool.Get() : ChunkPool.Create(transform);

            ChunkMesher.Build(view, layout, profile, theme, owned, sealedEdges);
            loaded[coord] = view;
            chunkLoaded.Invoke(view);
        }

        void Unload(Vector2Int coord)
        {
            if (!loaded.TryGetValue(coord, out var view)) return;

            loaded.Remove(coord);
            chunkUnloaded.Invoke(view);
            pool.Release(view);
        }

        Transform ResolveTarget()
        {
            if (target != null) return target;
            var cam = Camera.main;
            return cam != null ? cam.transform : transform;
        }

        bool HasAssets()
        {
            if (profile != null && theme != null) return true;
            Debug.LogWarning("EasyBackrooms: drag a profile and a theme onto the Backrooms World first.", this);
            return false;
        }
    }
}
