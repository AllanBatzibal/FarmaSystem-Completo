using FarmaSystem.Core.DTOs;

namespace FarmaSystem.Core.Interfaces;

public interface ICompraService
{
    Task<List<CompraDTO>> ObtenerTodasAsync();
    Task<CompraDTO?> ObtenerPorIdAsync(int id);
    Task<CompraResumenMesDTO> ObtenerResumenMesAsync();
    Task<CompraDTO> RegistrarCompraAsync(CompraCreateDTO dto);
}
