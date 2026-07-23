using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStock.Models.DTOs;
using SmartStock.Services.Interfaces;

namespace SmartStock.Controllers;

/// <summary>
/// Authentification – connexion, inscription et gestion du mot de passe.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>Connexion et obtention du token JWT.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }

    /// <summary>Inscription d'un nouvel utilisateur (Responsable seulement).</summary>
    [HttpPost("register")]
    [Authorize(Roles = "Responsable")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = await _authService.RegisterAsync(request);
        return CreatedAtAction(nameof(Register), new { id = user.Id }, user);
    }

    /// <summary>Changer son propre mot de passe.</summary>
    [HttpPut("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _authService.ChangePasswordAsync(userId, request);
        return NoContent();
    }

    /// <summary>Informations de l'utilisateur connecté.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public IActionResult Me()
    {
        var claims = User.Claims.ToDictionary(c => c.Type, c => c.Value);
        return Ok(new
        {
            Id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            Email = User.FindFirstValue(ClaimTypes.Email),
            Role = User.FindFirstValue(ClaimTypes.Role),
            FirstName = claims.GetValueOrDefault("firstName"),
            LastName = claims.GetValueOrDefault("lastName")
        });
    }
}
