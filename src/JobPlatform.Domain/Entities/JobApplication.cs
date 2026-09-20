using JobPlatform.Domain.Common;
using JobPlatform.Domain.Enums;
using JobPlatform.Domain.Exceptions;

namespace JobPlatform.Domain.Entities;

public class JobApplication : BaseEntity
{
    public Guid JobId { get; private set; }
    public string ApplicantId { get; private set; }
    public ApplicationStatus Status { get; private set; }
    public DateTime AppliedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    private JobApplication() 
    {
        ApplicantId = string.Empty;
    }

    public static JobApplication Create(Guid jobId, string applicantId, DateTime utcNow)
    {
        return new JobApplication
        {
            JobId = jobId,
            ApplicantId = applicantId,
            Status = ApplicationStatus.Pending,
            AppliedAt = utcNow
        };
    }

    public void Cancel(DateTime utcNow)
    {
        if (Status == ApplicationStatus.Cancelled)
        {
            throw new DomainException("Application is already cancelled.");
        }

        Status = ApplicationStatus.Cancelled;
        CancelledAt = utcNow;
    }
}
