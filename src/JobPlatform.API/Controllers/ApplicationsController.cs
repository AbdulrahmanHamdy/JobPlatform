using JobPlatform.Application.Applications.Commands.CancelApplication;
using JobPlatform.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPlatform.API.Controllers;

[Authorize(Roles = Roles.JobSeeker)]
public class ApplicationsController : ApiControllerBase
{
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new CancelApplicationCommand(id), cancellationToken);
        return NoContent();
    }
}
