using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Domain.Entities;
using JobPlatform.Domain.Enums;
using MediatR;

namespace JobPlatform.Application.Jobs.Commands.ScheduleJobClosure;

public class ScheduleJobClosureCommandHandler : IRequestHandler<ScheduleJobClosureCommand, string>
{
    private readonly IRepository<Job> _jobRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IJobSchedulerService _schedulerService;

    public ScheduleJobClosureCommandHandler(
        IRepository<Job> jobRepository,
        ICurrentUserService currentUserService,
        IJobSchedulerService schedulerService)
    {
        _jobRepository = jobRepository;
        _currentUserService = currentUserService;
        _schedulerService = schedulerService;
    }

    public async Task<string> Handle(ScheduleJobClosureCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException();

        var job = await _jobRepository.GetByIdAsync(request.JobId, cancellationToken);
        if (job == null)
            throw new NotFoundException($"Job with ID {request.JobId} not found.");

        if (job.RecruiterId != userId)
            throw new ForbiddenException("You are not authorized to schedule closure for this job.");

        if (job.Status == JobStatus.Closed)
            throw new ConflictException("Job is already closed.");

        var hangfireJobId = _schedulerService.ScheduleJobClosure(request.JobId, request.CloseAt);
        return hangfireJobId;
    }
}
