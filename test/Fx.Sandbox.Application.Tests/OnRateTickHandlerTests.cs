using Fx.Sandbox.Application.Abstractions;
using Fx.Sandbox.Application.Handlers;
using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Application.Tests;

public class OnRateTickHandlerTests
{
    private sealed class StubClock : IClock { public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.UtcNow; }

    [Fact]
    public async Task Matches_eligible_orders_creates_trade_and_updates_position()
    {
        var order = new LimitOrder(Guid.NewGuid(), CurrencyPair.EurUsd, OrderSide.Buy, 100m, 0.92m, DateTimeOffset.UtcNow);
        var orders = Substitute.For<IOrderRepository>();
        orders.ListOpenAsync(CurrencyPair.EurUsd, Arg.Any<CancellationToken>())
              .Returns(new List<LimitOrder> { order });
        var positions = Substitute.For<IPositionRepository>();
        var position = new Position(CurrencyPair.EurUsd);
        positions.GetOrCreateAsync(CurrencyPair.EurUsd, Arg.Any<CancellationToken>()).Returns(position);
        var accounts = Substitute.For<IAccountRepository>();
        var trades = Substitute.For<ITradeRepository>();
        var clock = new StubClock();
        var sut = new OnRateTickHandler(orders, positions, accounts, trades, clock);

        await sut.HandleAsync(new Rate(CurrencyPair.EurUsd, 0.91m, clock.UtcNow));

        order.Status.Should().Be(OrderStatus.Filled);
        order.FillPrice.Should().Be(0.91m);
        position.Quantity.Should().Be(100m);
        position.AveragePrice.Should().Be(0.91m);
        await trades.Received(1).AddAsync(Arg.Any<Trade>(), Arg.Any<CancellationToken>());
        await positions.Received(1).UpsertAsync(position, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Skips_orders_not_meeting_limit_price()
    {
        var order = new LimitOrder(Guid.NewGuid(), CurrencyPair.EurUsd, OrderSide.Buy, 100m, 0.90m, DateTimeOffset.UtcNow);
        var orders = Substitute.For<IOrderRepository>();
        orders.ListOpenAsync(CurrencyPair.EurUsd, Arg.Any<CancellationToken>())
              .Returns(new List<LimitOrder> { order });
        var sut = new OnRateTickHandler(
            orders,
            Substitute.For<IPositionRepository>(),
            Substitute.For<IAccountRepository>(),
            Substitute.For<ITradeRepository>(),
            new StubClock());

        await sut.HandleAsync(new Rate(CurrencyPair.EurUsd, 0.92m, DateTimeOffset.UtcNow));

        order.Status.Should().Be(OrderStatus.Open);
        await orders.DidNotReceive().UpdateAsync(Arg.Any<LimitOrder>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task No_open_orders_is_noop()
    {
        var orders = Substitute.For<IOrderRepository>();
        orders.ListOpenAsync(Arg.Any<CurrencyPair>(), Arg.Any<CancellationToken>()).Returns(new List<LimitOrder>());
        var positions = Substitute.For<IPositionRepository>();
        var sut = new OnRateTickHandler(orders, positions, Substitute.For<IAccountRepository>(), Substitute.For<ITradeRepository>(), new StubClock());

        await sut.HandleAsync(new Rate(CurrencyPair.EurUsd, 0.91m, DateTimeOffset.UtcNow));

        await positions.DidNotReceive().GetOrCreateAsync(Arg.Any<CurrencyPair>(), Arg.Any<CancellationToken>());
    }
}
