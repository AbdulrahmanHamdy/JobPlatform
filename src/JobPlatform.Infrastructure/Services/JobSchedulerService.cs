using Hangfire;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Infrastructure.BackgroundJobs;

namespace JobPlatform.Infrastructure.Services;

/// <summary>
/// Implements IJobSchedulerService by delegating to Hangfire's IBackgroundJobClient.
/// Kept in Infrastructure so Hangfire is never referenced from Application or Domain.
/// </summary>
public class JobSchedulerService : IJobSchedulerService
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public JobSchedulerService(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public string ScheduleJobClosure(Guid jobId, DateTimeOffset closeAt)
    {
        var delay = closeAt - DateTimeOffset.UtcNow;
        if (delay <= TimeSpan.Zero)
            delay = TimeSpan.Zero; // enqueue immediately if time has passed (safety net)

        return _backgroundJobClient.Schedule<CloseJobBackgroundJob>(
            job => job.ExecuteAsync(jobId),
            delay);
    }

    public bool CancelScheduledClosure(string hangfireJobId)
    {
        return _backgroundJobClient.Delete(hangfireJobId);
    }
}
