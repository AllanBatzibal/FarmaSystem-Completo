using FarmaSystem.Core.DTOs;

namespace FarmaSystem.Core.Interfaces;

public interface IProveedorService
{
    Task<List<ProveedorDTO>> ObtenerTodosAsync();
    Task<ProveedorDTO?> ObtenerPorIdAsync(int id);
    Task<ProveedorDTO> CrearAsync(ProveedorCreateDTO dto);
    Task<ProveedorDTO?> ActualizarAsync(int id, ProveedorCreateDTO dto);
    Task EliminarAsync(int id);
}
