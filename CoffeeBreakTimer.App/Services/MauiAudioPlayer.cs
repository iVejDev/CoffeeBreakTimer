using CoffeeBreakTimer.Core.Interfaces;
using System.Text;

#if WINDOWS
using Windows.Media.Core;
using Windows.Media.Playback;
#endif

namespace CoffeeBreakTimer.App.Services;

public sealed class MauiAudioPlayer : IAudioPlayer
{
#if WINDOWS
    private static readonly List<MediaPlayer> ActivePlayers = [];
#endif

    public void Play(string soundName)
    {
#if WINDOWS
        PlayWindowsDing();
#elif ANDROID
        using var tone = new Android.Media.ToneGenerator(Android.Media.Stream.Notification, 70);
        tone.StartTone(Android.Media.Tone.PropBeep2, 220);
#elif IOS || MACCATALYST
        AudioToolbox.AudioServices.PlaySystemSound(1007);
#else
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
#endif
    }

#if WINDOWS
    private static void PlayWindowsDing()
    {
        var path = EnsureDingFile();
        var player = new MediaPlayer
        {
            Source = MediaSource.CreateFromUri(new Uri(path)),
            Volume = 0.55
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
#endif
}
