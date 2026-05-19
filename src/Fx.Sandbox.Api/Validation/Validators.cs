using FluentValidation;
using Fx.Sandbox.Application.Dtos;

namespace Fx.Sandbox.Api.Validation;

public sealed class PlaceOrderRequestValidator : AbstractValidator<PlaceOrderRequest>
{
    public PlaceOrderRequestValidator()
    {
        RuleFor(request => request.Quantity).GreaterThan(0).LessThanOrEqualTo(1_000_000);
        RuleFor(request => request.LimitPrice).GreaterThan(0).LessThanOrEqualTo(1000);
        RuleFor(request => request.Side).IsInEnum();
        RuleFor(request => request.Pair).IsInEnum();
    }
}

public sealed class ModifyOrderRequestValidator : AbstractValidator<ModifyOrderRequest>
{
    public ModifyOrderRequestValidator()
    {
        RuleFor(request => request.Quantity).GreaterThan(0).LessThanOrEqualTo(1_000_000);
        RuleFor(request => request.LimitPrice).GreaterThan(0).LessThanOrEqualTo(1000);
    }
}
