#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using VRPedestrianSimulator.Audio;
using VRPedestrianSimulator.Player;
using VRPedestrianSimulator.Simulation;
using VRPedestrianSimulator.Traffic;

namespace VRPedestrianSimulator.Editor
{
    /// <summary>
    /// Editor menu that builds the desktop crossing scene: road, crosswalk,
    /// buildings, trees, player, traffic, audio and crossing triggers.
    /// </summary>
    public static class PedestrianShowcaseBuilder
    {
        private const string DemoScenePath = "Assets/Scenes/PedestrianShowcase.unity";
        private const string MaterialsPath = "Assets/Materials/Demo";

        [MenuItem("Tools/Pedestrian Simulator/Generate Desktop Showcase Scene")]
        public static void Generate()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            System.IO.Directory.CreateDirectory("Assets/Scenes");
            System.IO.Directory.CreateDirectory(MaterialsPath);
            AssetDatabase.Refresh();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Material grass = MaterialFor("Green ground", new Color(0.26f, 0.44f, 0.31f));
            Material asphalt = MaterialFor("Dark asphalt", new Color(0.20f, 0.23f, 0.26f));
            Material sidewalk = MaterialFor("Concrete walkway", new Color(0.56f, 0.58f, 0.57f));
            Material white = MaterialFor("Road markings", new Color(0.93f, 0.92f, 0.87f));
            Material yellow = MaterialFor("Sign yellow", new Color(0.91f, 0.69f, 0.22f));
            Material windows = MaterialFor("Glass blue", new Color(0.31f, 0.57f, 0.65f));
            Material trunk = MaterialFor("Tree bark", new Color(0.29f, 0.22f, 0.15f));
            Material leaves = MaterialFor("Tree foliage", new Color(0.18f, 0.41f, 0.20f));

            GameObject environment = new GameObject("Environment - generated geometry");
            Block("Terrain", new Vector3(0,-0.35f,0), new Vector3(62,0.6f,42), grass, environment.transform);
            Block("Two-lane road", new Vector3(0,-0.025f,0), new Vector3(60,0.10f,8.4f), asphalt, environment.transform);
            Block("Near sidewalk", new Vector3(0,0.06f,-6.4f), new Vector3(60,0.17f,3.7f), sidewalk, environment.transform);
            Block("Far sidewalk", new Vector3(0,0.06f,6.4f), new Vector3(60,0.17f,3.7f), sidewalk, environment.transform);
            Block("Median divider stripe", new Vector3(0,0.035f,0), new Vector3(59,0.012f,.10f), yellow, environment.transform);

            // Crossing is along Z; stripe columns run across the eight-meter road.
            for (int i=-3; i<=3; i++)
                Block("Zebra crossing stripe", new Vector3(i*0.78f,0.045f,0), new Vector3(.46f,.012f,8.05f), white, environment.transform);

            for (int side=-1;side<=1;side+=2)
            {
                for (int i=0;i<4;i++)
                {
                    float x=-23f+i*15f;
                    float z=side*13.7f;
                    float height=4.6f+i*.8f;
                    Block("Street building",new Vector3(x,height/2f,z),new Vector3(9,height,6),
                        i%2==0?sidewalk:white,environment.transform);
                    for(int level=1; level<=3; level++)
                        for(int win=0; win<3;win++)
                            Block("Building window",new Vector3(x-3.0f+3f*win,1.1f*level,z-side*3.04f),
                                new Vector3(1.35f,.65f,.025f),windows,environment.transform);
                }
                for (int i=0; i<5; i++)
                {
                    float x = -26 + i*13;
                    float z = side*9.5f;
                    Block("Tree trunk",new Vector3(x,1.1f,z),new Vector3(.30f,2.2f,.30f),trunk,environment.transform);
                    Sphere("Tree crown",new Vector3(x,3.2f,z),new Vector3(2.25f,2.7f,2.1f),leaves,environment.transform);
                }
            }

            GameObject sun = new GameObject("Sun - directional light");
            Light light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            sun.transform.rotation = Quaternion.Euler(40,-28,0);
            RenderSettings.ambientLight = new Color(.5f,.56f,.63f);

            GameObject cameraObject = new GameObject("Player Camera");
            cameraObject.tag = "MainCamera";
            Camera playerCamera = cameraObject.AddComponent<Camera>();
            playerCamera.nearClipPlane = .06f;
            playerCamera.fieldOfView = 76;
            cameraObject.AddComponent<AudioListener>();

