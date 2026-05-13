using CoffeeBreakTimer.Core.Domain.Enums;
using CoffeeBreakTimer.Core.Interfaces;
using System.Diagnostics;

#if WINDOWS
using Windows.Media.Core;
using Windows.Media.Playback;
#endif

namespace CoffeeBreakTimer.App.Services;

public sealed class MauiAmbiencePlayer : IAmbiencePlayer, IDisposable
{
    private const string RainUrl = "https://upload.wikimedia.org/wikipedia/commons/transcoded/1/15/Sound_of_light_rainfall.ogg/Sound_of_light_rainfall.ogg.mp3";
    private const string ChillUrl = "https://upload.wikimedia.org/wikipedia/commons/transcoded/5/53/Placid_Ambient_by_MusicLFiles.ogg/Placid_Ambient_by_MusicLFiles.ogg.mp3";

    private static readonly HttpClient HttpClient = new();

    private double _volume = 0.32;

#if WINDOWS
    private readonly Dictionary<AmbienceTrack, MediaPlayer> _players = [];
    private readonly Dictionary<AmbienceTrack, CancellationTokenSource> _downloads = [];
#endif

    public void SetEnabled(AmbienceTrack track, bool isEnabled)
    {
        if (isEnabled)
        {
            _ = PlayAsync(track);
            return;
        }

        Stop(track);
    }

    public void SetVolume(double volume)
    {
        _volume = Math.Clamp(volume, 0, 1);

#if WINDOWS
        foreach (var player in _players.Values)
        {
            player.Volume = _volume;
        }
#endif
    }

    public void StopAll()
    {
#if WINDOWS
        foreach (var player in _players.Values)
        {
            player.Pause();
            player.Dispose();
        }

        foreach (var download in _downloads.Values)
        {
            download.Cancel();
            download.Dispose();
        }

        _players.Clear();
        _downloads.Clear();
#endif
    }

    public void Dispose()
    {
        StopAll();
    }

    private async Task PlayAsync(AmbienceTrack track)
    {
#if WINDOWS
        if (_players.TryGetValue(track, out var existingPlayer))
        {
            existingPlayer.Volume = _volume;
            existingPlayer.Play();
            return;
        }

        _downloads[track] = new CancellationTokenSource();
        var cancellationToken = _downloads[track].Token;

        try
        {
            var localPath = await GetCachedTrackPathAsync(track, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var player = new MediaPlayer
            {
                Source = MediaSource.CreateFromUri(new Uri(localPath)),
                Volume = _volume,
                IsLoopingEnabled = true
            };

            player.MediaFailed += (_, args) =>
            {
                Debug.WriteLine($"Ambience playback failed for {track}: {args.ErrorMessage}");
                player.Dispose();
                _players.Remove(track);
            };

            _players[track] = player;
            player.Play();
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ambience download/playback failed for {track}: {ex}");
        }
        finally
        {
            _downloads[track].Dispose();
            _downloads.Remove(track);
        }
#else
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        await Task.CompletedTask;
#endif
    }

    private void Stop(AmbienceTrack track)
    {
#if WINDOWS
        if (_downloads.Remove(track, out var download))
        {
            download.Cancel();
            download.Dispose();
        }

        if (!_players.Remove(track, out var player))
        {
            return;
        }

        player.Pause();
        player.Dispose();
#endif
    }

    private static async Task<string> GetCachedTrackPathAsync(AmbienceTrack track, CancellationToken cancellationToken)
    {
        var directory = Path.Combine(FileSystem.AppDataDirectory, "ambience");
        Directory.CreateDirectory(directory);

        var path = Path.Combine(directory, GetTrackFileName(track));

        if (File.Exists(path) && new FileInfo(path).Length > 0)
        {
            return path;
        }

        await using var remoteStream = await HttpClient.GetStreamAsync(GetTrackUrl(track), cancellationToken);
        await using var localStream = File.Create(path);
        await remoteStream.CopyToAsync(localStream, cancellationToken);
        return path;
    }

    private static string GetTrackUrl(AmbienceTrack track)
    {
        return track switch
        {
            AmbienceTrack.Rain => RainUrl,
            AmbienceTrack.Chill => ChillUrl,
            _ => throw new ArgumentOutOfRangeException(nameof(track), track, null)
        };
    }

    private static string GetTrackFileName(AmbienceTrack track)
    {
        return track switch
        {
            AmbienceTrack.Rain => "ambient_rain.mp3",
            AmbienceTrack.Chill => "ambient_chill.mp3",
            _ => throw new ArgumentOutOfRangeException(nameof(track), track, null)
        };
    }
}
