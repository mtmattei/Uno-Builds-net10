#if __ANDROID__
using Android.Content;
using Android.OS;
using Android.Views;
#else
using Windows.Devices.Haptics;
#endif

namespace OrbFidget.Services;

public class OrbHapticService : IOrbHapticService
{
#if __ANDROID__
    private Vibrator? _vibrator;

    public Task InitializeAsync()
    {
        try
        {
            var context = Android.App.Application.Context;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
            {
                var manager = (VibratorManager?)context.GetSystemService(Context.VibratorManagerService);
                _vibrator = manager?.DefaultVibrator;
            }

            _vibrator ??= (Vibrator?)context.GetSystemService(Context.VibratorService);
        }
        catch
        {
            _vibrator = null;
        }
        return Task.CompletedTask;
    }

    public void OnGrab()
    {
        TryPredefined(VibrationEffect.EffectClick);
    }

    public void OnMilestone(int index)
    {
        TryPredefined(VibrationEffect.EffectTick);
    }

    public void OnRelease(float intensity)
    {
        if (intensity > 0.6f)
            TryPredefined(VibrationEffect.EffectHeavyClick);
        else
            TryPredefined(VibrationEffect.EffectClick);
    }

    private void TryPredefined(int effectId)
    {
        if (_vibrator == null || !_vibrator.HasVibrator) return;

        try
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                _vibrator.Vibrate(VibrationEffect.CreatePredefined(effectId));
            }
            else if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                _vibrator.Vibrate(VibrationEffect.CreateOneShot(50, VibrationEffect.DefaultAmplitude));
            }
            else
            {
#pragma warning disable CA1422
                _vibrator.Vibrate(50);
#pragma warning restore CA1422
            }
        }
        catch
        {
            // Silently ignore
        }
    }
#else
    private SimpleHapticsController? _controller;

    public async Task InitializeAsync()
    {
        try
        {
            var device = await VibrationDevice.GetDefaultAsync();
            if (device != null)
            {
                _controller = device.SimpleHapticsController;
            }
        }
        catch
        {
            _controller = null;
        }
    }

    public void OnGrab()
    {
        TryBuzz(TimeSpan.FromMilliseconds(10));
    }

    public void OnMilestone(int index)
    {
        int durationMs = 8 + index * 6;
        TryBuzz(TimeSpan.FromMilliseconds(durationMs));
    }

    public void OnRelease(float intensity)
    {
        int durationMs = intensity > 0.6f ? 30 : 18;
        TryBuzz(TimeSpan.FromMilliseconds(durationMs));
    }

    private void TryBuzz(TimeSpan duration)
    {
        if (_controller == null) return;

        try
        {
            var feedback = _controller.SupportedFeedback.FirstOrDefault();
            if (feedback != null)
            {
                _controller.SendHapticFeedbackForDuration(feedback, 1.0, duration);
            }
        }
        catch
        {
            // Silently ignore on unsupported platforms
        }
    }
#endif
}
