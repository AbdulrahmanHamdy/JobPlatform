using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace JobPlatform.Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire job class. Hangfire instantiates this via DI, so all constructor
/// dependencies are fully resolved at execution time.
/// </summary>
public class CloseJobBackgroundJob
{
    private readonly IRepository<Job> _jobRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<CloseJobBackgroundJob> _logger;

    public CloseJobBackgroundJob(
        IRepository<Job> jobRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider,
        ILogger<CloseJobBackgroundJob> logger)
    {
        _jobRepository = jobRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    /// <summary>
    /// Entry point called by Hangfire. Closes the job if it is still Open.
    /// Idempotent: if the job is already closed (e.g., recruiter closed it manually),
    /// the method logs and exits gracefully instead of throwing.
    /// </summary>
    public async Task ExecuteAsync(Guid jobId)
    {
        _logger.LogInformation("Hangfire: attempting to auto-close Job {JobId}", jobId);

        var job = await _jobRepository.GetByIdAsync(jobId);
        if (job == null)
        {
            _logger.LogWarning("Hangfire: Job {JobId} not found — skipping.", jobId);
            return;
        }

        try
        {
            job.Close(_dateTimeProvider.UtcNow);
            _jobRepository.Update(job);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Hangfire: Job {JobId} closed successfully.", jobId);
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            // Job already closed by recruiter — not an error
            _logger.LogInformation("Hangfire: Job {JobId} was already closed — {Reason}", jobId, ex.Message);
        }
    }
}
