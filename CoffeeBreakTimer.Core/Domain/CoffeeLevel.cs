namespace CoffeeBreakTimer.Core.Domain;

public sealed class CoffeeLevel
{
    public double Value { get; }

    public CoffeeLevel(double value)
    {
        Value = Math.Clamp(value, 0.0, 1.0);
    }

    public static CoffeeLevel Empty { get; } = new(0.0);

    public static CoffeeLevel Full { get; } = new(1.0);
}
