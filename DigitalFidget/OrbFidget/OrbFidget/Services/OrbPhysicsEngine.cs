using OrbFidget.Models;

namespace OrbFidget.Services;

public class OrbPhysicsEngine
{
    // Constants from design brief section 10
    public const float OrbRadius = 44f;
    public const float HitRadius = 74f;
    public const float MaxDragDist = 200f;
    public const float DragLerp = 0.3f;
    public const float SpringConstant = 12f;
    public const float Damping = 0.82f;
    public const float ImpulseScale = 10f;
    public const float BreathFrequency = 1.8f;
    public const float BreathAmplitude = 0.03f;
    public const float ScaleStretchFactor = 0.001f;
    public const float ScaleLerp = 0.15f;
    public static readonly int[] MilestoneThresholds = { 50, 110, 170 };
    public const int TrailLength = 20;
    public const int ParticleMinCount = 6;
    public const int ParticleMaxCount = 28;
    public const float ParticleFriction = 0.96f;
    public const float ParticleGravity = 0.05f;
    public const long ComboWindow = 1500;
    public const long ComboFade = 2000;
    public const float SnapMinIntensity = 0.12f;
    public const float StretchToneThreshold = 8f;
    public const float DtClamp = 0.05f;
    public const float DotGridSpacing = 40f;
    public const float DotGridRadius = 0.8f;

    private static readonly Random _rng = new();

    public void Update(OrbState state, float dt)
    {
        if (state.IsDragging)
        {
            UpdateDrag(state);
            UpdateStretchScale(state, dt);
            state.IdleTime = 0;
        }
        else if (state.IsSettling)
        {
            UpdateSpring(state, dt);
            UpdateStretchScale(state, dt);
            state.IdleTime = 0;

            float speed = MathF.Sqrt(state.VelocityX * state.VelocityX + state.VelocityY * state.VelocityY);
            float dist = MathF.Sqrt(state.OrbX * state.OrbX + state.OrbY * state.OrbY);
            if (speed < 0.1f && dist < 0.5f)
            {
                state.IsSettling = false;
                state.OrbX = 0;
                state.OrbY = 0;
                state.VelocityX = 0;
                state.VelocityY = 0;
            }
        }
        else
        {
            // Idle breathing
            state.IdleTime += dt;
            float breathScale = MathF.Sin(state.IdleTime * BreathFrequency) * BreathAmplitude + 1f;
            state.CurrentScale += (breathScale - state.CurrentScale) * 0.15f;
        }

        UpdateParticles(state, dt);
        UpdateTrail(state);
    }

    public bool TryGrab(OrbState state, float px, float py)
    {
        float dx = px - state.OrbX;
        float dy = py - state.OrbY;
        float dist = MathF.Sqrt(dx * dx + dy * dy);

        if (dist <= HitRadius)
        {
            state.IsDragging = true;
            state.IsSettling = false;
            state.PointerX = px;
            state.PointerY = py;
            state.MaxStretch = 0;
            state.MilestonesHit.Clear();
            state.VelocityX = 0;
            state.VelocityY = 0;
            state.TickAccumulator = 0;
            return true;
        }

        return false;
    }

    public void UpdatePointer(OrbState state, float px, float py)
    {
        state.PointerX = px;
        state.PointerY = py;
    }

