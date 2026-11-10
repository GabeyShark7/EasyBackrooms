using UnityEngine;
using UnityEngine.Audio;

namespace EasyBackrooms
{
    // Drop your carpet step sounds in here
    [AddComponentMenu("EasyBackrooms/Footstep Audio")]
    public sealed class FootstepAudio : MonoBehaviour
    {
        [Tooltip("Leave empty to use the player on this object or a parent.")]
        public FirstPersonPlayer player;

        [Tooltip("A few short step sounds. One is picked at random each step.")]
        public AudioClip[] clips;

        [Range(0f, 2f), Tooltip("Above 1 boosts quiet recordings.")]
        public float volume = 1f;
        [Range(0f, 0.5f)] public float pitchJitter = 0.08f;
        [Range(0f, 0.5f)] public float volumeJitter = 0.15f;

        [Min(0f)] public float sprintVolume = 1.3f;
        [Min(0f)] public float crouchVolume = 0.5f;

        public AudioMixerGroup mixerGroup;

        AudioSource source;
        int last = -1;

        void Awake()
        {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
        }

        void OnEnable()
        {
            if (player == null) player = GetComponentInParent<FirstPersonPlayer>();
            if (player != null) player.stepped.AddListener(Play);
        }

        void OnDisable()
        {
            if (player != null) player.stepped.RemoveListener(Play);
        }

        public void Play()
        {
            if (clips == null || clips.Length == 0) return;

            int i = Random.Range(0, clips.Length);
            if (clips.Length > 1 && i == last) i = (i + 1) % clips.Length;
            last = i;
            if (clips[i] == null) return;

            float v = volume * (1f + Random.Range(-volumeJitter, volumeJitter));
            if (player != null && player.IsSprinting) v *= sprintVolume;
            if (player != null && player.IsCrouching) v *= crouchVolume;

            source.outputAudioMixerGroup = mixerGroup;
            source.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
            source.volume = 1f;
            source.PlayOneShot(clips[i], Mathf.Max(0f, v));
        }
    }
}
