using FarmaSystem.API.DTOs;

namespace FarmaSystem.API.Services;

public interface ICompraService
{
    Task<List<CompraDTO>> ObtenerTodasAsync();
    Task<CompraDTO?> ObtenerPorIdAsync(int id);
    Task<CompraResumenMesDTO> ObtenerResumenMesAsync();
    Task<CompraDTO> RegistrarCompraAsync(CompraCreateDTO dto);
    Task ActualizarCompraAsync(int id, ActualizarCompraDTO dto);
}
