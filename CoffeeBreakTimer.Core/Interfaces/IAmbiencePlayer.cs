using CoffeeBreakTimer.Core.Domain.Enums;

namespace CoffeeBreakTimer.Core.Interfaces;

public interface IAmbiencePlayer
{
    void SetEnabled(AmbienceTrack track, bool isEnabled);

    void SetVolume(double volume);

    void StopAll();
}
