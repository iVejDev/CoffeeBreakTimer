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

    private double _volume = 0.55;

#if WINDOWS
    private readonly Dictionary<AmbienceTrack, MediaPlayer> _players = [];
#endif

    public void SetEnabled(AmbienceTrack track, bool isEnabled)
    {
        if (isEnabled)
        {
            Play(track);
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

        _players.Clear();
#endif
    }

    public void Dispose()
    {
        StopAll();
    }

    private void Play(AmbienceTrack track)
    {
#if WINDOWS
        if (_players.TryGetValue(track, out var existingPlayer))
        {
            existingPlayer.Volume = _volume;
            existingPlayer.Play();
            return;
        }

        try
        {
            var player = new MediaPlayer
            {
                Source = MediaSource.CreateFromUri(new Uri(GetTrackUrl(track))),
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
            Debug.WriteLine($"Ambience playback failed for {track}: {ex}");
        }
#else
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
#endif
    }

    private void Stop(AmbienceTrack track)
    {
#if WINDOWS
        if (!_players.Remove(track, out var player))
        {
            return;
        }

        player.Pause();
        player.Dispose();
#endif
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
}
