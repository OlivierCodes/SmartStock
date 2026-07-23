using SmartStock.Models.DTOs;
using SmartStock.Models.Entities;

namespace SmartStock.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<UserDto> RegisterAsync(RegisterRequest request);
    Task ChangePasswordAsync(int userId, ChangePasswordRequest request);
}

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task<UserDto> GetByIdAsync(int id);
    Task<UserDto> UpdateAsync(int id, UpdateUserRequest request);
    Task DeleteAsync(int id);
}
