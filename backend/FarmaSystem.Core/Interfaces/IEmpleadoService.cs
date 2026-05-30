using FarmaSystem.Core.DTOs;

namespace FarmaSystem.Core.Interfaces;

public interface IEmpleadoService
{
    Task<List<EmpleadoDTO>> ObtenerTodosAsync();
    Task<EmpleadoDTO?> ObtenerPorIdAsync(int id);
    Task<EmpleadoDTO> CrearAsync(EmpleadoCreateDTO dto);
    Task<EmpleadoDTO?> ActualizarAsync(int id, EmpleadoCreateDTO dto);
    Task EliminarAsync(int id);
}
