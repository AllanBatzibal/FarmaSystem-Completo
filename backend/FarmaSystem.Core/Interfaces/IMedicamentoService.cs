using FarmaSystem.Core.DTOs;

namespace FarmaSystem.Core.Interfaces;

public interface IMedicamentoService
{
    Task<List<MedicamentoDTO>> ObtenerTodosAsync();
    Task<MedicamentoDTO?> ObtenerPorIdAsync(int id);
    Task<List<MedicamentoDTO>> ObtenerStockCriticoAsync();
    Task<List<MedicamentoDTO>> ObtenerPorCategoriaAsync(int idCategoria);
    Task<MedicamentoDTO> CrearAsync(MedicamentoCreateDTO dto);
    Task<MedicamentoDTO?> ActualizarAsync(int id, MedicamentoCreateDTO dto);
    Task EliminarAsync(int id);
}
