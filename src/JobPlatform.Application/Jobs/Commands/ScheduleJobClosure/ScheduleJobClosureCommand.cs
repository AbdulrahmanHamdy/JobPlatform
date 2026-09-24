using MediatR;

namespace JobPlatform.Application.Jobs.Commands.ScheduleJobClosure;

/// <summary>
/// Schedules an automatic job closure at the given UTC time.
/// Only the owning Recruiter may schedule it.
/// Returns the Hangfire job ID so the recruiter can cancel it later if needed.
/// </summary>
public record ScheduleJobClosureCommand(Guid JobId, DateTimeOffset CloseAt) : IRequest<string>;
