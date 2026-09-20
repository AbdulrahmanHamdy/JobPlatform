using MediatR;

namespace JobPlatform.Application.Applications.Commands.CancelApplication;

public record CancelApplicationCommand(Guid ApplicationId) : IRequest;
