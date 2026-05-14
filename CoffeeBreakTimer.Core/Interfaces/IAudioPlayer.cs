namespace CoffeeBreakTimer.Core.Interfaces;

public interface IAudioPlayer
{
    bool IsEnabled { get; set; }

    void Play(string soundName);
}
