"""
Generate the sample sounds used by the Unity desktop demo.

All sounds are synthesised from scratch (no recordings), so they can be shared freely.

    python tools/generate_audio.py

Writes to Assets/Audio/Demo/:
    footstep_01.wav ... footstep_04.wav   footsteps on pavement (mono)
    city_ambience.wav                     30 s stereo loop: distant traffic, passing cars, wind, birds
    crowd_murmur.wav                      20 s mono loop: people talking nearby (unintelligible)
    car_engine_loop.wav                   2 s mono loop: 4-cylinder engine, Unity changes pitch with speed
"""
from pathlib import Path
import wave

import numpy as np
from scipy import signal

SR = 44100
OUT = Path(__file__).resolve().parents[1] / "Assets" / "Audio" / "Demo"
rng = np.random.default_rng(7)


# ------------------------------------------------------------------ helpers
def bandpass(x, lo, hi, order=4):
    sos = signal.butter(order, [lo, hi], btype="band", fs=SR, output="sos")
    return signal.sosfilt(sos, x, axis=0)


def lowpass(x, f, order=4):
    return signal.sosfilt(signal.butter(order, f, btype="low", fs=SR, output="sos"), x, axis=0)


def highpass(x, f, order=4):
    return signal.sosfilt(signal.butter(order, f, btype="high", fs=SR, output="sos"), x, axis=0)


def pink(n):
    """Pink noise via spectral shaping."""
    spec = np.fft.rfft(rng.standard_normal(n))
    f = np.fft.rfftfreq(n, 1 / SR)
    f[0] = f[1]
    x = np.fft.irfft(spec / np.sqrt(f), n)
    return x / np.max(np.abs(x))


def smooth_noise(n, rate_hz):
    """Slow random modulation curve in [0, 1]."""
    pts = max(4, int(n / SR * rate_hz) + 2)
    y = signal.resample(rng.random(pts), n)
    y -= y.min()
    return y / (y.max() + 1e-9)


def reverb(x, seconds=0.6, mix=0.25, damp=3000):
    n = int(seconds * SR)
    t = np.arange(n) / SR
    ir = rng.standard_normal(n) * np.exp(-t * 6.9 / seconds)
    ir = lowpass(ir, damp, 2)
    ir /= np.sqrt(np.sum(ir ** 2))
    if x.ndim == 1:
        wet = signal.fftconvolve(x, ir)[: len(x)]
    else:
        wet = np.stack([signal.fftconvolve(x[:, c], np.roll(ir, 37 * c))[: len(x)] for c in range(x.shape[1])], 1)
    return (1 - mix) * x + mix * wet


def make_loop(x, fade_s):
    """Seamless loop: crossfade the tail into the head, return length len(x) - fade."""
    f = int(fade_s * SR)
    w = np.linspace(0, 1, f)
    if x.ndim == 2:
        w = w[:, None]
    head = x[:f] * w + x[-f:] * (1 - w)
    return np.concatenate([head, x[f:-f]])


def save(name, x, peak_db=-3.0):
    x = np.asarray(x, dtype=np.float64)
    x = x / (np.max(np.abs(x)) + 1e-9) * 10 ** (peak_db / 20)
    pcm = (x * 32767).astype("<i2")
    ch = 1 if x.ndim == 1 else x.shape[1]
    with wave.open(str(OUT / name), "wb") as w:
        w.setnchannels(ch)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(pcm.tobytes())
    print(f"  {name:22s} {ch} ch  {len(x) / SR:5.2f} s")


# ------------------------------------------------------------------ sounds
def footstep(seed):
    r = np.random.default_rng(seed)
    n = int(0.30 * SR)
    t = np.arange(n) / SR
    x = np.zeros(n)
    # heel: low thump + short gritty burst
    thump = np.sin(2 * np.pi * r.uniform(70, 95) * t) * np.exp(-t * r.uniform(38, 48))
    grit = bandpass(r.standard_normal(n), 300, r.uniform(2800, 3600)) * np.exp(-t * r.uniform(55, 70))
    x += 0.9 * thump + 0.55 * grit
    # toe / sole roll: softer, brighter scuff a little later
    d = int(r.uniform(0.07, 0.10) * SR)
    tt = np.arange(n - d) / SR
    scuff = bandpass(r.standard_normal(n - d), 900, 6000) * np.exp(-tt * r.uniform(45, 60))
    scuff *= np.minimum(1, tt / 0.004)
    x[d:] += r.uniform(0.25, 0.4) * scuff
    x *= np.minimum(1, t / 0.0015)                       # no click at the start
    x = reverb(x, 0.35, 0.18, 4000)
    x[-int(0.03 * SR):] *= np.linspace(1, 0, int(0.03 * SR))
    return x


