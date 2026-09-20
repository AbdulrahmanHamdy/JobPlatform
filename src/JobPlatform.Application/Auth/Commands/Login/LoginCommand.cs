using JobPlatform.Application.Auth.DTOs;
using JobPlatform.Application.Common.Models;
using MediatR;

namespace JobPlatform.Application.Auth.Commands.Login;

public class LoginCommand : IRequest<Result<AuthResponseDto>>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
