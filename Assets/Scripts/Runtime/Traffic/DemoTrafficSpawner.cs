using UnityEngine;
using VRPedestrianSimulator.Audio;
using VRPedestrianSimulator.Simulation;

namespace VRPedestrianSimulator.Traffic
{
    public sealed class DemoTrafficSpawner : MonoBehaviour
    {
        [SerializeField] private AudioClip engineLoop;
        [SerializeField] private float carSpeed = 5.0f;
        [SerializeField] private float spawnInterval = 5.0f;
        private float timer;
        private int nextLane;
        private ShowcaseSession session;
        private Transform vehiclesRoot;

        public float CurrentCarSpeed => carSpeed;

        public void Configure(ShowcaseSession owner, AudioClip engine)
        {
            session = owner;
            engineLoop = engine;
        }

        public void SetDifficulty(int level)
        {
            // Level 1: slow, big gaps. Level 2: faster, smaller gaps. Level 3: fastest, smallest gaps.
            carSpeed = level == 1 ? 5f : level == 2 ? 8f : 11f;
            spawnInterval = level == 1 ? 5.0f : level == 2 ? 3.2f : 2.0f;
            timer = spawnInterval * 0.55f;
        }

        public void ClearVehicles()
        {
            if (vehiclesRoot != null) Destroy(vehiclesRoot.gameObject);
            vehiclesRoot = new GameObject("Generated vehicles (runtime)").transform;
            vehiclesRoot.SetParent(transform, false);
        }

        private void Start() { ClearVehicles(); }

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer < spawnInterval) return;
            timer = 0f;
            SpawnCar();
        }

        private void SpawnCar()
        {
            float z = (nextLane++ % 2 == 0) ? -1.8f : 1.8f;
            GameObject car = GameObject.CreatePrimitive(PrimitiveType.Cube);
            car.name = "Traffic car";
            car.transform.SetParent(vehiclesRoot);
            car.transform.position = new Vector3(-19f, 0.68f, z);
            car.transform.localScale = new Vector3(2.8f, 1.25f, 1.4f);
            BoxCollider trigger = car.GetComponent<BoxCollider>();
            trigger.isTrigger = true;
            car.AddComponent<Rigidbody>();
            var vehicle = car.AddComponent<DemoVehicle>();
            vehicle.Setup(carSpeed, 20f, session);
            var sound = car.AddComponent<SpatialVehicleAudio>();
            sound.Configure(engineLoop);
            Renderer renderer = car.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color paint = nextLane % 3 == 0 ? new Color(0.30f, 0.58f, 0.76f)
                            : nextLane % 3 == 1 ? new Color(0.88f, 0.65f, 0.23f)
                            : new Color(0.79f, 0.32f, 0.30f);
                renderer.material.color = paint;
            }
        }
    }
}
