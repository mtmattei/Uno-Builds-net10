# ORB — Design & Architecture Brief

**A Single-Interaction Digital Fidget Toy**
Target Platform: Uno Platform (C# / WinUI 3 / XAML + SkiaSharp Canvas)
Version 1.0 · March 2026

---

## 1. Overview

Orb is a single-interaction digital fidget toy. The entire experience is one object: a glowing sphere that the user drags, stretches against spring tension, and releases to snap back to center. Every aspect serves one goal — make the pull-and-release cycle so satisfying the user repeats it unconsciously.

No score. No progression. No UI chrome. The orb sits on a dark canvas, breathes gently, and waits to be touched.

**Core loop:** grab → stretch → feel resistance build → release → snap-back → particles burst → chime → repeat.

### 1.1 Psychology Hooks

| Hook | Mechanism | Implementation |
|------|-----------|----------------|
| Variable Reward | Each release plays a random pentatonic chime and spawns a unique particle burst. The brain cannot predict the exact payoff. | Random note from C4 pentatonic scale; random particle count, angle, speed, hue per burst. |
| Tension → Release | Dragging builds resistance via rubber-band physics and a rising stretch tone. Releasing delivers the payoff: spring overshoot, particles, chime. | Logarithmic drag mapping past 200px; spring K=12, damping=0.82. |
| Haptic Feedback | Device vibration at grab, at stretch milestones, and on snap-back. Intensity scales with stretch distance. | Vibration API with three escalating patterns. |
| Milestone Escalation | Three invisible rings appear while dragging. Crossing each triggers audio + haptic feedback, creating micro-goals. | Rings at 50px, 110px, 170px from center. |
| Combo System | Rapid successive snaps within 1.5s build a visible multiplier (×2, ×3…). Fades after 2s idle. | Timestamp comparison. Counter increments if gap < 1500ms. |

---

## 2. Visual Design Specification

### 2.1 Color System

| Token | Value | Usage |
|-------|-------|-------|
| Background Dark | `#141210` | Canvas fill, outer edge of radial gradient |
| Background Mid | `#1E1B17` | Canvas fill, center of radial gradient |
| Accent (Terracotta) | `#C9785D` / `rgb(201, 120, 93)` | Orb glow, anchor glow, stretch line, particles, milestone rings, combo text, trail |
| Grid Dot | `rgba(100, 90, 80, 0.04)` | Subtle dot grid across background, 40px spacing, 0.8px radius |
| Rim Light | `rgba(255, 200, 160, 0.08–0.14)` | Thin stroke around orb edge; alpha increases during drag |
| Specular | `rgba(255, 255, 255, 0.18)` | Highlight on orb, offset upper-left |
| Title Text | `rgba(200, 191, 176, 0.35)` | Title; extremely muted |
| Subtitle Text | `rgba(200, 191, 176, 0.15)` | Instruction text |

### 2.2 Typography

- **Title:** Fraunces (serif), 20px, weight 300, letter-spacing 0.12em, color `rgba(200,191,176,0.35)`
- **Subtitle:** DM Mono, 8px, letter-spacing 0.35em, uppercase, color `rgba(200,191,176,0.15)`. Content: `"drag · stretch · release"`
- **Combo indicator:** DM Mono, 11px, weight 500, color `rgba(201,120,93, fading alpha)`

### 2.3 The Orb — Four Visual Layers

The orb is the sole interactive element. It is rendered as a layered circle with four components, drawn in this order:

#### 2.3.1 Outer Glow

- Radial gradient from orb center outward
- Radius: `orbRadius + 20 + (isDragging ? stretchDist * 0.15 : 0)`
- Stop 0.0: accent color at alpha `0.08` (idle) or `0.15` (dragging)
- Stop 0.6: accent color at alpha `0.03` (idle) or `0.06` (dragging)
- Stop 1.0: transparent
- The glow physically expands with stretch distance during drag

#### 2.3.2 Body

- Radial gradient with light origin offset upper-left: `(-25%, -30%)` relative to orb center
- Inner radius of gradient source: `finalRadius * 0.1`
- Three HSL color stops, all hue-shifted based on stretch distance:
  - `hueShift = stretchDist * 0.15`
  - Stop 0.0: `hsl(22 + hueShift, 55%, 52%)` — warm terracotta highlight
  - Stop 0.5: `hsl(18 + hueShift, 50%, 38%)` — mid body
  - Stop 1.0: `hsl(15 + hueShift, 45%, 22%)` — dark edge
- The orb shifts warmer/more orange the further it is stretched. Subtle but perceptible.

#### 2.3.3 Specular Highlight

- Radial gradient centered at `(-30%, -35%)` relative to orb center
- Radius: `60%` of orb radius
- Stop 0.0: `rgba(255, 255, 255, 0.18)`
- Stop 0.5: `rgba(255, 255, 255, 0.04)`
- Stop 1.0: transparent
- Creates the illusion of a 3D sphere with top-left lighting

#### 2.3.4 Rim Light

- 1.5px stroke around the orb circumference
- Color: `rgba(255, 200, 160, 0.08 + (isDragging ? 0.06 : 0))`
- Brightens slightly during drag to emphasize the object under interaction

### 2.4 Idle Breathing

- When untouched, the orb radius oscillates: `finalRadius = baseRadius × (sin(time × 1.8) × 0.03 + 1)`
- This is a **3% scale oscillation** at **1.8 rad/s** (~0.29 Hz, one breath every ~3.5 seconds)
- The breathing is the primary idle affordance — it says "I am alive, touch me"

### 2.5 Base Orb Dimensions

- Base radius: **44px** (88px diameter)
- Hit target radius: **74px** (base + 30px tolerance = 148px touch target)
- Scale factor during stretch: `1.0 + stretchDist × 0.001` (grows ~20% at max stretch)
- Scale interpolation: `currentScale += (targetScale - currentScale) × 0.15`

---

## 3. Interaction States & Transitions

### 3.1 State Machine

| State | Entry Condition | Visual | Audio | Haptic |
|-------|----------------|--------|-------|--------|
| **IDLE** | Default / pointer up with orb at rest | Orb at center, breathing, ambient glow | None | None |
| **GRABBED** | Pointer down within 74px of orb center | Breathing pauses (scale driven by physics), glow brightens | None | 10ms pulse |
| **STRETCHING** | Pointer moves while grabbed | Rubber-band follow; anchor glow at center; dashed stretch line; milestone rings visible; trail dots; orb hue shifts warmer | Continuous stretch tone (pitch rises) | Milestone vibrations at 50/110/170px |
| **RELEASED** | Pointer up while intensity > 0.12 | Spring snap-back with overshoot; particle burst at release point; trail persists | Random pentatonic chime (3 layered oscillators + click) | Scaled vibration pattern |
| **SETTLING** | Immediately after release | Damped spring oscillation back to center | None | None |
| **COMBO** | Release within 1500ms of previous release | Combo text (×2, ×3…) below orb, fades over 2000ms | Same as RELEASED | Same as RELEASED |

### 3.2 Transition Flow

```
IDLE → GRABBED          (pointer down on orb)
GRABBED → STRETCHING    (pointer moves)
STRETCHING → RELEASED   (pointer up, intensity > 0.12)
STRETCHING → IDLE       (pointer up, intensity ≤ 0.12 — too small to snap)
RELEASED → SETTLING     (immediate, spring physics begins)
SETTLING → IDLE         (velocity < 0.1 px/frame)
RELEASED → COMBO        (if previous release was < 1500ms ago)
```

**Intensity** is defined as: `min(stretchDist / 170, 1.0)`

---

## 4. Physics Specification

### 4.1 Drag / Rubber-Band Model

During drag, the orb follows the pointer with a rubber-band constraint and interpolation lag:

```
rawDist    = hypot(pointerX - centerX, pointerY - centerY)
maxDist    = 200
mappedDist = rawDist < maxDist ? rawDist : maxDist + (rawDist - maxDist) × 0.3
angle      = atan2(pointerY - centerY, pointerX - centerX)
targetX    = cos(angle) × mappedDist
targetY    = sin(angle) × mappedDist
orbX      += (targetX - orbX) × 0.3    // per frame
orbY      += (targetY - orbY) × 0.3    // per frame
velocity   = (0, 0)                     // zeroed during drag
```

The **0.3 interpolation factor** creates a smooth "following" feel rather than 1:1 tracking. The **logarithmic falloff past 200px** creates increasing perceived resistance — the orb never reaches the pointer at extreme stretch, which feels like pulling against an elastic.

### 4.2 Spring-Back Model

On release, the orb is governed by a damped spring:

```
springConstant = 12
damping        = 0.82

// Per frame:
ax = -orbX × springConstant × deltaTime
ay = -orbY × springConstant × deltaTime
velocityX = (velocityX + ax) × damping
velocityY = (velocityY + ay) × damping
orbX += velocityX
orbY += velocityY
```

On the frame of release, an additional **velocity impulse** is injected toward center:

```
impulseAngle = atan2(-orbY, -orbX)
velocityX += cos(impulseAngle) × intensity × 10
velocityY += sin(impulseAngle) × intensity × 10
```

This impulse creates the satisfying **overshoot past center** before the spring pulls it back. The damping of 0.82 means the oscillation dies within roughly 8–12 frames depending on initial intensity.

### 4.3 Scale Animation

```
targetScale  = 1.0 + stretchDist × 0.001
currentScale += (targetScale - currentScale) × 0.15
```

The orb grows slightly while stretched (max ~1.2× at 200px) and smoothly returns to 1.0.

### 4.4 Frame Rate & Delta Time

- Target: **60fps** via `CompositionTarget.Rendering` or `DispatcherTimer(16ms)`
- `deltaTime = min((now - lastFrame) / 1000, 0.05)` — clamped to prevent physics explosion on tab-switch
- All physics calculations are dt-dependent

---

## 5. Visual Effects Specification

### 5.1 Particle System

Particles spawn at the orb's position at the moment of release. Purely cosmetic.

| Property | Value / Formula |
|----------|----------------|
| Count | `floor(6 + intensity × 22)` — range 6 to 28 |
| Initial position | Orb center at release moment |
| Direction | Random angle: `random() × 2π` |
| Speed | `1.5 + random() × 3.5 × intensity` |
| Size | `2 + random() × 4` pixels |
| Lifetime | `0.4 + random() × 0.5` seconds |
| Color hue | `15 + random() × 35` (warm amber through orange-gold) |
| Color format | `hsla(hue, 70%, 65%, alpha)` |
| Velocity decay | `vx *= 0.96` and `vy *= 0.96` per frame (air friction) |
| Gravity | `vy += 0.05` per frame (subtle downward drift) |
| Alpha | `(remainingLife / maxLife) × 0.8` |
| Size decay | `initialSize × (remainingLife / maxLife)` |
| Removal | When age exceeds maxLife |

### 5.2 Trail

- Ring buffer of the last **20** orb positions, one recorded each frame
- Each dot radius: `(index / trailLength) × 6` — grows from oldest to newest
- Each dot alpha: `(index / trailLength) × 0.15`
- Color: `rgba(201, 120, 93, alpha)`
- Oldest entries are shifted out. Trail is visible during both drag and spring-back

### 5.3 Anchor Indicator

Visible only when orb is displaced **more than 10px** from center:

- **Glow:** Radial gradient, 30px radius, alpha = `min(dist / 200, 0.3)`
- **Crosshair:** Two perpendicular lines, 24px total span (12px each direction), 0.5px stroke
- Crosshair alpha: `min(dist / 300, 0.15)`

### 5.4 Stretch Line

Visible when orb is displaced **more than 8px**:

- Dashed line from screen center to orb center
- Dash pattern: `[4, 6]`
- Line width: `2 + dist × 0.01`
- Alpha: `min(dist / 200, 0.5) × 0.3`
- Color: accent (terracotta)

### 5.5 Milestone Rings

Visible **only during drag state**. Three concentric circles centered on screen center:

| Ring | Radius | Unhit Style | Hit Style |
|------|--------|-------------|-----------|
| 1 | 50px | 0.5px dashed `[3,5]`, `rgba(100,90,80,0.08)` | 1.5px solid, `rgba(201,120,93,0.25)` |
| 2 | 110px | 0.5px dashed `[3,5]`, `rgba(100,90,80,0.06)` | 1.5px solid, `rgba(201,120,93,0.19)` |
| 3 | 170px | 0.5px dashed `[3,5]`, `rgba(100,90,80,0.04)` | 1.5px solid, `rgba(201,120,93,0.13)` |

Once a milestone is hit during a drag, it stays lit for the remainder of that drag. **Milestones reset on each new grab.**

### 5.6 Background

- Radial gradient centered on screen: `#1E1B17` (center) → `#141210` (edges at 60% of canvas width)
- Dot grid: every 40px in both axes, 0.8px radius circles, `rgba(100,90,80,0.04)`
- The grid provides barely-perceptible texture that prevents the background from feeling like a void

### 5.7 Combo Text

- Position: centered horizontally, `orbY + finalRadius + 24px` vertically
- Format: `×{count}` (e.g. ×2, ×3, ×7)
- Font: DM Mono, 11px, weight 500
- Alpha: `max(0, 1 - (now - lastSnapTime) / 2000) × 0.6`
- Disappears completely 2 seconds after the last snap

---

## 6. Audio Specification

All audio is **synthesized at runtime**. No audio assets needed. Each sound is one or more oscillators with an exponential gain decay routed through a lowpass filter.

### 6.1 Synth Architecture

```
Signal chain: Oscillator → BiquadFilter (lowpass, 3000Hz) → GainNode → Destination
Gain envelope: instant attack, exponential ramp to 0.001 over [duration]
```

All parameters below: `(frequency, duration, waveform, volume, detune_cents)`

### 6.2 Sound Definitions

| Sound | Trigger | Oscillators |
|-------|---------|-------------|
| `stretchTone` | Continuous during drag (throttled: fires when `tickAccumulator > 8`) | `(200 + dist×3, 0.06s, sine, 0.02 + min(dist/500, 0.03), 0)` |
| `milestoneClick` | Each milestone ring crossed (once per ring per drag) | `(1200, 0.04s, square, 0.025, 0)` + `(600, 0.06s, triangle, 0.018, 0)` |
| `snapChime` | On release when intensity > 0.12 | 4 simultaneous oscillators (see below) |

### 6.3 snapChime Detail

The snap chime is the **primary reward sound**. Four simultaneous oscillators:

1. **Fundamental:** Random note from pentatonic `[261.6, 329.6, 392, 523.3, 659.3, 784]` Hz. Duration 0.5s, sine, volume = `0.04 + intensity × 0.04`
2. **Octave harmonic:** `freq × 2`, duration 0.3s, sine, `volume × 0.35`, detune **+7 cents**
3. **Sub-octave:** `freq × 0.5`, duration 0.55s, sine, `volume × 0.2`, detune **−5 cents**
4. **Percussive click:** 1800Hz, 0.03s, **square** wave, volume 0.02. Provides the "snap" attack transient.

The slight detune on harmonics creates a shimmering quality. The random note selection means every release sounds different but always consonant (pentatonic scale guarantees no dissonance).

### 6.4 Stretch Tone Throttling

```
tickAccumulator += stretchDistance × deltaTime
when tickAccumulator > 8 → fire one stretchTone, reset accumulator to 0
```

This produces more frequent tones when stretched further (higher distance = faster accumulation). Near center, tones are sparse. At max stretch, they approach a continuous hum.

---

## 7. Haptic Feedback Specification

| Event | Vibration Pattern (ms) | Uno/Platform API |
|-------|----------------------|------------------|
| Grab (pointer down on orb) | `[10]` — single 10ms pulse | `HapticFeedback.LightImpact` |
| Milestone 1 crossed (50px) | `[8]` — 8ms pulse | `HapticFeedback.LightImpact` |
| Milestone 2 crossed (110px) | `[14]` — 14ms pulse | `HapticFeedback.MediumImpact` |
| Milestone 3 crossed (170px) | `[20]` — 20ms pulse | `HapticFeedback.HeavyImpact` |
| Release (intensity ≤ 0.6) | `[10, 18]` — double pulse | `HapticFeedback.MediumImpact` |
| Release (intensity > 0.6) | `[15, 30, 20]` — triple pulse | `HapticFeedback.HeavyImpact` |

**Milestone vibration formula:** `duration = 8 + milestoneIndex × 6` ms → yields 8, 14, 20ms.

**Platform mapping:** Android → `Vibrator` service. iOS → `UIImpactFeedbackGenerator` with matching style. WASM → `navigator.vibrate`. Desktop → skip gracefully.

---

## 8. Uno Platform Architecture

### 8.1 Project Structure

```
OrbFidget/
├── Models/
│   ├── OrbState.cs          — all mutable physics + visual state
│   └── Particle.cs          — particle data struct
├── Services/
│   ├── OrbPhysicsEngine.cs  — drag, spring, particles, trail
│   ├── OrbAudioEngine.cs    — synth via platform audio API
│   └── OrbHapticService.cs  — platform haptic abstraction
├── Controls/
│   └── OrbCanvas.cs         — SKXamlCanvas + pointer handlers + draw
├── MainPage.xaml             — fullscreen canvas host
└── App.xaml
```

### 8.2 Rendering Strategy

Use **`SKXamlCanvas`** (SkiaSharp.Views.Uno) as the rendering surface. Hardware-accelerated 2D drawing with full gradient, path, and compositing support across all Uno targets.

- Subscribe to `PaintSurface` event for all drawing
- Drive the game loop with `CompositionTarget.Rendering` or `DispatcherTimer` at 16ms interval
- Each tick: call `OrbPhysicsEngine.Update(dt)`, then `canvas.Invalidate()`
- All drawing in `OnPaintSurface(SKPaintSurfaceEventArgs e)` using `SKCanvas`

### 8.3 SkiaSharp Drawing Mappings

| Web Canvas API | SkiaSharp Equivalent |
|----------------|---------------------|
| `createRadialGradient()` | `SKShader.CreateRadialGradient()` |
| `arc()` + `fill()` | `canvas.DrawCircle()` with `SKPaint { Shader = gradient }` |
| `setLineDash([4, 6])` | `SKPaint { PathEffect = SKPathEffect.CreateDash(new[]{4f, 6f}, 0) }` |
| `fillText()` | `canvas.DrawText()` with `SKPaint { Typeface = ... }` |
| `rgba()` alpha | `SKPaint { Color = new SKColor(r, g, b, a) }` |
| `strokeStyle` + `stroke()` | `SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = ... }` |
| HSL colors | `SKColor.FromHsl(h, s, l)` or manual conversion |

### 8.4 Input Handling

Register pointer events on the `SKXamlCanvas` or its parent container:

- `PointerPressed` → check hit radius (74px), set drag state, fire haptic
- `PointerMoved` → update pointer position (only if dragging)
- `PointerReleased` → calculate intensity, fire snap chime + haptic + particles, inject impulse
- `PointerCancelled` → treat as release

Set `ManipulationMode="All"` on the canvas to capture all pointer events and prevent scroll interference.

### 8.5 Audio Strategy

| Target | Approach |
|--------|----------|
| **WASM** | JS interop to Web Audio API (identical to reference implementation) |
| **Android** | `AudioTrack` for PCM sine/square/triangle wave generation |
| **iOS** | `AVAudioEngine` with `AVAudioPlayerNode` or AudioToolbox |
| **Desktop (WinUI)** | `Windows.Media.Audio` AudioGraph with `AudioFrameInputNode` |

**Simpler alternative:** Pre-render 6 pentatonic chime WAV files (one per note) + stretch tone + milestone click as short samples. Play randomly on release. Sacrifices variable-detune shimmer but dramatically simplifies cross-platform audio.

### 8.6 OrbState Model

```csharp
public class OrbState
{
    public float OrbX, OrbY;              // offset from center (px)
    public float VelocityX, VelocityY;    // spring velocity
    public float Scale;                    // current scale (breathing + stretch)
    public bool IsDragging;
    public float PointerX, PointerY;      // last pointer position (canvas-relative)
    public float MaxStretch;              // max distance this drag session
    public HashSet<int> MilestonesHit;    // subset of {50, 110, 170}
    public List<Particle> Particles;
    public Queue<TrailDot> Trail;         // ring buffer, max 20 entries
    public float IdleTime;                // cumulative seconds, for breathing sin()
    public int ComboCount;
    public long LastSnapTimestamp;         // ms since epoch
    public float TickAccumulator;         // for stretch tone throttle
}

public struct Particle
{
    public float X, Y, VX, VY;
    public float Size, Life, MaxLife;
    public float Hue;                     // 15–50 range
    public long BirthTime;
}

public struct TrailDot
{
    public float X, Y;
    public long Timestamp;
}
```

---

## 9. Rendering Order (Z-Index, Back to Front)

Every frame, clear the canvas and redraw in this exact order:

1. Background radial gradient (full canvas fill)
2. Dot grid (40px spacing, 0.8px radius, near-invisible)
3. Anchor glow + crosshair (only if `dist > 10`)
4. Stretch line (dashed, only if `dist > 8`)
5. Trail dots (oldest/smallest first)
6. Particles (all active)
7. Milestone rings (drag state only)
8. Orb outer glow
9. Orb body (radial gradient)
10. Orb specular highlight
11. Orb rim light (stroke)
12. Combo text (only if active)

This ensures the orb is always on top, particles appear behind the orb, and the stretch line / anchor are the deepest interactive layers.

---

## 10. Constants Reference

| Constant | Value | Notes |
|----------|-------|-------|
| `ORB_RADIUS` | 44px | Base radius before scale/breathing |
| `HIT_RADIUS` | 74px | Orb radius + 30px grab tolerance |
| `MAX_DRAG_DIST` | 200px | Linear drag range; beyond this, 0.3× falloff |
| `DRAG_LERP` | 0.3 | Per-frame interpolation toward pointer target |
| `SPRING_CONSTANT` | 12 | Restoring force multiplier |
| `DAMPING` | 0.82 | Velocity retention per frame |
| `IMPULSE_SCALE` | 10 | Release velocity boost = intensity × 10 |
| `BREATH_FREQUENCY` | 1.8 rad/s | Idle oscillation speed (~0.29 Hz) |
| `BREATH_AMPLITUDE` | 0.03 | 3% radius oscillation |
| `SCALE_STRETCH_FACTOR` | 0.001 | Additional scale per pixel of displacement |
| `SCALE_LERP` | 0.15 | Scale interpolation speed |
| `MILESTONE_THRESHOLDS` | [50, 110, 170] | Distance thresholds in pixels |
| `TRAIL_LENGTH` | 20 | Maximum trail dots |
| `PARTICLE_MIN_COUNT` | 6 | At intensity 0 |
| `PARTICLE_MAX_COUNT` | 28 | At intensity 1 |
| `PARTICLE_FRICTION` | 0.96 | Velocity retention per frame |
| `PARTICLE_GRAVITY` | 0.05 | Downward acceleration per frame |
| `COMBO_WINDOW` | 1500ms | Max gap between snaps for combo |
| `COMBO_FADE` | 2000ms | Time for combo text to disappear |
| `SNAP_MIN_INTENSITY` | 0.12 | Minimum intensity to trigger snap feedback |
| `STRETCH_TONE_THRESHOLD` | 8 | Tick accumulator threshold for stretch tone |
| `DT_CLAMP` | 0.05s | Maximum delta time (prevents physics explosion) |
| `DOT_GRID_SPACING` | 40px | Background dot grid interval |
| `DOT_GRID_RADIUS` | 0.8px | Background dot size |

---

*This document contains all information needed to reproduce the Orb fidget toy pixel-for-pixel and behavior-for-behavior on Uno Platform. Every value, formula, color, timing, and interaction rule is specified. The only creative decisions left to the implementer are platform-specific audio synthesis strategy and haptic API selection.*
