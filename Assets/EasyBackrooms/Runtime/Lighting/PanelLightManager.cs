using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

namespace EasyBackrooms
{
    // Every visible panel gets a light
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
            public bool WantsShadow;
            public float ShadowFade;
        }

        [Tooltip("The world to light. Leave empty to find one in the scene.")]
        public BackroomsWorld world;

        [Tooltip("Lights gather around this. Leave empty to use the main camera.")]
        public Transform listener;

        [Header("Lights")]
        [Range(0, 256), Tooltip("Most real lights at once. Forward+ handles a lot.")]
        public int maxLights = 192;

        [Min(1f), Tooltip("Panels closer than this get a real light. Keep it inside the fog so you never see them switch.")]
        public float lightDistance = 28f;

        [Range(0.1f, 0.95f), Tooltip("Where lights start fading out, as a share of Light Distance.")]
        public float fadeStart = 0.65f;

        public Color lightColor = new Color(1f, 0.9f, 0.66f);

        [Min(0f)]
        public float intensity = 3f;

        [Min(0.1f), Tooltip("How far each light reaches in meters.")]
        public float range = 8f;

        [Range(1f, 179f), Tooltip("Width of the light cone. Real ceiling fixtures are very wide.")]
        public float spotAngle = 160f;

        [Range(0f, 179f), Tooltip("Width of the fully bright middle of the cone.")]
        public float innerSpotAngle = 70f;

        [Min(0f), Tooltip("Seconds a light takes to fade in.")]
        public float fadeTime = 0.6f;

        [Header("Shadows")]
        [Range(0, 32), Tooltip("How many of the nearest lights cast soft shadows. More looks better but costs more.")]
        public int shadowedLights = 12;

        [Range(0f, 1f)]
        public float shadowStrength = 0.85f;

        [Min(0.01f), Tooltip("Seconds shadows take to fade in or out.")]
        public float shadowFadeTime = 1f;

        [Header("Buzz")]
        [Tooltip("Drop your looping buzz sound here.")]
        public AudioClip buzz;

        [Range(0, 32), Tooltip("How many panels near you play the buzz.")]
        public int maxBuzzSources = 2;

        [Range(0f, 1f)]
        public float volume = 0.04f;

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
        [Min(0.02f), Tooltip("Seconds between picking which panels get lights.")]
        public float refreshInterval = 0.2f;

        readonly List<ChunkView> views = new List<ChunkView>();
        readonly List<Vector3> candidates = new List<Vector3>();
        readonly List<Vector3> pending = new List<Vector3>();
        readonly HashSet<Vector3> shadowSet = new HashSet<Vector3>();

        Slot[] lightSlots = Array.Empty<Slot>();
        Slot[] buzzSlots = Array.Empty<Slot>();
        Comparison<Vector3> byDistance;
        Vector3 center;
        float timer;
        float cutoff;
        float targetCutoff;

        void Awake()
        {
            byDistance = (a, b) => (a - center).sqrMagnitude.CompareTo((b - center).sqrMagnitude);
            cutoff = targetCutoff = lightDistance;
        }

        void OnDisable()
        {
            foreach (var slot in lightSlots)
            {
                if (slot.Light != null) slot.Light.enabled = false;
                slot.Used = false;
            }

            foreach (var slot in buzzSlots)
            {
                if (slot.Source != null) slot.Source.Stop();
                slot.Used = false;
            }

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
            float maxSqr = lightDistance * lightDistance;

            foreach (var view in views)
            {
                var points = view.LightPoints;
                for (int i = 0; i < points.Count; i++)
                {
                    var p = view.transform.TransformPoint(points[i]);
                    if ((p - center).sqrMagnitude <= maxSqr) candidates.Add(p);
                }
            }

            candidates.Sort(byDistance);

            // If we run out of lights fade sooner
            targetCutoff = candidates.Count > lightSlots.Length && lightSlots.Length > 0
                ? Vector3.Distance(center, candidates[lightSlots.Length - 1])
                : lightDistance;

            Assign(lightSlots);
            Assign(buzzSlots);

            // Shadows linger so they dont blink
            shadowSet.Clear();
            int keepShadow = shadowedLights + shadowedLights / 2;
            for (int i = 0; i < keepShadow && i < candidates.Count; i++) shadowSet.Add(candidates[i]);

            int shadowCount = 0;
            foreach (var slot in lightSlots)
            {
                bool keep = slot.Used && slot.WantsShadow && shadowSet.Contains(slot.Position);
                slot.WantsShadow = keep;
                if (keep) shadowCount++;
                if (slot.Used) slot.Light.transform.position = slot.Position;
            }

            for (int i = 0; i < shadowedLights && i < candidates.Count && shadowCount < shadowedLights; i++)
            {
                foreach (var slot in lightSlots)
                {
                    if (!slot.Used || slot.WantsShadow || slot.Position != candidates[i]) continue;
                    slot.WantsShadow = true;
                    shadowCount++;
                    break;
                }
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
                slot.ShadowFade = 0f;
                slot.WantsShadow = false;
            }
        }

        void Animate()
        {
            float t = Time.time;
            float dt = Time.deltaTime;
            float step = fadeTime > 0f ? dt / fadeTime : 1f;
            float shadowStep = dt / shadowFadeTime;
            var eye = ResolveListener().position;

            cutoff = Mathf.MoveTowards(cutoff, targetCutoff, dt * 8f);
            float fadeFrom = cutoff * fadeStart;

            foreach (var slot in lightSlots)
            {
                var light = slot.Light;
                if (!slot.Used)
                {
                    light.enabled = false;
                    continue;
                }

                slot.Fade = Mathf.MoveTowards(slot.Fade, 1f, step);
                slot.ShadowFade = Mathf.MoveTowards(slot.ShadowFade, slot.WantsShadow ? 1f : 0f, shadowStep);

                float distance = Vector3.Distance(eye, slot.Position);
                float distanceFade = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(fadeFrom, cutoff, distance));

                float level = flicker != null ? flicker.Evaluate(slot.Position, t) : 1f;
                bool low = level < 0.35f;
                if (low && !slot.WasLow) flickered.Invoke(slot.Position);
                slot.WasLow = low;

                light.enabled = distanceFade > 0.001f;
                light.color = lightColor;
                light.range = range;
                light.spotAngle = spotAngle;
                light.innerSpotAngle = Mathf.Min(innerSpotAngle, spotAngle);
                light.intensity = intensity * level * slot.Fade * distanceFade;

                if (slot.ShadowFade > 0.001f)
                {
                    light.shadows = LightShadows.Soft;
                    light.shadowStrength = shadowStrength * slot.ShadowFade;
                }
                else
                {
                    light.shadows = LightShadows.None;
                }
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
                    // Spot pointing straight down
                    go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                    slot.Light = go.AddComponent<Light>();
                    slot.Light.type = LightType.Spot;
                    slot.Light.shadowNearPlane = 0.1f;
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
