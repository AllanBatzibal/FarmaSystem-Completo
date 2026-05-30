using FarmaSystem.Core.DTOs;

namespace FarmaSystem.Core.Interfaces;

public interface ICategoriaService
{
    Task<List<CategoriaDTO>> ObtenerTodosAsync();
    Task<CategoriaDTO?> ObtenerPorIdAsync(int id);
    Task<CategoriaDTO> CrearAsync(CategoriaDTO dto);
}
