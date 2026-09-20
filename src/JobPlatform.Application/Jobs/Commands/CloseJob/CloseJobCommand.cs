using MediatR;

namespace JobPlatform.Application.Jobs.Commands.CloseJob;

public record CloseJobCommand(Guid JobId) : IRequest;
