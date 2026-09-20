using JobPlatform.Application.Auth.DTOs;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Application.Common.Models;
using MediatR;

namespace JobPlatform.Application.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponseDto>>
{
    private readonly IIdentityService _identityService;

    public RegisterCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<RegisterResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var (result, response) = await _identityService.RegisterUserAsync(
            request.FirstName.Trim(),
            request.LastName.Trim(),
            request.Email.Trim(),
            request.Password,
            request.Role,
            cancellationToken);

        if (!result.Succeeded || response == null)
        {
            return Result<RegisterResponseDto>.Failure(result.Errors);
        }

        return Result<RegisterResponseDto>.Success(response);
    }
}
