using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Domain.Entities;
using MediatR;

namespace JobPlatform.Application.Applications.Commands.CancelApplication;

public class CancelApplicationCommandHandler : IRequestHandler<CancelApplicationCommand>
{
    private readonly IRepository<JobApplication> _applicationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CancelApplicationCommandHandler(
        IRepository<JobApplication> applicationRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _applicationRepository = applicationRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task Handle(CancelApplicationCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException();
        }

        var application = await _applicationRepository.GetByIdAsync(request.ApplicationId, cancellationToken);
        if (application == null)
        {
            throw new NotFoundException($"Application with ID {request.ApplicationId} not found.");
        }

        if (application.ApplicantId != userId)
        {
            throw new ForbiddenException("You are not authorized to cancel this application.");
        }

        application.Cancel(_dateTimeProvider.UtcNow);
        
        _applicationRepository.Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
