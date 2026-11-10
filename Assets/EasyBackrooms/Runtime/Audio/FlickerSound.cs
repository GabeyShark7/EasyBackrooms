using UnityEngine;
using UnityEngine.Audio;

namespace EasyBackrooms
{
    // Crackle where a light flickers
    [AddComponentMenu("EasyBackrooms/Flicker Sound")]
    public sealed class FlickerSound : MonoBehaviour
    {
        [Tooltip("Leave empty to find the Backrooms Lights in the scene.")]
        public PanelLightManager lights;

        [Tooltip("Short buzz or crackle sounds.")]
        public AudioClip[] clips;

        [Range(0f, 1f)] public float volume = 0.25f;
        [Range(0f, 0.5f)] public float pitchJitter = 0.1f;

        [Min(0.2f)] public float maxDistance = 10f;

        [Range(1, 16), Tooltip("Most crackles playing at once.")]
        public int voices = 4;

        [Min(0f), Tooltip("Shortest gap between crackles in seconds.")]
        public float cooldown = 0.12f;

        public AudioMixerGroup mixerGroup;

        AudioSource[] pool;
        int next;
        float lastTime = -10f;

        void Awake()
        {
            pool = new AudioSource[voices];
            for (int i = 0; i < voices; i++)
            {
                var go = new GameObject($"Flicker Voice {i}");
                go.transform.SetParent(transform, false);
                var s = go.AddComponent<AudioSource>();
                s.playOnAwake = false;
                s.spatialBlend = 1f;
                s.dopplerLevel = 0f;
                s.rolloffMode = AudioRolloffMode.Linear;
                s.minDistance = 0.5f;
                pool[i] = s;
            }
        }

        void OnEnable()
        {
            if (lights == null) lights = FindFirstObjectByType<PanelLightManager>();
            if (lights != null) lights.flickered.AddListener(Play);
        }

        void OnDisable()
        {
            if (lights != null) lights.flickered.RemoveListener(Play);
        }

        public void Play(Vector3 position)
        {
            if (clips == null || clips.Length == 0 || pool.Length == 0) return;
            if (Time.time - lastTime < cooldown) return;
            lastTime = Time.time;

            var clip = clips[Random.Range(0, clips.Length)];
            if (clip == null) return;

            var s = pool[next++ % pool.Length];
            s.transform.position = position;
            s.maxDistance = maxDistance;
            s.outputAudioMixerGroup = mixerGroup;
            s.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
            s.PlayOneShot(clip, volume);
        }
    }
}
