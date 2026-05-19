using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Application.Dtos;

public sealed record RateDto(CurrencyPair Pair, decimal Mid, DateTimeOffset Timestamp);
