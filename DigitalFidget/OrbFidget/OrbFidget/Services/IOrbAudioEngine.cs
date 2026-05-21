namespace OrbFidget.Services;

public interface IOrbAudioEngine
{
    void PlayStretchTone(float distance);
    void PlayMilestoneClick();
    void PlaySnapChime(float intensity);
    void StopAll();
}
