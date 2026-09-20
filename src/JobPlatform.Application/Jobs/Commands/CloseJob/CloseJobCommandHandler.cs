using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Domain.Entities;
using MediatR;

namespace JobPlatform.Application.Jobs.Commands.CloseJob;

public class CloseJobCommandHandler : IRequestHandler<CloseJobCommand>
{
    private readonly IRepository<Job> _jobRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CloseJobCommandHandler(
        IRepository<Job> jobRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _jobRepository = jobRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task Handle(CloseJobCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException();
        }

        var job = await _jobRepository.GetByIdAsync(request.JobId, cancellationToken);
        if (job == null)
        {
            throw new NotFoundException($"Job with ID {request.JobId} not found.");
        }

        if (job.RecruiterId != userId)
        {
            throw new ForbiddenException("You are not authorized to close this job.");
        }

        job.Close(_dateTimeProvider.UtcNow);
        
        _jobRepository.Update(job);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
