# Orb Fidget Toy

A single-interaction digital fidget toy built with Uno Platform and SkiaSharp. The entire experience is one object: a glowing sphere that the user drags, stretches against spring tension, and releases to snap back with particles, trail effects, and combo counting.

## Project Structure

```
OrbFidget/OrbFidget/
├── OrbFidget.csproj          (net10.0-desktop + net10.0-android, Skia + SkiaRenderer)
├── App.xaml / App.xaml.cs
├── MainPage.xaml / .cs        (dark #141210 background, wires OrbCanvas on Loaded)
├── GlobalUsings.cs
├── Models/
│   ├── OrbState.cs            (mutable game state)
│   ├── Particle.cs            (burst particle struct)
│   └── TrailDot.cs            (trail position struct)
├── Helpers/
│   └── ColorHelper.cs         (HSL conversion, accent/background colors)
├── Services/
│   ├── IOrbHapticService.cs   (interface)
│   ├── OrbHapticService.cs    (VibrationDevice, no-op on Desktop)
│   ├── IOrbAudioEngine.cs     (interface)
│   ├── OrbAudioEngine.cs      (PCM WAV generation, pentatonic chimes)
│   └── OrbPhysicsEngine.cs    (rubber-band drag, damped spring, particles, trail, combos)
└── Controls/
    └── OrbCanvas.cs           (SKCanvasElement with 12-layer rendering + pointer input)
```

## Features

- **Rubber-band drag** with logarithmic falloff past 200px
- **Damped spring snap-back** (K=12, damping=0.82) with velocity impulse overshoot
- **Idle breathing animation** (3% oscillation at ~0.29 Hz)
- **Particle burst system** (6-28 particles with friction, gravity, age-based fade)
- **20-position trail** ring buffer
- **3 milestone rings** at 50/110/170px with hit detection
- **Combo system** (1.5s window, fading text)
- **12-layer back-to-front rendering** (background gradient, dot grid, anchor, stretch line, trail, particles, milestones, orb glow/body/specular/rim, combo text)
- **Programmatic PCM audio** (pentatonic chimes, stretch tone, milestone clicks)
- **Haptic feedback** service (graceful no-op on Desktop)

## Targets

- **Desktop (Skia)** — Windows, macOS, Linux
- **Android** — touch input with haptic feedback

## Build & Run

```bash
# Desktop
dotnet build -f net10.0-desktop
dotnet run -f net10.0-desktop

# Android
dotnet build -f net10.0-android
```

## Known Limitations

- **Audio on Desktop:** `MediaSource.CreateFromStream` is not yet implemented in Uno Platform for Desktop. Audio playback silently no-ops. Audio works on Android.
- **Haptics on Desktop:** `VibrationDevice` is not available on Desktop. The service gracefully no-ops.
