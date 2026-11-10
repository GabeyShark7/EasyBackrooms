using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyBackrooms
{
    public enum PropPlacement
    {
        CellCenter,
        AgainstWall
    }

    [Serializable]
    public sealed class PropRule
    {
        public string name = "Prop";

        [Tooltip("Your prefab. Its pivot should sit on the floor.")]
        public GameObject prefab;

        [Range(0f, 1f), Tooltip("Chance per open cell.")]
        public float chance = 0.02f;

        public PropPlacement placement = PropPlacement.AgainstWall;

        [Min(0f), Tooltip("Gap between the prop and the wall line in meters.")]
        public float wallGap = 0.15f;

        [Tooltip("Height above the floor in meters. Use this for outlets and signs.")]
        public float height = 0f;

        [Tooltip("Spin center props to a random angle.")]
        public bool randomYaw = true;

        [Tooltip("Only spawn inside open rooms and corridors.")]
        public bool onlyInRooms;

        [Min(0)] public int maxPerChunk = 4;
    }

    // Spawns your prefabs as chunks load
    [AddComponentMenu("EasyBackrooms/Prop Spawner")]
    public sealed class PropSpawner : MonoBehaviour
    {
        [Tooltip("Leave empty to use the world on this object or in the scene.")]
        public BackroomsWorld world;

        public List<PropRule> rules = new List<PropRule>();

        readonly Dictionary<ChunkView, List<GameObject>> spawned = new Dictionary<ChunkView, List<GameObject>>();

        static readonly Side[] Sides = { Side.West, Side.East, Side.South, Side.North };

        void OnEnable()
        {
            if (world == null) world = GetComponent<BackroomsWorld>();
            if (world == null) world = FindFirstObjectByType<BackroomsWorld>();
            if (world == null) return;

            world.chunkLoaded.AddListener(Spawn);
            world.chunkUnloaded.AddListener(Despawn);

            // Fixed worlds may be built already
            foreach (var view in world.GetComponentsInChildren<ChunkView>()) Spawn(view);
        }

        void OnDisable()
        {
            if (world == null) return;
            world.chunkLoaded.RemoveListener(Spawn);
            world.chunkUnloaded.RemoveListener(Despawn);

            foreach (var view in new List<ChunkView>(spawned.Keys)) Despawn(view);
        }

        public void Spawn(ChunkView view)
        {
            if (view == null || world == null || world.profile == null) return;
            Despawn(view);

            var layout = LayoutGenerator.Generate(world.profile, view.Coord);
            float c = world.profile.cellSize;
            var list = new List<GameObject>();

            for (int r = 0; r < rules.Count; r++)
            {
                var rule = rules[r];
                if (rule.prefab == null) continue;

                var rng = new DetRandom(Hash.Of(world.profile.seed, view.Coord.x, view.Coord.y, 1000 + r));
                int count = 0;

                for (int y = 0; y < layout.Size && count < rule.maxPerChunk; y++)
                {
                    for (int x = 0; x < layout.Size && count < rule.maxPerChunk; x++)
                    {
                        if (layout.GetCell(x, y) != CellKind.Open) continue;
                        if (rule.onlyInRooms && !layout.IsReserved(x, y)) continue;
                        if (!rng.Chance(rule.chance)) continue;

                        var center = new Vector3((x + 0.5f) * c, rule.height, (y + 0.5f) * c);
                        Vector3 position;
                        Quaternion rotation;

                        if (rule.placement == PropPlacement.CellCenter)
                        {
                            position = center;
                            rotation = Quaternion.Euler(0f, rule.randomYaw ? rng.NextFloat() * 360f : 0f, 0f);
                        }
                        else
                        {
                            if (!PickWall(layout, x, y, ref rng, out var dir)) continue;
                            position = center + dir * (c * 0.5f - rule.wallGap);
                            rotation = Quaternion.LookRotation(-dir, Vector3.up);
                        }

                        var go = Instantiate(rule.prefab, view.transform);
                        go.transform.localPosition = position;
                        go.transform.localRotation = rotation;
                        list.Add(go);
                        count++;
                    }
                }
            }

            spawned[view] = list;
        }

        public void Despawn(ChunkView view)
        {
            if (view == null || !spawned.TryGetValue(view, out var list)) return;
            foreach (var go in list) if (go != null) Destroy(go);
            spawned.Remove(view);
        }

        static bool PickWall(ChunkLayout layout, int x, int y, ref DetRandom rng, out Vector3 dir)
        {
            int found = 0;
            Side chosen = Side.West;

            foreach (var side in Sides)
            {
                if (!layout.HasWall(x, y, side)) continue;
                found++;
                if (rng.Range(0, found) == 0) chosen = side;
            }

            dir = chosen switch
            {
                Side.West => Vector3.left,
                Side.East => Vector3.right,
                Side.South => Vector3.back,
                _ => Vector3.forward
            };
            return found > 0;
        }
    }
}
