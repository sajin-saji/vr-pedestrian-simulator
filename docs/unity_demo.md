# Desktop demo: setup and test checklist

A keyboard-and-mouse version of the crossing simulator in plain Unity and C#. It
needs no headset and no third-party assets. The original project's Meta Quest
integration is described in the [project report](VR_Pedestrian_Simulator_Report.pdf).

## Setup

1. Install **Unity Hub** and **Unity 2022.3 LTS** (the project file names `2022.3.20f1`;
   any 2022.3 version works). Use the default **built-in render pipeline**.
2. In Unity Hub choose **Add project from disk** and select the repository folder
   (the one containing `Assets`, `Packages` and `ProjectSettings`).
3. Wait for Unity to import the scripts.
4. In the top menu choose **Tools → Pedestrian Simulator → Generate Desktop Showcase Scene**.
5. The scene opens at `Assets/Scenes/PedestrianShowcase.unity`. Press **Play**.

If input does not work, set **Edit → Project Settings → Player → Active Input
Handling** to **Input Manager (Old)** or **Both**.

## Controls

| Input | Action |
| --- | --- |
| WASD | Move |
| Mouse | Look around |
| Shift | Run |
| R | Restart |
| Space | Restart after finishing all levels |
| Escape | Release the mouse cursor |
| Click | Capture the mouse again |

## How it works

| Script | Role |
| --- | --- |
| [PedestrianShowcaseBuilder](../Assets/Scripts/Editor/PedestrianShowcaseBuilder.cs) | Builds the 8 m two-lane road, crosswalk, sidewalks, buildings, trees, light, player, audio, traffic and the start/finish triggers |
| [PedestrianFirstPersonController](../Assets/Scripts/Runtime/Player/PedestrianFirstPersonController.cs) | CharacterController movement, mouse look, passes walked distance to the footsteps |
| [DemoTrafficSpawner](../Assets/Scripts/Runtime/Traffic/DemoTrafficSpawner.cs) | Spawns cars in alternating lanes; level 1: 5 m/s every 5 s, level 2: 8 m/s every 3.2 s, level 3: 11 m/s every 2 s |
| [DemoVehicle](../Assets/Scripts/Runtime/Traffic/DemoVehicle.cs) | Kinematic rigidbody movement, "car passed" event, collision with the player |
| [ShowcaseSession](../Assets/Scripts/Runtime/Simulation/ShowcaseSession.cs) | Level, score (−10 per car passing during a crossing), Game Over, level-up and restart, on-screen HUD |
| [CrossingTrigger](../Assets/Scripts/Runtime/Simulation/CrossingTrigger.cs) | Start and finish zones of the crosswalk |
| [PedestrianFootstepAudio](../Assets/Scripts/Runtime/Audio/PedestrianFootstepAudio.cs) | One footstep per stride, longer stride when running, random pitch |
| [EnvironmentSoundscape](../Assets/Scripts/Runtime/Audio/EnvironmentSoundscape.cs) | 2D city ambience and 3D crowd sound with fade-in |
| [SpatialVehicleAudio](../Assets/Scripts/Runtime/Audio/SpatialVehicleAudio.cs) | 3D engine loop, Doppler 1, logarithmic rolloff; pitch and volume follow speed |

The sounds in `Assets/Audio/Demo/` are synthesised by `tools/generate_audio.py`:
pavement footsteps, a stereo city ambience loop (traffic, passing cars, wind, birds),
a crowd murmur and a 4-cylinder engine loop. See [the sound list](../Assets/Audio/Demo/README.md).

## Test checklist

Run through this after opening the project:

- [ ] Scene generates without errors in the Console
- [ ] Player walks, runs and looks around; footsteps play while moving
- [ ] Cars spawn in both lanes and their engine sound gets louder and higher as they approach
- [ ] Walking into a car ends the session with "Vehicle collision detected"
- [ ] Each car that passes during a crossing removes 10 points
- [ ] Reaching the far sidewalk advances to the next level with faster, denser traffic
- [ ] After level 3, Space or R restarts from level 1
