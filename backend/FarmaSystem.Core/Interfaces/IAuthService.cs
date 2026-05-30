using FarmaSystem.Core.DTOs;

namespace FarmaSystem.Core.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDTO?> LoginAsync(LoginRequestDTO request);
    Task<object> ObtenerEstadoAsync();
    string NormalizarRol(string? cargo, string? rolOverride = null);
}
