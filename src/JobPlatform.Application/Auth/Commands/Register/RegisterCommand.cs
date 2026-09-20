using JobPlatform.Application.Auth.DTOs;
using JobPlatform.Application.Common.Models;
using MediatR;

namespace JobPlatform.Application.Auth.Commands.Register;

public class RegisterCommand : IRequest<Result<RegisterResponseDto>>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
