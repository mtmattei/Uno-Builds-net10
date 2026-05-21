#if __ANDROID__
using Android.Media;
#endif

namespace OrbFidget.Services;

public class OrbAudioEngine : IOrbAudioEngine
{
    private static readonly float[] PentatonicFreqs = { 261.6f, 329.6f, 392f, 523.3f, 659.3f, 784f };
    private static readonly Random _rng = new();
    private const int SampleRate = 44100;
    private const int MaxConcurrentSounds = 6;

#if __ANDROID__
    private SoundPool? _pool;
    private readonly Dictionary<int, int> _loadedSounds = new();
    private int _nextId;
    private bool _initialized;

    private void EnsureInitialized()
    {
        if (_initialized) return;
        _initialized = true;

        try
        {
            var attrs = new AudioAttributes.Builder()!
                .SetUsage(AudioUsageKind.Game)!
                .SetContentType(AudioContentType.Sonification)!
                .Build();

            _pool = new SoundPool.Builder()!
                .SetMaxStreams(MaxConcurrentSounds)!
                .SetAudioAttributes(attrs)!
                .Build();

            // Pre-generate common sounds
            PreloadSound("stretch_low", 300f, 0.06f, 0.03f, WaveType.Sine);
            PreloadSound("stretch_mid", 500f, 0.06f, 0.03f, WaveType.Sine);
            PreloadSound("stretch_high", 700f, 0.06f, 0.03f, WaveType.Sine);
            PreloadSound("click_hi", 1200f, 0.04f, 0.025f, WaveType.Square);
            PreloadSound("click_lo", 600f, 0.06f, 0.018f, WaveType.Triangle);
            PreloadSound("snap_click", 1800f, 0.03f, 0.02f, WaveType.Square);

            foreach (var freq in PentatonicFreqs)
            {
                PreloadSound($"chime_{freq:F0}", freq, 0.5f, 0.06f, WaveType.Sine);
                PreloadSound($"oct_{freq:F0}", freq * 2f, 0.3f, 0.025f, WaveType.Sine);
            }
        }
        catch
        {
            _pool = null;
        }
    }

    private void PreloadSound(string key, float freq, float durSec, float volume, WaveType wave)
    {
        if (_pool == null) return;

        try
        {
            int sampleCount = (int)(SampleRate * durSec);
            byte[] wav = GenerateWav(freq, sampleCount, volume, wave);

            var tempPath = System.IO.Path.Combine(
                Android.App.Application.Context.CacheDir!.AbsolutePath,
                $"orb_snd_{_nextId++}.wav");

            System.IO.File.WriteAllBytes(tempPath, wav);
            int soundId = _pool.Load(tempPath, 1);
            _loadedSounds[key.GetHashCode()] = soundId;

            // Clean up temp file after a short delay
            Task.Delay(2000).ContinueWith(_ =>
            {
                try { System.IO.File.Delete(tempPath); } catch { }
            });
        }
        catch { }
    }

    private void PlayLoaded(string key, float volume)
    {
        if (_pool == null) return;

        if (_loadedSounds.TryGetValue(key.GetHashCode(), out int soundId))
        {
            float vol = Math.Clamp(volume * 10f, 0f, 1f);
            _pool.Play(soundId, vol, vol, 1, 0, 1f);
        }
    }

    public void PlayStretchTone(float distance)
    {
        EnsureInitialized();
        float vol = 0.02f + MathF.Min(distance / 500f, 0.03f);

        if (distance < 150f)
            PlayLoaded("stretch_low", vol);
        else if (distance < 300f)
            PlayLoaded("stretch_mid", vol);
        else
            PlayLoaded("stretch_high", vol);
    }

    public void PlayMilestoneClick()
    {
        EnsureInitialized();
        PlayLoaded("click_hi", 0.025f);
        PlayLoaded("click_lo", 0.018f);
    }

    public void PlaySnapChime(float intensity)
    {
        EnsureInitialized();
        float freq = PentatonicFreqs[_rng.Next(PentatonicFreqs.Length)];
        float volume = 0.04f + intensity * 0.04f;

        PlayLoaded($"chime_{freq:F0}", volume);
        PlayLoaded($"oct_{freq:F0}", volume * 0.35f);
        PlayLoaded("snap_click", 0.02f);
    }

    public void StopAll()
    {
        _pool?.AutoPause();
    }
#else
    private readonly Queue<MediaPlayer> _activePlayers = new();

    public void PlayStretchTone(float distance)
    {
        float freq = 200f + distance * 3f;
        float volume = 0.02f + MathF.Min(distance / 500f, 0.03f);
        float duration = 0.06f;
        PlayTone(freq, duration, volume, WaveType.Sine);
    }

