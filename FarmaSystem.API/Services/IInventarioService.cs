using FarmaSystem.API.DTOs;

namespace FarmaSystem.API.Services;

public interface IInventarioService
{
    Task<List<InventarioItemDTO>> ObtenerEstadoStockAsync();
    Task<List<MovimientoInventarioDTO>> ObtenerMovimientosAsync();
    Task<List<MovimientoInventarioDTO>> ObtenerMovimientosPorMedicamentoAsync(int idMedicamento);
    string ClasificarStock(int stockActual, int stockMinimo);
}
