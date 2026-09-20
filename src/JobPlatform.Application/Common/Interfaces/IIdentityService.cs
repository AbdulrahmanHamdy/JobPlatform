using JobPlatform.Application.Auth.DTOs;
using JobPlatform.Application.Common.Models;

namespace JobPlatform.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<(Result Result, RegisterResponseDto? Response)> RegisterUserAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        string role,
        CancellationToken cancellationToken = default);

    Task<(Result Result, AuthResponseDto? Response)> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default);
}
