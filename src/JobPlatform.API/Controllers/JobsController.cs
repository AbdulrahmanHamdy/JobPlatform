using JobPlatform.Application.Jobs.Commands.ScheduleJobClosure;
using JobPlatform.Application.Jobs.Commands.ApplyToJob;
using JobPlatform.Application.Jobs.Commands.CloseJob;
using JobPlatform.Application.Jobs.Commands.CreateJob;
using JobPlatform.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPlatform.API.Controllers;

[Authorize]
public class JobsController : ApiControllerBase
{
    [HttpPost]
    [Authorize(Roles = Roles.Recruiter)]
    public async Task<IActionResult> Create(CreateJobCommand command, CancellationToken cancellationToken)
    {
        var jobId = await Mediator.Send(command, cancellationToken);
        return Created(string.Empty, new { JobId = jobId });
    }

    [HttpPost("{jobId}/apply")]
    [Authorize(Roles = Roles.JobSeeker)]
    public async Task<IActionResult> Apply(Guid jobId, CancellationToken cancellationToken)
    {
        var applicationId = await Mediator.Send(new ApplyToJobCommand(jobId), cancellationToken);
        return Created(string.Empty, new { ApplicationId = applicationId });
    }

    [HttpPost("{jobId}/close")]
    [Authorize(Roles = Roles.Recruiter)]
    public async Task<IActionResult> Close(Guid jobId, CancellationToken cancellationToken)
    {
        await Mediator.Send(new CloseJobCommand(jobId), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Schedules a job to be closed automatically at the given UTC time.
    /// Returns the Hangfire job ID.
    /// </summary>
    [HttpPost("{jobId}/schedule-close")]
    [Authorize(Roles = Roles.Recruiter)]
    public async Task<IActionResult> ScheduleClose(Guid jobId, ScheduleCloseRequest request, CancellationToken cancellationToken)
    {
        var hangfireJobId = await Mediator.Send(
            new ScheduleJobClosureCommand(jobId, request.CloseAt), cancellationToken);

        return Accepted(new { HangfireJobId = hangfireJobId, ScheduledFor = request.CloseAt });
    }
}

/// <summary>
/// Request body for schedule-close. Only CloseAt is accepted from the caller;
/// JobId always comes from the route.
/// </summary>
public record ScheduleCloseRequest(DateTimeOffset CloseAt);
