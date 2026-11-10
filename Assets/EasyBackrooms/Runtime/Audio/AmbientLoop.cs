using UnityEngine;
using UnityEngine.Audio;

namespace EasyBackrooms
{
    // Quiet room tone everywhere
    [AddComponentMenu("EasyBackrooms/Ambient Loop")]
    public sealed class AmbientLoop : MonoBehaviour
    {
        [Tooltip("A looping drone or room tone.")]
        public AudioClip clip;

        [Range(0f, 1f)] public float volume = 0.15f;

        [Min(0f), Tooltip("Seconds to fade in on start.")]
        public float fadeIn = 2f;

        public AudioMixerGroup mixerGroup;

        AudioSource source;

        void Start()
        {
            if (clip == null) return;

            source = gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.spatialBlend = 0f;
            source.volume = 0f;
            source.outputAudioMixerGroup = mixerGroup;
            source.Play();
        }

        void Update()
        {
            if (source == null) return;
            float step = fadeIn > 0f ? Time.deltaTime / fadeIn * volume : volume;
            source.volume = Mathf.MoveTowards(source.volume, volume, step);
        }
    }
}
