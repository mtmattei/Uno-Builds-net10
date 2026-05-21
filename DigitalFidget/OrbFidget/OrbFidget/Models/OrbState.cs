namespace OrbFidget.Models;

public class OrbState
{
    public float OrbX;
    public float OrbY;
    public float VelocityX;
    public float VelocityY;
    public float CurrentScale = 1f;
    public bool IsDragging;
    public bool IsSettling;
    public float PointerX;
    public float PointerY;
    public float MaxStretch;
    public HashSet<int> MilestonesHit = new();
    public List<Particle> Particles = new();
    public LinkedList<TrailDot> Trail = new();
    public float IdleTime;
    public int ComboCount;
    public long LastSnapTimestamp;
    public float TickAccumulator;
}
