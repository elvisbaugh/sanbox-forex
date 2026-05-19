using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Fx.Sandbox.Application.Abstractions.Handlers;
using Fx.Sandbox.Application.Dtos;

namespace Fx.Sandbox.Api.Controllers;

[ApiController]
[Route("api/v1/orders")]
public class OrdersController : ControllerBase
{
    private readonly IGetOrderBookQuery _book;
    private readonly IPlaceLimitOrderHandler _place;
    private readonly ICancelOrderHandler _cancel;
    private readonly IModifyOrderHandler _modify;
    private readonly IValidator<PlaceOrderRequest> _placeValidator;
    private readonly IValidator<ModifyOrderRequest> _modifyValidator;

    public OrdersController(
        IGetOrderBookQuery book,
        IPlaceLimitOrderHandler place,
        ICancelOrderHandler cancel,
        IModifyOrderHandler modify,
        IValidator<PlaceOrderRequest> placeValidator,
        IValidator<ModifyOrderRequest> modifyValidator)
    {
        _book = book;
        _place = place;
        _cancel = cancel;
        _modify = modify;
        _placeValidator = placeValidator;
        _modifyValidator = modifyValidator;
    }
    

    /// <summary>
    /// All orders (open, filled, cancelled), newest first.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> List(CancellationToken cancellationToken) =>
        Ok(await _book.HandleAsync(cancellationToken));

    
    /// <summary>
    /// Place a limit order.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> Place([FromBody] PlaceOrderRequest request, CancellationToken cancellationToken)
    {
        var validation = await _placeValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return ValidationProblem(ToModelState(validation));
        var order = await _place.HandleAsync(request, cancellationToken);
        return CreatedAtAction(nameof(List), new { }, order);
    }

    /// <summary>
    /// Cancel an open order.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrderDto>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _cancel.HandleAsync(id, cancellationToken);
            return order is null ? NotFound() : Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(statusCode: 409, title: "Cannot cancel order", detail: ex.Message);
        }
    }


    /// <summary>
    /// Modify an open order's quantity and/or limit price.
    /// </summary>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrderDto>> Modify(Guid id, [FromBody] ModifyOrderRequest request, CancellationToken cancellationToken)
    {
        var validation = await _modifyValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return ValidationProblem(ToModelState(validation));
        try
        {
            var order = await _modify.HandleAsync(id, request, cancellationToken);
            return order is null ? NotFound() : Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(statusCode: 409, title: "Cannot modify order", detail: ex.Message);
        }
    }

    private static ModelStateDictionary ToModelState(FluentValidation.Results.ValidationResult result)
    {
        var modelState = new ModelStateDictionary();
        foreach (var error in result.Errors) modelState.AddModelError(error.PropertyName, error.ErrorMessage);
        return modelState;
    }
}
