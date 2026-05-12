using CoffeeBreakTimer.Core.Domain.Enums;

namespace CoffeeBreakTimer.Core.Domain;

public sealed record Session(SessionType Type, TimeSpan Duration);
