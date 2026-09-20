using JobPlatform.Application.Common.Interfaces;

namespace JobPlatform.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
