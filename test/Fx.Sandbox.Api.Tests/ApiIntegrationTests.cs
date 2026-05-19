using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fx.Sandbox.Application.Abstractions;
using Fx.Sandbox.Application.Abstractions.Handlers;
using Fx.Sandbox.Application.Dtos;
using Fx.Sandbox.Application.Handlers;
using Fx.Sandbox.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Fx.Sandbox.Api.Tests;

public class ApiIntegrationTests : IClassFixture<ApiIntegrationTests.Factory>
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public sealed class Factory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // "Testing" environment disables the background tick service so tests can
            // drive the tick manually.
            builder.UseEnvironment("Testing");
        }
    }

    private readonly Factory _factory;
    private readonly HttpClient _client;

    public ApiIntegrationTests(Factory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Healthz_returns_ok()
    {
        var r = await _client.GetAsync("/healthz");
        r.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Rates_returns_three_pairs()
    {
        var rates = await _client.GetFromJsonAsync<List<RateDto>>("/api/v1/rates", JsonOpts);
        rates.Should().NotBeNull().And.HaveCount(3);
    }

    [Fact]
    public async Task Place_then_tick_fills_order_and_creates_position()
    {
        var seedMid = (await _client.GetFromJsonAsync<List<RateDto>>("/api/v1/rates", JsonOpts))!
                      .First(r => r.Pair == CurrencyPair.EurUsd).Mid;

        // Buy at a price well above current mid → should fill on next tick.
        var place = new PlaceOrderRequest(CurrencyPair.EurUsd, OrderSide.Buy, 100m, seedMid * 2m);
        var placeRes = await _client.PostAsJsonAsync("/api/v1/orders", place, JsonOpts);
        placeRes.StatusCode.Should().Be(HttpStatusCode.Created);

        await DriveTickAsync(CurrencyPair.EurUsd);

        var positions = await _client.GetFromJsonAsync<List<PositionDto>>("/api/v1/positions", JsonOpts);
        positions.Should().ContainSingle(p => p.Pair == CurrencyPair.EurUsd && p.Quantity == 100m);
    }

    [Fact]
    public async Task Validation_rejects_negative_quantity()
    {
        var bad = new PlaceOrderRequest(CurrencyPair.EurUsd, OrderSide.Buy, -1m, 1m);
        var res = await _client.PostAsJsonAsync("/api/v1/orders", bad, JsonOpts);
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Cancel_open_order_transitions_to_cancelled()
    {
        var place = new PlaceOrderRequest(CurrencyPair.GbpUsd, OrderSide.Sell, 50m, 999m); // far from market
        var created = await (await _client.PostAsJsonAsync("/api/v1/orders", place, JsonOpts))
                            .Content.ReadFromJsonAsync<OrderDto>(JsonOpts);
        created.Should().NotBeNull();

        var del = await _client.DeleteAsync($"/api/v1/orders/{created!.Id}");
        del.StatusCode.Should().Be(HttpStatusCode.OK);
        var cancelled = await del.Content.ReadFromJsonAsync<OrderDto>(JsonOpts);
        cancelled!.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task Modify_open_order_updates_price_and_qty()
    {
        var place = new PlaceOrderRequest(CurrencyPair.UsdChf, OrderSide.Sell, 50m, 999m);
        var created = await (await _client.PostAsJsonAsync("/api/v1/orders", place, JsonOpts))
                            .Content.ReadFromJsonAsync<OrderDto>(JsonOpts);

        var mod = new ModifyOrderRequest(75m, 998m);
        var res = await _client.PatchAsJsonAsync($"/api/v1/orders/{created!.Id}", mod, JsonOpts);
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await res.Content.ReadFromJsonAsync<OrderDto>(JsonOpts);
        updated!.Quantity.Should().Be(75m);
        updated.LimitPrice.Should().Be(998m);
    }

    [Fact]
    public async Task Unknown_pair_history_returns_404()
    {
        var r = await _client.GetAsync("/api/v1/rates/USDJPY/history");
        r.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task DriveTickAsync(CurrencyPair pair)
    {
        // Manually advance + match (no background service in Testing env).
        using var scope = _factory.Services.CreateScope();
        var feed = _factory.Services.GetRequiredService<IRateFeed>();
        var history = _factory.Services.GetRequiredService<IRateHistory>();
        var onTick = _factory.Services.GetRequiredService<IOnRateTickHandler>();
        var rate = feed.Advance(pair);
        history.Append(rate);
        await onTick.HandleAsync(rate);
    }
}
