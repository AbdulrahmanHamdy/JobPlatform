using System.Security.Claims;
using JobPlatform.API.Common;
using JobPlatform.Application.Auth.Commands.Login;
using JobPlatform.Application.Auth.Commands.Register;
using JobPlatform.Application.Auth.DTOs;
using JobPlatform.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPlatform.API.Controllers;

[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<RegisterResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<RegisterResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var result = await Mediator.Send(command);

        if (!result.Succeeded || result.Data == null)
        {
            return BadRequest(ApiResponse<RegisterResponseDto>.FailureResponse("Registration failed.", result.Errors));
        }

        return StatusCode(StatusCodes.Status201Created, ApiResponse<RegisterResponseDto>.SuccessResponse(result.Data, "Registration successful."));
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await Mediator.Send(command);

        if (!result.Succeeded || result.Data == null)
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.FailureResponse("Login failed.", result.Errors));
        }

        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result.Data, "Login successful."));
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
        var firstName = User.FindFirstValue(ClaimTypes.GivenName);
        var lastName = User.FindFirstValue(ClaimTypes.Surname);
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        var userInfo = new
        {
            UserId = userId,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Roles = roles
        };

        return Ok(ApiResponse<object>.SuccessResponse(userInfo, "Current user profile retrieved successfully."));
    }

    [Authorize(Roles = Roles.JobSeeker)]
    [HttpGet("jobseeker-only")]
    public IActionResult JobSeekerOnly()
    {
        return Ok(ApiResponse<string>.SuccessResponse("Access granted. Welcome, JobSeeker!"));
    }

    [Authorize(Roles = Roles.Recruiter)]
    [HttpGet("recruiter-only")]
    public IActionResult RecruiterOnly()
    {
        return Ok(ApiResponse<string>.SuccessResponse("Access granted. Welcome, Recruiter!"));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("admin-only")]
    public IActionResult AdminOnly()
    {
        return Ok(ApiResponse<string>.SuccessResponse("Access granted. Welcome, Administrator!"));
    }
}
