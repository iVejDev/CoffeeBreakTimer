using CoffeeBreakTimer.Core.Interfaces;

namespace CoffeeBreakTimer.App.Services;

public sealed class MauiAudioPlayer : IAudioPlayer
{
    public void Play(string soundName)
    {
#if WINDOWS
        Console.Beep(880, 180);
#elif ANDROID
        using var tone = new Android.Media.ToneGenerator(Android.Media.Stream.Notification, 80);
        tone.StartTone(Android.Media.Tone.PropBeep, 250);
#elif IOS || MACCATALYST
        AudioToolbox.AudioServices.PlaySystemSound(1007);
#else
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
#endif
    }
}
