namespace Fx.Sandbox.Domain;

public sealed record Rate(CurrencyPair Pair, decimal Mid, DateTimeOffset Timestamp);
