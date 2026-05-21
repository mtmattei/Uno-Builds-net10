namespace OrbFidget.Services;

public interface IOrbHapticService
{
    Task InitializeAsync();
    void OnGrab();
    void OnMilestone(int index);
    void OnRelease(float intensity);
}
