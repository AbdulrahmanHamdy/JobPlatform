using JobPlatform.Domain.Common;
using JobPlatform.Domain.Enums;
using JobPlatform.Domain.Exceptions;

namespace JobPlatform.Domain.Entities;

public class Job : BaseEntity
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string RecruiterId { get; private set; }
    public JobStatus Status { get; private set; }
    public DateTime? ClosedAt { get; private set; }

    private Job() 
    {
        Title = string.Empty;
        Description = string.Empty;
        RecruiterId = string.Empty;
    }

    public static Job Create(string title, string description, string recruiterId)
    {
        return new Job
        {
            Title = title,
            Description = description,
            RecruiterId = recruiterId,
            Status = JobStatus.Open
        };
    }

    public void Close(DateTime utcNow)
    {
        if (Status == JobStatus.Closed)
        {
            throw new DomainException("Job is already closed.");
        }

        Status = JobStatus.Closed;
        ClosedAt = utcNow;
    }
}
