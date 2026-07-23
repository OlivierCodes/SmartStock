using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStock.Models.DTOs;
using SmartStock.Services.Interfaces;

namespace SmartStock.Controllers;

/// <summary>
/// Gestion des utilisateurs (Responsable seulement).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Responsable")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService) => _userService = userService;

    /// <summary>Liste de tous les utilisateurs.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() =>
        Ok(await _userService.GetAllAsync());

    /// <summary>Détail d'un utilisateur par son identifiant.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _userService.GetByIdAsync(id));

    /// <summary>Modifier un utilisateur.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request) =>
        Ok(await _userService.UpdateAsync(id, request));

    /// <summary>Désactiver (soft delete) un utilisateur.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _userService.DeleteAsync(id);
        return NoContent();
    }
}
