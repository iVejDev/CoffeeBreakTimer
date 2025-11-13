using CoffeeBreakTimer.Core.Domain.Enums;

namespace CoffeeBreakTimer.Core.Domain;

// Den här klassen kommer användas av timerlogiken
public class Session
{
    public SessionType Type { get; set; }
    public TimeSpan Duration { get; set; }
}
