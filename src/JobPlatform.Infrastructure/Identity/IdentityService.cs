using JobPlatform.Application.Auth.DTOs;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Application.Common.Models;
using Microsoft.AspNetCore.Identity;

namespace JobPlatform.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user == null;
    }

    public async Task<(Result Result, RegisterResponseDto? Response)> RegisterUserAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        string role,
        CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return (Result.Failure("The specified email address is already registered."), null);
        }

        if (!await _roleManager.RoleExistsAsync(role))
        {
            return (Result.Failure($"Role '{role}' does not exist."), null);
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            return (Result.Failure(createResult.Errors.Select(e => e.Description)), null);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            return (Result.Failure(roleResult.Errors.Select(e => e.Description)), null);
        }

        var response = new RegisterResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = role,
            Message = "User registered successfully."
        };

        return (Result.Success(), response);
    }

    public async Task<(Result Result, AuthResponseDto? Response)> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return (Result.Failure("Invalid email or password."), null);
        }

        var isValidPassword = await _userManager.CheckPasswordAsync(user, password);
        if (!isValidPassword)
        {
            return (Result.Failure("Invalid email or password."), null);
        }

        var roles = await _userManager.GetRolesAsync(user);

        var (token, expiration) = _jwtTokenGenerator.GenerateToken(
            user.Id,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            roles);

        var response = new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = roles.FirstOrDefault() ?? string.Empty,
            Token = token,
            Expiration = expiration
        };

        return (Result.Success(), response);
    }
}
