using JobPlatform.Application.Auth.DTOs;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Application.Common.Models;
using MediatR;

namespace JobPlatform.Application.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly IIdentityService _identityService;

    public LoginCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var (result, response) = await _identityService.LoginAsync(
            request.Email.Trim(),
            request.Password,
            cancellationToken);

        if (!result.Succeeded || response == null)
        {
            return Result<AuthResponseDto>.Failure(result.Errors);
        }

        return Result<AuthResponseDto>.Success(response);
    }
}
