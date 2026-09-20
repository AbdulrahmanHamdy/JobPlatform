using MediatR;

namespace JobPlatform.Application.Jobs.Commands.ApplyToJob;

public record ApplyToJobCommand(Guid JobId) : IRequest<Guid>;
