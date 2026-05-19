namespace Fx.Sandbox.Application.Dtos;

public sealed record AccountSummaryDto(
    decimal Cash,
    decimal RealisedPnL,
    decimal UnrealisedPnL,
    decimal Nav);
