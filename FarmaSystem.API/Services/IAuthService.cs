using FarmaSystem.API.DTOs;

namespace FarmaSystem.API.Services;

public interface IAuthService
{
    Task<LoginResponseDTO?> LoginAsync(LoginRequestDTO request);
    string NormalizarRol(string? cargo, string? rolOverride = null);
}