def city_ambience(seconds=30, fade=3):
    n = int((seconds + fade) * SR)
    t = np.arange(n) / SR
    # distant traffic rumble: brown-ish noise, slowly breathing
    rumble = lowpass(pink(n), 380, 2)
    rumble *= 0.55 + 0.45 * smooth_noise(n, 0.15)
    out = np.stack([rumble, np.roll(rumble, 1700)], 1) * 0.9
    # cars passing in the distance: band noise with a swell and a left/right sweep
    for _ in range(7):
        dur = rng.uniform(4.5, 7.5)
        start = rng.uniform(0, seconds + fade - dur)
        m = int(dur * SR)
        s = int(start * SR)
        tt = np.linspace(0, 1, m)
        env = np.exp(-((tt - 0.5) ** 2) / 0.035)
        car = bandpass(rng.standard_normal(m), 90, rng.uniform(900, 1500)) * env
        pan = tt if rng.random() < 0.5 else 1 - tt
        gain = rng.uniform(0.35, 0.7)
        out[s:s + m, 0] += gain * car * np.cos(pan * np.pi / 2)
        out[s:s + m, 1] += gain * car * np.sin(pan * np.pi / 2)
    # wind in the trees
    wind = highpass(pink(n), 500, 2) * (0.1 + 0.25 * smooth_noise(n, 0.25) ** 2)
    out += np.stack([wind, np.roll(wind, 3100)], 1) * 0.5
    # a few birds
    for _ in range(12):
        s = int(rng.uniform(0, seconds + fade - 1) * SR)
        chirps = rng.integers(2, 5)
        pan = rng.uniform(0.15, 0.85)
        f0 = rng.uniform(2800, 4200)
        for k in range(chirps):
            m = int(rng.uniform(0.06, 0.11) * SR)
            tt = np.arange(m) / SR
            f = f0 + rng.uniform(800, 1600) * np.sin(np.pi * tt / tt[-1])
            ph = 2 * np.pi * np.cumsum(f) / SR
            c = np.sin(ph) * np.sin(np.pi * tt / tt[-1]) ** 2 * 0.07
            a = s + k * int(0.14 * SR)
            if a + m < n:
                out[a:a + m, 0] += c * np.cos(pan * np.pi / 2)
                out[a:a + m, 1] += c * np.sin(pan * np.pi / 2)
    out = reverb(out, 1.2, 0.2, 2500)
    return make_loop(out, fade)


def crowd_murmur(seconds=20, fade=2, voices=14):
    n = int((seconds + fade) * SR)
    t = np.arange(n) / SR
    out = np.zeros(n)
    vowels = [(730, 1090, 2440), (530, 1840, 2480), (270, 2290, 3010),
              (570, 840, 2410), (300, 870, 2240), (660, 1720, 2410)]
    for v in range(voices):
        female = rng.random() < 0.5
        f0 = rng.uniform(170, 230) if female else rng.uniform(95, 135)
        # intonation + jitter
        pitch = f0 * (1 + 0.12 * (smooth_noise(n, 0.8) - 0.5) + 0.01 * rng.standard_normal(n).cumsum() / np.sqrt(n))
        phase = np.cumsum(pitch) / SR
        source = (2 * (phase % 1) - 1) ** 3                 # buzzy glottal-like source
        source += 0.15 * rng.standard_normal(n)
        # syllables: change vowel ~4-5 times per second, with pauses between phrases
        syl = int(SR / rng.uniform(3.8, 5.2))
        voiced = np.zeros(n)
        k = 0
        while k < n:
            phrase = int(rng.uniform(1.2, 3.5) * SR)
            gap = int(rng.uniform(0.3, 1.4) * SR)
            e = min(n, k + phrase)
            voiced[k:e] = 1
            k = e + gap
        env = np.zeros(n)
        voice = np.zeros(n)
        for s in range(0, n, syl):
            e = min(n, s + syl)
            seg = source[s:e]
            F = vowels[rng.integers(len(vowels))]
            if female:
                F = tuple(f * 1.15 for f in F)
            y = sum(g * bandpass(seg, f * 0.85, f * 1.15, 2) for f, g in zip(F, (1.0, 0.6, 0.3)))
            w = np.sin(np.pi * np.linspace(0, 1, e - s)) ** 0.6
            voice[s:e] = y * w
        voice *= signal.resample(signal.resample(voiced, n // 400), n).clip(0, 1)
        dist = rng.uniform(0.25, 1.0)                        # nearer voices louder and brighter
        voice = lowpass(voice, 1800 + 2200 * dist, 2) * dist
        out += voice
    out = lowpass(out, 3500, 2)
    out += 0.08 * lowpass(pink(n), 600, 2)                   # room tone
    out = reverb(out, 0.9, 0.35, 2500)
    return make_loop(out, fade)


def car_engine_loop(seconds=2.0, rpm=1500):
    """4-stroke 4-cylinder: firing frequency = rpm / 60 * 2. Integer number of
    cycles in the loop, so it repeats without a click."""
    n = int(seconds * SR)
    t = np.arange(n) / SR
    fire = rpm / 60 * 2                                       # 50 Hz
    x = np.zeros(n)
    for h, a in [(0.5, 0.35), (1, 1.0), (2, 0.55), (3, 0.3), (4, 0.22), (6, 0.12), (8, 0.06)]:
        x += a * np.sin(2 * np.pi * fire * h * t + rng.uniform(0, 2 * np.pi))
    # combustion pulses: noise bursts at the firing rate
    pulses = (np.sin(2 * np.pi * fire * t) * 0.5 + 0.5) ** 6
    noise = rng.standard_normal(n)
    noise = np.concatenate([noise[-4000:], noise])           # pre-roll so the filter state loops
    noise = bandpass(noise, 120, 2200)[4000:]
    x += 0.6 * noise * pulses
    # intake / exhaust hiss
    x += 0.08 * bandpass(rng.standard_normal(n), 2000, 6000)
    x = lowpass(np.tile(x, 3), 3800, 2)[n:2 * n]              # filter as a loop
    return x


if __name__ == "__main__":
    OUT.mkdir(parents=True, exist_ok=True)
    print("Writing sounds to", OUT)
    for i in range(4):
        save(f"footstep_{i + 1:02d}.wav", footstep(100 + i), -4)
    save("city_ambience.wav", city_ambience(), -3)
    save("crowd_murmur.wav", crowd_murmur(), -3)
    save("car_engine_loop.wav", car_engine_loop(), -3)
