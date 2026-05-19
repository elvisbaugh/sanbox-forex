using Fx.Sandbox.Application.Abstractions;
using Fx.Sandbox.Application.Dtos;
using Fx.Sandbox.Application.Handlers;
using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Application.Tests;

public class OrderHandlerTests
{
    private static IClock FixedClock(DateTimeOffset t)
    {
        var c = Substitute.For<IClock>();
        c.UtcNow.Returns(t);
        return c;
    }

    [Fact]
    public async Task PlaceLimitOrder_persists_open_order_and_returns_dto()
    {
        var repo = Substitute.For<IOrderRepository>();
        var now = DateTimeOffset.UtcNow;
        var sut = new PlaceLimitOrderHandler(repo, FixedClock(now));

        var dto = await sut.HandleAsync(new PlaceOrderRequest(CurrencyPair.EurUsd, OrderSide.Buy, 100m, 0.92m));

        dto.Status.Should().Be(OrderStatus.Open);
        dto.Pair.Should().Be(CurrencyPair.EurUsd);
        dto.Quantity.Should().Be(100m);
        dto.LimitPrice.Should().Be(0.92m);
        dto.CreatedAt.Should().Be(now);
        await repo.Received(1).AddAsync(Arg.Any<LimitOrder>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CancelOrder_returns_null_when_order_missing()
    {
        var repo = Substitute.For<IOrderRepository>();
        repo.GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((LimitOrder?)null);
        var sut = new CancelOrderHandler(repo, FixedClock(DateTimeOffset.UtcNow));

        var result = await sut.HandleAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task CancelOrder_transitions_to_cancelled()
    {
        var order = new LimitOrder(Guid.NewGuid(), CurrencyPair.EurUsd, OrderSide.Buy, 100m, 0.92m, DateTimeOffset.UtcNow);
        var repo = Substitute.For<IOrderRepository>();
        repo.GetAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);
        var sut = new CancelOrderHandler(repo, FixedClock(DateTimeOffset.UtcNow));

        var result = await sut.HandleAsync(order.Id);

        result!.Status.Should().Be(OrderStatus.Cancelled);
        await repo.Received(1).UpdateAsync(order, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ModifyOrder_updates_qty_and_price()
    {
        var order = new LimitOrder(Guid.NewGuid(), CurrencyPair.EurUsd, OrderSide.Buy, 100m, 0.92m, DateTimeOffset.UtcNow);
        var repo = Substitute.For<IOrderRepository>();
        repo.GetAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);
        var sut = new ModifyOrderHandler(repo);

        var result = await sut.HandleAsync(order.Id, new ModifyOrderRequest(150m, 0.93m));

        result!.Quantity.Should().Be(150m);
        result.LimitPrice.Should().Be(0.93m);
    }
}