    public (float intensity, int particlesSpawned, bool isCombo) Release(OrbState state)
    {
        state.IsDragging = false;

        float dist = MathF.Sqrt(state.OrbX * state.OrbX + state.OrbY * state.OrbY);
        float intensity = MathF.Min(dist / 170f, 1f);

        if (intensity <= SnapMinIntensity)
        {
            // Too small — just settle
            state.IsSettling = true;
            return (intensity, 0, false);
        }

        // Inject velocity impulse toward center
        float impulseAngle = MathF.Atan2(-state.OrbY, -state.OrbX);
        state.VelocityX += MathF.Cos(impulseAngle) * intensity * ImpulseScale;
        state.VelocityY += MathF.Sin(impulseAngle) * intensity * ImpulseScale;

        state.IsSettling = true;

        // Spawn particles
        int count = (int)(ParticleMinCount + intensity * (ParticleMaxCount - ParticleMinCount));
        float spawnX = state.OrbX;
        float spawnY = state.OrbY;

        for (int i = 0; i < count; i++)
        {
            float angle = (float)(_rng.NextDouble() * Math.PI * 2);
            float speed = 1.5f + (float)_rng.NextDouble() * 3.5f * intensity;
            float size = 2f + (float)_rng.NextDouble() * 4f;
            float life = 0.4f + (float)_rng.NextDouble() * 0.5f;
            float hue = 15f + (float)_rng.NextDouble() * 35f;

            state.Particles.Add(new Particle
            {
                X = spawnX,
                Y = spawnY,
                VX = MathF.Cos(angle) * speed,
                VY = MathF.Sin(angle) * speed,
                Size = size,
                Life = life,
                MaxLife = life,
                Hue = hue,
            });
        }

        // Combo check
        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        bool isCombo = false;
        if (now - state.LastSnapTimestamp < ComboWindow)
        {
            state.ComboCount++;
            isCombo = true;
        }
        else
        {
            state.ComboCount = 1;
        }
        state.LastSnapTimestamp = now;

        return (intensity, count, isCombo);
    }

    public int CheckMilestones(OrbState state)
    {
        if (!state.IsDragging) return -1;

        float dist = MathF.Sqrt(state.OrbX * state.OrbX + state.OrbY * state.OrbY);
        state.MaxStretch = MathF.Max(state.MaxStretch, dist);

        for (int i = 0; i < MilestoneThresholds.Length; i++)
        {
            int threshold = MilestoneThresholds[i];
            if (dist >= threshold && !state.MilestonesHit.Contains(threshold))
            {
                state.MilestonesHit.Add(threshold);
                return i;
            }
        }

        return -1;
    }

    public float GetStretchDistance(OrbState state)
    {
        return MathF.Sqrt(state.OrbX * state.OrbX + state.OrbY * state.OrbY);
    }

    private void UpdateDrag(OrbState state)
    {
        float rawDist = MathF.Sqrt(state.PointerX * state.PointerX + state.PointerY * state.PointerY);
        float mappedDist = rawDist < MaxDragDist
            ? rawDist
            : MaxDragDist + (rawDist - MaxDragDist) * 0.3f;

        float angle = MathF.Atan2(state.PointerY, state.PointerX);
        float targetX = MathF.Cos(angle) * mappedDist;
        float targetY = MathF.Sin(angle) * mappedDist;

        state.OrbX += (targetX - state.OrbX) * DragLerp;
        state.OrbY += (targetY - state.OrbY) * DragLerp;

        state.VelocityX = 0;
        state.VelocityY = 0;
    }

    private void UpdateSpring(OrbState state, float dt)
    {
        float ax = -state.OrbX * SpringConstant * dt;
        float ay = -state.OrbY * SpringConstant * dt;
        state.VelocityX = (state.VelocityX + ax) * Damping;
        state.VelocityY = (state.VelocityY + ay) * Damping;
        state.OrbX += state.VelocityX;
        state.OrbY += state.VelocityY;
    }

    private void UpdateStretchScale(OrbState state, float dt)
    {
        float dist = MathF.Sqrt(state.OrbX * state.OrbX + state.OrbY * state.OrbY);
        float targetScale = 1f + dist * ScaleStretchFactor;
        state.CurrentScale += (targetScale - state.CurrentScale) * ScaleLerp;
    }

    private void UpdateParticles(OrbState state, float dt)
    {
        for (int i = state.Particles.Count - 1; i >= 0; i--)
        {
            var p = state.Particles[i];
            p.VX *= ParticleFriction;
            p.VY *= ParticleFriction;
            p.VY += ParticleGravity;
            p.X += p.VX;
            p.Y += p.VY;
            p.Life -= dt;

            if (p.Life <= 0)
            {
                state.Particles.RemoveAt(i);
            }
            else
            {
                state.Particles[i] = p;
            }
        }
    }

    private void UpdateTrail(OrbState state)
    {
        state.Trail.AddLast(new TrailDot { X = state.OrbX, Y = state.OrbY });
        while (state.Trail.Count > TrailLength)
        {
            state.Trail.RemoveFirst();
        }
    }
}