    public void PlayMilestoneClick()
    {
        PlayTone(1200f, 0.04f, 0.025f, WaveType.Square);
        PlayTone(600f, 0.06f, 0.018f, WaveType.Triangle);
    }

    public void PlaySnapChime(float intensity)
    {
        float freq = PentatonicFreqs[_rng.Next(PentatonicFreqs.Length)];
        float volume = 0.04f + intensity * 0.04f;

        PlayTone(freq, 0.5f, volume, WaveType.Sine);
        PlayTone(freq * 2f, 0.3f, volume * 0.35f, WaveType.Sine);
        PlayTone(freq * 0.5f, 0.55f, volume * 0.2f, WaveType.Sine);
        PlayTone(1800f, 0.03f, 0.02f, WaveType.Square);
    }

    public void StopAll()
    {
        while (_activePlayers.Count > 0)
        {
            var player = _activePlayers.Dequeue();
            try { player.Pause(); } catch { }
        }
    }

    private void PlayTone(float frequency, float durationSec, float volume, WaveType waveType)
    {
        try
        {
            while (_activePlayers.Count >= MaxConcurrentSounds && _activePlayers.Count > 0)
            {
                var old = _activePlayers.Dequeue();
                try { old.Pause(); } catch { }
            }

            int sampleCount = (int)(SampleRate * durationSec);
            byte[] wavData = GenerateWav(frequency, sampleCount, volume, waveType);

            var stream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
            var writer = new Windows.Storage.Streams.DataWriter(stream.GetOutputStreamAt(0));
            writer.WriteBytes(wavData);
            _ = writer.StoreAsync().AsTask().ContinueWith(t =>
            {
                try { _ = stream.FlushAsync(); } catch { }
            });

            var player = new MediaPlayer();
            player.Source = Windows.Media.Core.MediaSource.CreateFromStream(stream, "audio/wav");
            player.Volume = Math.Clamp(volume * 10, 0, 1);
            player.Play();

            _activePlayers.Enqueue(player);
        }
        catch
        {
            // Audio is best-effort
        }
    }
#endif

    private static byte[] GenerateWav(float frequency, int sampleCount, float volume, WaveType waveType)
    {
        int dataSize = sampleCount * 2;
        byte[] wav = new byte[44 + dataSize];

        WriteString(wav, 0, "RIFF");
        WriteInt32(wav, 4, 36 + dataSize);
        WriteString(wav, 8, "WAVE");
        WriteString(wav, 12, "fmt ");
        WriteInt32(wav, 16, 16);
        WriteInt16(wav, 20, 1);
        WriteInt16(wav, 22, 1);
        WriteInt32(wav, 24, SampleRate);
        WriteInt32(wav, 28, SampleRate * 2);
        WriteInt16(wav, 32, 2);
        WriteInt16(wav, 34, 16);
        WriteString(wav, 36, "data");
        WriteInt32(wav, 40, dataSize);

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            float envelope = MathF.Exp(-t * 8f);
            float phase = 2f * MathF.PI * frequency * t;

            float sample = waveType switch
            {
                WaveType.Sine => MathF.Sin(phase),
                WaveType.Square => MathF.Sin(phase) >= 0 ? 1f : -1f,
                WaveType.Triangle => 2f * MathF.Abs(2f * (t * frequency - MathF.Floor(t * frequency + 0.5f))) - 1f,
                _ => MathF.Sin(phase),
            };

            short value = (short)(sample * envelope * volume * 32767f);
            wav[44 + i * 2] = (byte)(value & 0xFF);
            wav[44 + i * 2 + 1] = (byte)((value >> 8) & 0xFF);
        }

        return wav;
    }

    private static void WriteString(byte[] data, int offset, string value)
    {
        for (int i = 0; i < value.Length; i++)
            data[offset + i] = (byte)value[i];
    }

    private static void WriteInt32(byte[] data, int offset, int value)
    {
        data[offset] = (byte)(value & 0xFF);
        data[offset + 1] = (byte)((value >> 8) & 0xFF);
        data[offset + 2] = (byte)((value >> 16) & 0xFF);
        data[offset + 3] = (byte)((value >> 24) & 0xFF);
    }

    private static void WriteInt16(byte[] data, int offset, short value)
    {
        data[offset] = (byte)(value & 0xFF);
        data[offset + 1] = (byte)((value >> 8) & 0xFF);
    }

    private enum WaveType
    {
        Sine,
        Square,
        Triangle,
    }
}
