using Fx.Sandbox.Application.Abstractions;

namespace Fx.Sandbox.Infrastructure;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
