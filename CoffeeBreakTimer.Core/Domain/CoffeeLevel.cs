
namespace CoffeeBreakTimer.Core.Domain;


// UI kan alltid lita på att nivån är korrekt, och tjänster i domänen slipper uppfinna samma validering om och om igen.
public class CoffeeLevel
{
    public double Value { get; }

    public CoffeeLevel(double value)
    {
        value = Math.Clamp(value, 0.0, 1.0);
    }
}
