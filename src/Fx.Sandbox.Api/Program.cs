using System.Threading.RateLimiting;
using FluentValidation;
using Fx.Sandbox.Api.Middleware;
using Fx.Sandbox.Api.Validation;
using Fx.Sandbox.Application.Abstractions;
using Fx.Sandbox.Application.Abstractions.Handlers;
using Fx.Sandbox.Application.Dtos;
using Fx.Sandbox.Application.Handlers;
using Fx.Sandbox.Infrastructure;
using Fx.Sandbox.Infrastructure.Rates;
using Fx.Sandbox.Infrastructure.Repositories;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// -------- Logging --------
builder.Host.UseSerilog((hostContext, configuration) => configuration
    .ReadFrom.Configuration(hostContext.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(new Serilog.Formatting.Compact.RenderedCompactJsonFormatter()));

// -------- CORS --------
const string CorsPolicy = "Ui";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options => options.AddPolicy(CorsPolicy, policy => policy
    .WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

// -------- Rate limiting --------
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 2000,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true,
            }));
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// -------- MVC --------
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(swagger =>
{
    swagger.SwaggerDoc("v1", new() { Title = "Fx.Sandbox API", Version = "v1" });
});

// -------- Validation --------
builder.Services.AddScoped<IValidator<PlaceOrderRequest>, PlaceOrderRequestValidator>();
builder.Services.AddScoped<IValidator<ModifyOrderRequest>, ModifyOrderRequestValidator>();

// -------- Application + Infrastructure --------
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
builder.Services.AddSingleton<IPositionRepository, InMemoryPositionRepository>();
builder.Services.AddSingleton<IAccountRepository, InMemoryAccountRepository>();
builder.Services.AddSingleton<ITradeRepository, InMemoryTradeRepository>();
builder.Services.AddSingleton<IRateHistory>(_ => new InMemoryRateHistory(300));
builder.Services.AddSingleton<IRateFeed, RandomWalkRateFeed>();

builder.Services.AddSingleton<IPlaceLimitOrderHandler, PlaceLimitOrderHandler>();
builder.Services.AddSingleton<ICancelOrderHandler, CancelOrderHandler>();
builder.Services.AddSingleton<IModifyOrderHandler, ModifyOrderHandler>();
builder.Services.AddSingleton<IClosePositionHandler, ClosePositionHandler>();
builder.Services.AddSingleton<IGetOrderBookQuery, GetOrderBookQuery>();
builder.Services.AddSingleton<IGetPositionsQuery, GetPositionsQuery>();
builder.Services.AddSingleton<IGetAccountSummaryQuery, GetAccountSummaryQuery>();
builder.Services.AddSingleton<IGetRateHistoryQuery, GetRateHistoryQuery>();
builder.Services.AddSingleton<IGetCurrentRatesQuery, GetCurrentRatesQuery>();
builder.Services.AddSingleton<IGetTradesQuery, GetTradesQuery>();
builder.Services.AddSingleton<IOnRateTickHandler, OnRateTickHandler>();

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddHostedService<RateTickBackgroundService>();
}

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseMiddleware<CorrelationIdMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(CorsPolicy);
app.UseRateLimiter();
app.MapControllers();

app.Run();

public partial class Program { }
