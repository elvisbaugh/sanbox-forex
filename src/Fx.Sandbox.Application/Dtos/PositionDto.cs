using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Application.Dtos;

public sealed record PositionDto(
    CurrencyPair Pair,
    decimal Quantity,
    decimal AveragePrice,
    decimal MarkPrice,
    decimal UnrealisedPnL,
    decimal RealisedPnL);
