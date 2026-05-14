using CoffeeBreakTimer.Core.Interfaces;
using System.Text;

#if WINDOWS
using Windows.Media.Core;
using Windows.Media.Playback;
#endif

namespace CoffeeBreakTimer.App.Services;

public sealed class MauiAudioPlayer : IAudioPlayer
{
    private const string FocusEndSourcePageUrl = "https://pixabay.com/sound-effects/people-step-into-clarity-whisper-307972/";
    private const string BreakEndSourcePageUrl = "https://pixabay.com/sound-effects/now-that-is-a-nice-coffee-256074/";

    private static readonly IReadOnlyDictionary<string, AppSound> Sounds =
        new Dictionary<string, AppSound>(StringComparer.OrdinalIgnoreCase)
        {
            ["focus_end"] = new(
                "Step into clarity whisper",
                "focus_end_step_into_clarity_whisper.wav",
                FocusEndSourcePageUrl,
                null),
            ["break_end"] = new(
                "Now that is a nice coffee",
                "break_end_nice_coffee.wav",
                BreakEndSourcePageUrl,
                null),
            ["session_end"] = new(
                "Soft session chime",
                "session_end_soft_chime.mp3",
                null,
                null)
        };

#if WINDOWS
    private static readonly List<MediaPlayer> ActivePlayers = [];
#endif

    public bool IsEnabled { get; set; } = true;

    public void Play(string soundName)
    {
        if (!IsEnabled)
        {
            return;
        }

        var sound = ResolveSound(soundName);

#if WINDOWS
        PlayWindowsSound(sound);
#elif ANDROID
        using var tone = new Android.Media.ToneGenerator(Android.Media.Stream.Notification, 70);
        tone.StartTone(Android.Media.Tone.PropBeep2, 220);
#elif IOS || MACCATALYST
        AudioToolbox.AudioServices.PlaySystemSound(1007);
#else
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
#endif
    }

    private static AppSound ResolveSound(string soundName)
    {
        return Sounds.TryGetValue(soundName, out var sound)
            ? sound
            : Sounds["session_end"];
    }

#if WINDOWS
    private static void PlayWindowsSound(AppSound sound)
    {
        var localSoundPath = TryGetBundledSoundPath(sound.LocalFileName);

        if (localSoundPath is not null)
        {
            PlayWindowsMedia(new Uri(localSoundPath), fallBackToLocalDing: UsesGeneratedFallback(sound));
            return;
        }

        if (sound.RemoteFallbackUrl is not null &&
            Uri.TryCreate(sound.RemoteFallbackUrl, UriKind.Absolute, out var remoteUri))
        {
            PlayWindowsMedia(remoteUri, fallBackToLocalDing: true);
            return;
        }

        if (UsesGeneratedFallback(sound))
        {
            PlayGeneratedWindowsDing();
        }
    }

    private static string? TryGetBundledSoundPath(string fileName)
    {
        try
        {
            using var packageStream = FileSystem.OpenAppPackageFileAsync(fileName)
                .GetAwaiter()
                .GetResult();

            var directory = Path.Combine(FileSystem.AppDataDirectory, "sounds");
            Directory.CreateDirectory(directory);

            var path = Path.Combine(directory, fileName);

            if (File.Exists(path) && new FileInfo(path).Length == packageStream.Length)
            {
                return path;
            }

            using var localStream = File.Create(path);
            packageStream.CopyTo(localStream);
            return path;
        }
        catch (FileNotFoundException)
        {
            return null;
        }
        catch (DirectoryNotFoundException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static void PlayGeneratedWindowsDing()
    {
        var path = EnsureDingFile();
        PlayWindowsMedia(new Uri(path), fallBackToLocalDing: false);
    }

    private static void PlayWindowsMedia(Uri uri, bool fallBackToLocalDing)
    {
        var player = new MediaPlayer
        {
            Source = MediaSource.CreateFromUri(uri),
            Volume = 0.55
        };

        player.MediaFailed += (_, _) =>
        {
            player.Dispose();
            ActivePlayers.Remove(player);

            if (fallBackToLocalDing)
            {
                PlayGeneratedWindowsDing();
            }
        };

        player.MediaEnded += (_, _) =>
        {
            player.Dispose();
            ActivePlayers.Remove(player);
        };

        ActivePlayers.Add(player);
        player.Play();
    }

    private static string EnsureDingFile()
    {
        var directory = Path.Combine(FileSystem.AppDataDirectory, "sounds");
        Directory.CreateDirectory(directory);

        var path = Path.Combine(directory, "soft-ding.wav");

        if (!File.Exists(path))
        {
            WriteSoftDing(path);
        }

        return path;
    }

    private static void WriteSoftDing(string path)
    {
        const int sampleRate = 44100;
        const short channels = 1;
        const short bitsPerSample = 16;
        const double durationSeconds = 1.25;

        var sampleCount = (int)(sampleRate * durationSeconds);
        var samples = new short[sampleCount];

        for (var i = 0; i < sampleCount; i++)
        {
            var t = i / (double)sampleRate;
            var attack = Math.Min(1.0, t / 0.025);
            var decay = Math.Exp(-3.8 * t);
            var envelope = attack * decay;

            var bell = Math.Sin(2 * Math.PI * 1046.5 * t) * 0.34;
            var overtone = Math.Sin(2 * Math.PI * 1568.0 * t) * 0.18;
            var cup = Math.Sin(2 * Math.PI * 783.99 * t) * 0.12;
            var value = (bell + overtone + cup) * envelope;

            samples[i] = (short)Math.Clamp(value * short.MaxValue, short.MinValue, short.MaxValue);
        }

        using var stream = File.Create(path);
        using var writer = new BinaryWriter(stream);
        var byteRate = sampleRate * channels * bitsPerSample / 8;
        var dataLength = samples.Length * channels * bitsPerSample / 8;

        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(36 + dataLength);
        writer.Write(Encoding.ASCII.GetBytes("WAVE"));
        writer.Write(Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16);
        writer.Write((short)1);
        writer.Write(channels);
        writer.Write(sampleRate);
        writer.Write(byteRate);
        writer.Write((short)(channels * bitsPerSample / 8));
        writer.Write(bitsPerSample);
        writer.Write(Encoding.ASCII.GetBytes("data"));
        writer.Write(dataLength);

        foreach (var sample in samples)
        {
            writer.Write(sample);
        }
    }

    private static bool UsesGeneratedFallback(AppSound sound)
    {
        return string.Equals(sound.LocalFileName, "session_end_soft_chime.mp3", StringComparison.OrdinalIgnoreCase);
    }
#endif

    private sealed record AppSound(
        string DisplayName,
        string LocalFileName,
        string? SourcePageUrl,
        string? RemoteFallbackUrl);
}
