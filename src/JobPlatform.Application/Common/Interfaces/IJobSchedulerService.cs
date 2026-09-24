namespace JobPlatform.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the background-job scheduler (Hangfire).
/// Lives in Application so handlers can depend on it without referencing Hangfire directly.
/// </summary>
public interface IJobSchedulerService
{
    /// <summary>
    /// Schedules a job to be closed automatically at the specified UTC time.
    /// Returns the Hangfire job ID.
    /// </summary>
    string ScheduleJobClosure(Guid jobId, DateTimeOffset closeAt);

    /// <summary>
    /// Cancels a previously scheduled closure (e.g., if the recruiter closes manually first).
    /// Returns false if no scheduled job was found.
    /// </summary>
    bool CancelScheduledClosure(string hangfireJobId);
}
