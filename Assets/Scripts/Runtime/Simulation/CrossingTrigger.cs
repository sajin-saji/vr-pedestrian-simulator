using UnityEngine;
using VRPedestrianSimulator.Player;

namespace VRPedestrianSimulator.Simulation
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class CrossingTrigger : MonoBehaviour
    {
        public bool isExit;
        public ShowcaseSession session;

        private void OnTriggerEnter(Collider other)
        {
            if (session == null || other.GetComponentInParent<PedestrianFirstPersonController>() == null) return;
            if (isExit) session.FinishCrossing();
            else session.BeginCrossing();
        }
    }
}