            GameObject playerRoot = new GameObject("Player - desktop first person");
            playerRoot.tag = "Player";
            playerRoot.transform.position = new Vector3(0,.02f,-7.1f);
            CharacterController character = playerRoot.AddComponent<CharacterController>();
            character.height = 1.75f;
            character.radius = .32f;
            character.center = new Vector3(0,.875f,0);
            cameraObject.transform.SetParent(playerRoot.transform,false);
            cameraObject.transform.localPosition = new Vector3(0,1.58f,0);
            playerRoot.AddComponent<AudioSource>();
            PedestrianFootstepAudio feet = playerRoot.AddComponent<PedestrianFootstepAudio>();
            feet.Configure(new AudioClip[] {
                AudioAt("footstep_01.wav"),AudioAt("footstep_02.wav"),AudioAt("footstep_03.wav"),AudioAt("footstep_04.wav")});
            PedestrianFirstPersonController controls = playerRoot.AddComponent<PedestrianFirstPersonController>();
            controls.viewCamera = playerCamera;

            GameObject ambienceRoot = new GameObject("City soundscape");
            GameObject bed = new GameObject("City ambience - 2D");
            bed.transform.SetParent(ambienceRoot.transform);
            AudioSource bedSource = bed.AddComponent<AudioSource>();
            bedSource.clip = AudioAt("city_ambience.wav");
            bedSource.volume = .05f;
            GameObject crowds = new GameObject("Crowd murmur - 3D");
            crowds.transform.position = new Vector3(7,1.5f,7.5f);
            crowds.transform.SetParent(ambienceRoot.transform);
            AudioSource crowdSource = crowds.AddComponent<AudioSource>();
            crowdSource.clip = AudioAt("crowd_murmur.wav");
            crowdSource.volume = .02f;
            crowdSource.minDistance = 2;
            crowdSource.maxDistance = 20;
            var soundscape=ambienceRoot.AddComponent<EnvironmentSoundscape>();
            soundscape.Configure(bedSource,crowdSource);

            GameObject trafficObject = new GameObject("Traffic spawning and difficulty");
            DemoTrafficSpawner spawner = trafficObject.AddComponent<DemoTrafficSpawner>();
            GameObject sessionRoot = new GameObject("Crossing session / three levels");
            ShowcaseSession session = sessionRoot.AddComponent<ShowcaseSession>();
            session.player = controls;
            session.traffic = spawner;
            spawner.Configure(session,AudioAt("car_engine_loop.wav"));

            MakeZone("Crossing start trigger", new Vector3(0,1,-4.35f), new Vector3(5.9f,2.3f,.4f),false,session);
            MakeZone("Crossing finish trigger", new Vector3(0,1,5.15f), new Vector3(5.9f,2.3f,.5f),true,session);

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, DemoScenePath))
                throw new System.InvalidOperationException("Could not save: " + DemoScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Pedestrian showcase created",
                "Saved to Assets/Scenes/PedestrianShowcase.unity. Press Play to start.","OK");
        }

        private static GameObject Block(string label, Vector3 position, Vector3 scale, Material material, Transform parent)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name=label;
            obj.transform.SetParent(parent);
            obj.transform.position=position;
            obj.transform.localScale=scale;
            obj.GetComponent<Renderer>().sharedMaterial=material;
            return obj;
        }
        private static GameObject Sphere(string label, Vector3 position, Vector3 scale, Material material, Transform parent)
        {
            GameObject obj=GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.name=label;
            obj.transform.SetParent(parent);
            obj.transform.position=position;
            obj.transform.localScale=scale;
            obj.GetComponent<Renderer>().sharedMaterial=material;
            return obj;
        }
        private static Material MaterialFor(string name, Color color)
        {
            string path = MaterialsPath + "/" + name.Replace(" ", "_") + ".mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Standard");
                if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Unlit/Color");
                if (shader == null) throw new System.InvalidOperationException("Compatible material shader was not found");
                mat=new Material(shader) { name=name };
                AssetDatabase.CreateAsset(mat,path);
            }
            mat.color = color;
            EditorUtility.SetDirty(mat);
            return mat;
        }
        private static AudioClip AudioAt(string file)
        {
            return AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Demo/"+file);
        }
        private static void MakeZone(string label, Vector3 position, Vector3 scale, bool exit, ShowcaseSession session)
        {
            GameObject zone = new GameObject(label);
            zone.transform.position=position;
            BoxCollider collider = zone.AddComponent<BoxCollider>();
            collider.isTrigger=true;
            collider.size=scale;
            CrossingTrigger trigger = zone.AddComponent<CrossingTrigger>();
            trigger.isExit=exit;
            trigger.session=session;
        }
    }
}
#endif
