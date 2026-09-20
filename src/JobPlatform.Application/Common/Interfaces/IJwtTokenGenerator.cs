namespace JobPlatform.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, DateTime Expiration) GenerateToken(
        string userId,
        string email,
        string firstName,
        string lastName,
        IEnumerable<string> roles);
}
