using DayCareManagement.Application.Abstractions;

namespace DayCareManagement.Infrastructure.System;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
