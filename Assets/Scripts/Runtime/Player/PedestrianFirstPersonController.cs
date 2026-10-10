using UnityEngine;
using VRPedestrianSimulator.Audio;
using VRPedestrianSimulator.Simulation;

namespace VRPedestrianSimulator.Player
{
    /// <summary>First-person keyboard and mouse control with footstep hook.</summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class PedestrianFirstPersonController : MonoBehaviour
    {
        public Camera viewCamera;
        [SerializeField] private float walkingSpeed = 2.3f;
        [SerializeField] private float runningSpeed = 4.0f;
        [SerializeField] private float mouseSensitivity = 1.8f;
        [SerializeField] private float gravity = 16f;
        private CharacterController character;
        private PedestrianFootstepAudio footsteps;
        private float verticalVelocity;
        private float pitch;

        private void Awake()
        {
            character = GetComponent<CharacterController>();
            footsteps = GetComponent<PedestrianFootstepAudio>();
            if (viewCamera == null) viewCamera = GetComponentInChildren<Camera>();
        }

        private void Start() { SetCursorLocked(true); }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) SetCursorLocked(false);
            if (Input.GetMouseButtonDown(0)) SetCursorLocked(true);
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                transform.Rotate(0f, Input.GetAxis("Mouse X") * mouseSensitivity, 0f);
                pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * mouseSensitivity, -76f, 76f);
                if (viewCamera != null) viewCamera.transform.localEulerAngles = new Vector3(pitch, 0f, 0f);
            }

            Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            input = Vector3.ClampMagnitude(input, 1f);
            bool running = Input.GetKey(KeyCode.LeftShift);
            Vector3 lateral = transform.TransformDirection(input) * (running ? runningSpeed : walkingSpeed);
            if (character.isGrounded && verticalVelocity < 0f) verticalVelocity = -1f;
            verticalVelocity -= gravity * Time.deltaTime;
            Vector3 before = transform.position;
            character.Move((lateral + Vector3.up * verticalVelocity) * Time.deltaTime);

            Vector3 delta = transform.position - before;
            delta.y = 0f;
            if (footsteps != null) footsteps.Advance(delta.magnitude, running, character.isGrounded);
        }

        public void ResetTo(Vector3 worldPosition)
        {
            character.enabled = false;
            transform.position = worldPosition;
            transform.rotation = Quaternion.identity;
            character.enabled = true;
            verticalVelocity = 0f;
            pitch = 0f;
            if (footsteps != null) footsteps.ResetStride();
        }

        public void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
