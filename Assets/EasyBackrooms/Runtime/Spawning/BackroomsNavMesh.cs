using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace EasyBackrooms
{
    // Keeps a NavMesh around the player
    [AddComponentMenu("EasyBackrooms/Backrooms NavMesh")]
    public sealed class BackroomsNavMesh : MonoBehaviour
    {
        [Tooltip("Leave empty to use the world on this object or in the scene.")]
        public BackroomsWorld world;

        [Tooltip("The area follows this. Leave empty to use the main camera.")]
        public Transform center;

        [Tooltip("Size of the walkable area kept around the center. Keep Y under the wall height so the ceiling is left out.")]
        public Vector3 size = new Vector3(80f, 2.5f, 80f);

        [Tooltip("Agent type from the Navigation window. 0 is Humanoid.")]
        public int agentTypeId;

        public LayerMask layers = ~0;

        [Min(0.1f), Tooltip("Seconds between updates.")]
        public float updateInterval = 1f;

        readonly List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
        readonly List<NavMeshBuildMarkup> markups = new List<NavMeshBuildMarkup>();

        NavMeshData data;
        NavMeshDataInstance instance;
        AsyncOperation running;
        Vector3 lastCenter;
        bool dirty = true;
        float timer;

        void OnEnable()
        {
            if (world == null) world = GetComponent<BackroomsWorld>();
            if (world == null) world = FindFirstObjectByType<BackroomsWorld>();

            data = new NavMeshData(agentTypeId);
            instance = NavMesh.AddNavMeshData(data);

            if (world != null)
            {
                world.chunkLoaded.AddListener(MarkDirty);
                world.chunkUnloaded.AddListener(MarkDirty);
            }
        }

        void OnDisable()
        {
            instance.Remove();
            if (world == null) return;
            world.chunkLoaded.RemoveListener(MarkDirty);
            world.chunkUnloaded.RemoveListener(MarkDirty);
        }

        void MarkDirty(ChunkView _) => dirty = true;

        void Update()
        {
            timer -= Time.deltaTime;
            if (timer > 0f || (running != null && !running.isDone)) return;
            timer = updateInterval;

            var focus = Focus();
            if (!dirty && (focus - lastCenter).sqrMagnitude < size.x * size.x * 0.0625f) return;

            // Snap so small steps dont rebuild
            float snap = world != null && world.profile != null ? world.profile.ChunkWorldSize * 0.5f : 16f;
            var middle = new Vector3(Mathf.Round(focus.x / snap) * snap, focus.y + size.y * 0.5f, Mathf.Round(focus.z / snap) * snap);
            var bounds = new Bounds(middle, size);

            NavMeshBuilder.CollectSources(bounds, layers, NavMeshCollectGeometry.PhysicsColliders, 0, markups, sources);
            running = NavMeshBuilder.UpdateNavMeshDataAsync(data, NavMesh.GetSettingsByID(agentTypeId), sources, bounds);

            lastCenter = focus;
            dirty = false;
        }

        Vector3 Focus()
        {
            if (center != null) return center.position;
            var cam = Camera.main;
            if (cam == null) return transform.position;

            // Use the floor under the camera
            var p = cam.transform.position;
            return new Vector3(p.x, world != null ? world.transform.position.y : 0f, p.z);
        }
    }
}
