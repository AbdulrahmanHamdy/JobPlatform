using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Domain.Entities;
using JobPlatform.Domain.Enums;
using MediatR;

namespace JobPlatform.Application.Jobs.Commands.ApplyToJob;

public class ApplyToJobCommandHandler : IRequestHandler<ApplyToJobCommand, Guid>
{
    private readonly IRepository<Job> _jobRepository;
    private readonly IRepository<JobApplication> _applicationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ApplyToJobCommandHandler(
        IRepository<Job> jobRepository,
        IRepository<JobApplication> applicationRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _jobRepository = jobRepository;
        _applicationRepository = applicationRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Guid> Handle(ApplyToJobCommand request, CancellationToken cancellationToken)
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

        if (job.Status == JobStatus.Closed)
        {
            throw new ConflictException("Cannot apply to a closed job.");
        }

        var hasPendingApplication = await _applicationRepository.ExistsAsync(
            a => a.JobId == request.JobId && a.ApplicantId == userId && a.Status == ApplicationStatus.Pending,
            cancellationToken);

        if (hasPendingApplication)
        {
            throw new ConflictException("You already have a pending application for this job.");
        }

        var application = JobApplication.Create(request.JobId, userId, _dateTimeProvider.UtcNow);
        
        await _applicationRepository.AddAsync(application, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return application.Id;
    }
}
