using FarmaSystem.Core.DTOs;

namespace FarmaSystem.Core.Interfaces;

public interface IClienteService
{
    Task<object> TestConexionAsync();
    Task<List<ClienteDTO>> ObtenerTodosAsync();
    Task<ClienteDTO?> ObtenerPorIdAsync(int id);
    Task<ClienteDTO> CrearAsync(ClienteCreateDTO dto);
    Task<ClienteDTO?> ActualizarAsync(int id, ClienteCreateDTO dto);
    Task EliminarAsync(int id);
}
