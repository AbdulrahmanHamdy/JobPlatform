using FluentValidation;

namespace JobPlatform.Application.Jobs.Commands.ScheduleJobClosure;

public class ScheduleJobClosureCommandValidator : AbstractValidator<ScheduleJobClosureCommand>
{
    public ScheduleJobClosureCommandValidator()
    {
        RuleFor(x => x.JobId).NotEmpty();
        RuleFor(x => x.CloseAt)
            .Must(dt => dt > DateTimeOffset.UtcNow)
            .WithMessage("CloseAt must be a future UTC date/time.");
    }
}
