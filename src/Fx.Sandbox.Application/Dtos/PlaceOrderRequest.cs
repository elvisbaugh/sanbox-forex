using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Application.Dtos;

public sealed record PlaceOrderRequest(
    CurrencyPair Pair, 
    OrderSide Side, 
    decimal Quantity, 
    decimal LimitPrice);
