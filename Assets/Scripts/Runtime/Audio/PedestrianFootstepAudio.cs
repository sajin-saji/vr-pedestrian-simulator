using UnityEngine;

namespace VRPedestrianSimulator.Audio
{
    /// <summary>
    /// Footstep sounds driven by the distance the player actually walks,
    /// with a longer stride when running and no direct clip repeats.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class PedestrianFootstepAudio : MonoBehaviour
    {
        [Header("Demo Audio Clips")]
        [SerializeField] private AudioClip[] footstepClips;
        [SerializeField, Min(0.1f)] private float walkingStepDistance = 1.55f;
        [SerializeField, Min(0.1f)] private float runningStepDistance = 2.05f;
        [SerializeField, Range(0f, 1f)] private float volume = 0.65f;
        private AudioSource source;
        private float travelledDistance;
        private int previousClip = -1;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f; // Self-footsteps remain clear for the local player.
            source.loop = false;
        }

        public void Configure(AudioClip[] clips) { footstepClips = clips; }

        // Call once per frame with the actual horizontal displacement of the player.
        public void Advance(float horizontalDistance, bool running, bool grounded)
        {
            if (!grounded || horizontalDistance <= 0f || footstepClips == null || footstepClips.Length == 0)
                return;

            travelledDistance += horizontalDistance;
            float interval = running ? runningStepDistance : walkingStepDistance;
            if (travelledDistance < interval) return;
            travelledDistance -= interval;

            int index = Random.Range(0, footstepClips.Length);
            if (footstepClips.Length > 1 && index == previousClip)
                index = (index + 1) % footstepClips.Length;
            previousClip = index;
            if (footstepClips[index] == null) return;

            source.pitch = Random.Range(0.94f, 1.06f);
            source.PlayOneShot(footstepClips[index], volume);
        }

        public void ResetStride() { travelledDistance = 0f; }
    }
}
