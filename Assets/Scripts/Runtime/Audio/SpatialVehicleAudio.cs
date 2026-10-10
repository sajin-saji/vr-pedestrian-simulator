using UnityEngine;

namespace VRPedestrianSimulator.Audio
{
    /// <summary>
    /// 3D engine sound for a vehicle: Doppler, logarithmic rolloff,
    /// pitch and volume rise with speed.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class SpatialVehicleAudio : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float referenceSpeedMetersPerSecond = 12f;
        [SerializeField, Range(0f, 1f)] private float baseVolume = 0.12f;
        [SerializeField, Range(0f, 1f)] private float movingVolume = 0.40f;
        private AudioSource source;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.spatialBlend = 1f;
            source.dopplerLevel = 1f;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.minDistance = 1.5f;
            source.maxDistance = 45f;
            source.loop = true;
            source.playOnAwake = false;
        }

        public void Configure(AudioClip clip)
        {
            if (source == null) source = GetComponent<AudioSource>();
            source.clip = clip;
            if (clip != null && Application.isPlaying) source.Play();
        }

        public void SetVehicleSpeed(float metersPerSecond)
        {
            if (source == null) return;
            float fraction = Mathf.Clamp01(metersPerSecond / referenceSpeedMetersPerSecond);
            source.pitch = Mathf.Lerp(0.78f, 1.32f, fraction);
            source.volume = Mathf.Lerp(baseVolume, movingVolume, fraction);
        }
    }
}
