using MediatR;

namespace JobPlatform.Application.Jobs.Commands.CreateJob;

public record CreateJobCommand(string Title, string Description) : IRequest<Guid>;
