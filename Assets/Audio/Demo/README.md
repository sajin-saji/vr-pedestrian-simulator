# Demo sounds

All sounds are synthesised from scratch by [`tools/generate_audio.py`](../../../tools/generate_audio.py)
(no recordings, free to share). Run `python tools/generate_audio.py` to rebuild them.

| File | Length | Used for |
| --- | --- | --- |
| `footstep_01.wav` – `footstep_04.wav` | 0.3 s, mono | Player footsteps on pavement: heel thump + toe scuff, picked at random with slight pitch change |
| `city_ambience.wav` | 30 s, stereo loop | 2D background: distant traffic rumble, cars passing left/right, wind and birds |
| `crowd_murmur.wav` | 20 s, mono loop | 3D source near the far sidewalk: people talking (unintelligible) |
| `car_engine_loop.wav` | 2 s, mono loop | 3D engine sound on every car; Unity raises pitch and volume with speed, Doppler on |

All files are 44.1 kHz, 16-bit WAV and loop without clicks.
