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
}
