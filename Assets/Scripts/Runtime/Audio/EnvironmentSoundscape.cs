using UnityEngine;

namespace VRPedestrianSimulator.Audio
{
    /// <summary>
    /// Two-layer soundscape: a non-directional (2D) city bed and a
    /// spatial (3D) crowd source, both faded in to their target volume.
    /// </summary>
    public sealed class EnvironmentSoundscape : MonoBehaviour
    {
        [SerializeField] private AudioSource cityAmbience;
        [SerializeField] private AudioSource pedestrianCrowd;
        [SerializeField, Range(0f, 1f)] private float ambienceTarget = 0.20f;
        [SerializeField, Range(0f, 1f)] private float crowdTarget = 0.28f;
        [SerializeField, Min(0.1f)] private float fadeSpeed = 0.35f;

        public void Configure(AudioSource ambience, AudioSource crowd)
        {
            cityAmbience = ambience;
            pedestrianCrowd = crowd;
        }

        private void Start()
        {
            if (cityAmbience != null) { cityAmbience.loop = true; cityAmbience.spatialBlend = 0f; cityAmbience.Play(); }
            if (pedestrianCrowd != null) { pedestrianCrowd.loop = true; pedestrianCrowd.spatialBlend = 1f; pedestrianCrowd.Play(); }
        }

        private void Update()
        {
            if (cityAmbience != null)
                cityAmbience.volume = Mathf.MoveTowards(cityAmbience.volume, ambienceTarget, fadeSpeed * Time.deltaTime);
            if (pedestrianCrowd != null)
                pedestrianCrowd.volume = Mathf.MoveTowards(pedestrianCrowd.volume, crowdTarget, fadeSpeed * Time.deltaTime);
        }
    }
}
