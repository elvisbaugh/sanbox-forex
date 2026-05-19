using Fx.Sandbox.Application.Abstractions;
using Fx.Sandbox.Application.Handlers;
using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Application.Tests;

public class ClosePositionHandlerTests
{
    private sealed class StubClock : IClock { public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.UtcNow; }

    private sealed class StubRates : IRateFeed
    {
        private readonly decimal _mid;
        public StubRates(decimal mid) { _mid = mid; }
        public Rate CurrentRate(CurrencyPair pair) => new(pair, _mid, DateTimeOffset.UtcNow);
        public IReadOnlyList<Rate> Snapshot() => Array.Empty<Rate>();
        public Rate Advance(CurrencyPair pair) => CurrentRate(pair);
    }

    [Fact]
    public async Task Closes_long_position_with_sell_at_current_mid_and_realises_pnl()
    {
        var orders = Substitute.For<IOrderRepository>();
        var positions = Substitute.For<IPositionRepository>();
        var trades = Substitute.For<ITradeRepository>();
        var accounts = Substitute.For<IAccountRepository>();
        var account = new Account(10_000m);
        accounts.GetAsync(Arg.Any<CancellationToken>()).Returns(account);
        var position = new Position(CurrencyPair.EurUsd);
        PositionCalculatorService.ApplyFill(position, OrderSide.Buy, 100m, 0.90m); // open long 100 @ 0.90
        positions.GetOrCreateAsync(CurrencyPair.EurUsd, Arg.Any<CancellationToken>()).Returns(position);
        var sut = new ClosePositionHandler(positions, orders, trades, accounts, new StubRates(0.95m), new StubClock());

        var dto = await sut.HandleAsync(CurrencyPair.EurUsd);

        dto.Should().NotBeNull();
        dto!.Side.Should().Be(OrderSide.Sell);
        dto.Quantity.Should().Be(100m);
        dto.Status.Should().Be(OrderStatus.Filled);
        dto.FillPrice.Should().Be(0.95m);
        position.IsFlat.Should().BeTrue();
        position.RealisedPnL.Should().Be((0.95m - 0.90m) * 100m);
        account.Cash.Should().Be(10_000m + (0.95m - 0.90m) * 100m);
        account.RealisedPnL.Should().Be((0.95m - 0.90m) * 100m);
        await orders.Received(1).AddAsync(Arg.Any<LimitOrder>(), Arg.Any<CancellationToken>());
        await trades.Received(1).AddAsync(Arg.Any<Trade>(), Arg.Any<CancellationToken>());
        await positions.Received(1).UpsertAsync(position, Arg.Any<CancellationToken>());
        await accounts.Received(1).UpdateAsync(account, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Closes_short_position_with_buy_at_current_mid()
    {
        var orders = Substitute.For<IOrderRepository>();
        var positions = Substitute.For<IPositionRepository>();
        var trades = Substitute.For<ITradeRepository>();
        var accounts = Substitute.For<IAccountRepository>();
        accounts.GetAsync(Arg.Any<CancellationToken>()).Returns(new Account(10_000m));
        var position = new Position(CurrencyPair.GbpUsd);
        PositionCalculatorService.ApplyFill(position, OrderSide.Sell, 50m, 0.80m); // open short 50 @ 0.80
        positions.GetOrCreateAsync(CurrencyPair.GbpUsd, Arg.Any<CancellationToken>()).Returns(position);
        var sut = new ClosePositionHandler(positions, orders, trades, accounts, new StubRates(0.78m), new StubClock());

        var dto = await sut.HandleAsync(CurrencyPair.GbpUsd);

        dto!.Side.Should().Be(OrderSide.Buy);
        dto.Quantity.Should().Be(50m);
        position.IsFlat.Should().BeTrue();
        position.RealisedPnL.Should().Be((0.80m - 0.78m) * 50m);
    }

    [Fact]
    public async Task Returns_null_when_position_is_flat()
    {
        var orders = Substitute.For<IOrderRepository>();
        var positions = Substitute.For<IPositionRepository>();
        var trades = Substitute.For<ITradeRepository>();
        var accounts = Substitute.For<IAccountRepository>();
        positions.GetOrCreateAsync(CurrencyPair.UsdChf, Arg.Any<CancellationToken>())
                 .Returns(new Position(CurrencyPair.UsdChf));
        var sut = new ClosePositionHandler(positions, orders, trades, accounts, new StubRates(0.91m), new StubClock());

        var dto = await sut.HandleAsync(CurrencyPair.UsdChf);

        dto.Should().BeNull();
        await orders.DidNotReceive().AddAsync(Arg.Any<LimitOrder>(), Arg.Any<CancellationToken>());
        await trades.DidNotReceive().AddAsync(Arg.Any<Trade>(), Arg.Any<CancellationToken>());
    }
}
