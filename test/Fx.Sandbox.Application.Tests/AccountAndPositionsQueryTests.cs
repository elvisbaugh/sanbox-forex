using Fx.Sandbox.Application.Abstractions;
using Fx.Sandbox.Application.Handlers;
using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Application.Tests;

public class AccountAndPositionsQueryTests
{
    private sealed class FakeRateFeed : IRateFeed
    {
        private readonly Dictionary<CurrencyPair, decimal> _mids;
        public FakeRateFeed(Dictionary<CurrencyPair, decimal> mids) => _mids = mids;
        public Rate CurrentRate(CurrencyPair pair) => new(pair, _mids[pair], DateTimeOffset.UtcNow);
        public IReadOnlyList<Rate> Snapshot() => _mids.Select(kv => new Rate(kv.Key, kv.Value, DateTimeOffset.UtcNow)).ToList();
        public Rate Advance(CurrencyPair pair) => CurrentRate(pair);
    }

    [Fact]
    public async Task Account_summary_includes_realised_and_unrealised_pnl()
    {
        var account = new Account(10_005m, realisedPnL: 5m); // 5 already settled into cash
        var accounts = Substitute.For<IAccountRepository>();
        accounts.GetAsync(Arg.Any<CancellationToken>()).Returns(account);

        var longEur = new Position(CurrencyPair.EurUsd);
        PositionCalculatorService.ApplyFill(longEur, OrderSide.Buy, 100m, 0.90m); // long 100 @ 0.90
        var closedGbp = new Position(CurrencyPair.GbpUsd);
        PositionCalculatorService.ApplyFill(closedGbp, OrderSide.Buy, 100m, 0.80m);
        PositionCalculatorService.ApplyFill(closedGbp, OrderSide.Sell, 100m, 0.85m); // realised 5

        var positions = Substitute.For<IPositionRepository>();
        positions.ListAsync(Arg.Any<CancellationToken>())
                 .Returns(new List<Position> { longEur, closedGbp });

        var rates = new FakeRateFeed(new()
        {
            [CurrencyPair.EurUsd] = 1.00m,
            [CurrencyPair.GbpUsd] = 0.79m,
            [CurrencyPair.UsdChf] = 0.91m,
        });

        var sut = new GetAccountSummaryQuery(accounts, positions, rates);
        var dto = await sut.HandleAsync();

        dto.Cash.Should().Be(10_005m);
        dto.RealisedPnL.Should().Be(5m);
        dto.UnrealisedPnL.Should().Be(10m); // (1.00-0.90)*100
        dto.Nav.Should().Be(10_015m);
    }

    [Fact]
    public async Task Positions_query_excludes_flat_positions_and_includes_mark_and_unrealised()
    {
        var flat = new Position(CurrencyPair.GbpUsd); // flat
        var openShort = new Position(CurrencyPair.UsdChf);
        PositionCalculatorService.ApplyFill(openShort, OrderSide.Sell, 50m, 0.92m);

        var positions = Substitute.For<IPositionRepository>();
        positions.ListAsync(Arg.Any<CancellationToken>())
                 .Returns(new List<Position> { flat, openShort });

        var rates = new FakeRateFeed(new()
        {
            [CurrencyPair.EurUsd] = 1.00m,
            [CurrencyPair.GbpUsd] = 0.79m,
            [CurrencyPair.UsdChf] = 0.90m,
        });

        var sut = new GetPositionsQuery(positions, rates);
        var result = await sut.HandleAsync();

        result.Should().HaveCount(1);
        var r = result[0];
        r.Pair.Should().Be(CurrencyPair.UsdChf);
        r.Quantity.Should().Be(-50m);
        r.MarkPrice.Should().Be(0.90m);
        r.UnrealisedPnL.Should().Be(50m * (0.92m - 0.90m));
    }
}
