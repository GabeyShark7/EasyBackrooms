using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

namespace EasyBackrooms
{
    // Moves a few real lights near you
    [AddComponentMenu("EasyBackrooms/Backrooms Lights")]
    public sealed class PanelLightManager : MonoBehaviour
    {
        [Serializable]
        public sealed class PointEvent : UnityEvent<Vector3> { }

        sealed class Slot
        {
            public Light Light;
            public AudioSource Source;
            public Vector3 Position;
            public bool Used;
            public float Fade;
            public bool WasLow;
        }

        [Tooltip("The world to light. Leave empty to find one in the scene.")]
        public BackroomsWorld world;

        [Tooltip("Lights gather around this. Leave empty to use the main camera.")]
        public Transform listener;

        [Header("Lights")]
        [Range(0, 64), Tooltip("How many real lights exist at once. Every other panel only glows.")]
        public int maxLights = 16;

        public Color lightColor = new Color(1f, 0.9f, 0.62f);

        [Min(0f)]
        public float intensity = 3.5f;

        [Min(0.1f), Tooltip("How far each light reaches in meters.")]
        public float range = 7f;

        public LightShadows shadows = LightShadows.None;

        [Min(0f), Tooltip("Seconds a light takes to fade in so they dont pop.")]
        public float fadeTime = 0.4f;

        [Header("Buzz")]
        [Tooltip("Drop your looping buzz sound here.")]
        public AudioClip buzz;

        [Range(0, 32), Tooltip("How many panels near you play the buzz.")]
        public int maxBuzzSources = 4;

        [Range(0f, 1f)]
        public float volume = 0.08f;

        [Min(0.1f)]
        public float minDistance = 0.5f;

        [Min(0.2f)]
        public float maxDistance = 6f;

        [Tooltip("Optional mixer group for the buzz.")]
        public AudioMixerGroup mixerGroup;

        [Header("Flicker")]
        [Tooltip("Decides how lights flicker. Leave empty for steady lights.")]
        public FlickerPattern flicker;

        [Tooltip("Fires when a light dips during a flicker. Hook a sound up here.")]
        public PointEvent flickered = new PointEvent();

        [Header("Performance")]
        [Min(0.02f), Tooltip("Seconds between picking the nearest panels.")]
        public float refreshInterval = 0.2f;

        readonly List<ChunkView> views = new List<ChunkView>();
        readonly List<Vector3> candidates = new List<Vector3>();
        readonly List<Vector3> pending = new List<Vector3>();

        Slot[] lightSlots = Array.Empty<Slot>();
        Slot[] buzzSlots = Array.Empty<Slot>();
        Comparison<Vector3> byDistance;
        Vector3 center;
        float timer;

        void Awake()
        {
            byDistance = (a, b) => (a - center).sqrMagnitude.CompareTo((b - center).sqrMagnitude);
        }

        void OnDisable()
        {
            foreach (var slot in lightSlots) if (slot.Light != null) slot.Light.enabled = false;
            foreach (var slot in buzzSlots) if (slot.Source != null) slot.Source.Stop();
            foreach (var slot in lightSlots) slot.Used = false;
            foreach (var slot in buzzSlots) slot.Used = false;
            timer = 0f;
        }

        void Update()
        {
            if (world == null) world = FindFirstObjectByType<BackroomsWorld>();
            if (world == null) return;

            EnsureSlots();

            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                timer = refreshInterval;
                Refresh();
            }

            Animate();
        }

        void Refresh()
        {
            center = ResolveListener().position;

            candidates.Clear();
            world.GetComponentsInChildren(false, views);
            foreach (var view in views)
            {
                var points = view.LightPoints;
                for (int i = 0; i < points.Count; i++) candidates.Add(view.transform.TransformPoint(points[i]));
            }

            candidates.Sort(byDistance);

            Assign(lightSlots);
            Assign(buzzSlots);

            foreach (var slot in lightSlots)
            {
                if (slot.Used) slot.Light.transform.position = slot.Position;
            }

            foreach (var slot in buzzSlots)
            {
                if (!slot.Used)
                {
                    if (slot.Source.isPlaying) slot.Source.Stop();
                    continue;
                }

                slot.Source.transform.position = slot.Position;
                if (!slot.Source.isPlaying && buzz != null)
                {
                    slot.Source.Play();
                    slot.Source.time = UnityEngine.Random.value * buzz.length * 0.99f;
                }
            }
        }

        // Keeps lights on the same panel
        void Assign(Slot[] slots)
        {
            int count = Mathf.Min(slots.Length, candidates.Count);
            pending.Clear();
            for (int i = 0; i < count; i++) pending.Add(candidates[i]);

            foreach (var slot in slots)
            {
                if (slot.Used && pending.Remove(slot.Position)) continue;
                slot.Used = false;
            }

            int next = 0;
            foreach (var slot in slots)
            {
                if (slot.Used || next >= pending.Count) continue;
                slot.Position = pending[next++];
                slot.Used = true;
                slot.Fade = 0f;
                slot.WasLow = false;
            }
        }

        void Animate()
        {
            float t = Time.time;
            float step = fadeTime > 0f ? Time.deltaTime / fadeTime : 1f;

            foreach (var slot in lightSlots)
            {
                var light = slot.Light;
                if (!slot.Used)
                {
                    light.enabled = false;
                    continue;
                }

                slot.Fade = Mathf.MoveTowards(slot.Fade, 1f, step);

                float level = flicker != null ? flicker.Evaluate(slot.Position, t) : 1f;
                bool low = level < 0.35f;
                if (low && !slot.WasLow) flickered.Invoke(slot.Position);
                slot.WasLow = low;

                light.enabled = true;
                light.color = lightColor;
                light.range = range;
                light.shadows = shadows;
                light.intensity = intensity * level * slot.Fade;
            }

            foreach (var slot in buzzSlots)
            {
                var source = slot.Source;
                source.volume = volume;
                source.minDistance = minDistance;
                source.maxDistance = Mathf.Max(minDistance + 0.1f, maxDistance);
                source.outputAudioMixerGroup = mixerGroup;
            }
        }

        void EnsureSlots()
        {
            if (lightSlots.Length != maxLights) lightSlots = Rebuild(lightSlots, maxLights, true);

            int buzzCount = buzz != null ? maxBuzzSources : 0;
            if (buzzSlots.Length != buzzCount) buzzSlots = Rebuild(buzzSlots, buzzCount, false);
        }

        Slot[] Rebuild(Slot[] old, int count, bool lights)
        {
            foreach (var slot in old)
            {
                var item = lights ? (Component)slot.Light : slot.Source;
                if (item != null) Destroy(item.gameObject);
            }

            var slots = new Slot[count];
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject(lights ? $"Panel Light {i}" : $"Panel Buzz {i}");
                go.transform.SetParent(transform, false);
                var slot = new Slot();

                if (lights)
                {
                    slot.Light = go.AddComponent<Light>();
                    slot.Light.type = LightType.Point;
                    slot.Light.enabled = false;
                }
                else
                {
                    var source = go.AddComponent<AudioSource>();
                    source.clip = buzz;
                    source.loop = true;
                    source.playOnAwake = false;
                    source.spatialBlend = 1f;
                    source.dopplerLevel = 0f;
                    source.rolloffMode = AudioRolloffMode.Linear;
                    slot.Source = source;
                }

                slots[i] = slot;
            }

            timer = 0f;
            return slots;
        }

        Transform ResolveListener()
        {
            if (listener != null) return listener;
            var cam = Camera.main;
            return cam != null ? cam.transform : transform;
        }
    }
}
