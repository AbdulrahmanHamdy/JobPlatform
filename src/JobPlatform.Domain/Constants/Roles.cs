namespace JobPlatform.Domain.Constants;

public static class Roles
{
    public const string JobSeeker = "JobSeeker";
    public const string Recruiter = "Recruiter";
    public const string Admin = "Admin";

    public static readonly IReadOnlyCollection<string> All = new[] { JobSeeker, Recruiter, Admin };
    public static readonly IReadOnlyCollection<string> Registerable = new[] { JobSeeker, Recruiter };
}
