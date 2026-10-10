using UnityEngine;
using VRPedestrianSimulator.Audio;
using VRPedestrianSimulator.Player;
using VRPedestrianSimulator.Simulation;

namespace VRPedestrianSimulator.Traffic
{
    /// <summary>Kinematic vehicle that drives along X, counts as "passed" after the crossing and reports contact with the player.</summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class DemoVehicle : MonoBehaviour
    {
        private float speed;
        private float finishX;
        private bool counted;
        private ShowcaseSession session;
        private SpatialVehicleAudio engineAudio;
        private Rigidbody body;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            engineAudio = GetComponent<SpatialVehicleAudio>();
        }

        public void Setup(float speedMetersPerSecond, float endX, ShowcaseSession owner)
        {
            speed = speedMetersPerSecond;
            finishX = endX;
            session = owner;
        }

        private void FixedUpdate()
        {
            body.MovePosition(body.position + transform.right * speed * Time.fixedDeltaTime);
            if (engineAudio == null) engineAudio = GetComponent<SpatialVehicleAudio>();
            if (engineAudio != null) engineAudio.SetVehicleSpeed(speed);
            if (!counted && transform.position.x > 2.7f)
            {
                counted = true;
                if (session != null) session.RegisterPassingVehicle();
            }
            if (transform.position.x >= finishX) Destroy(gameObject);
        }

        // Vehicle body has a trigger volume so contact is easy to catch.
        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<PedestrianFirstPersonController>() != null && session != null)
                session.RegisterCollision();
        }
    }
}
