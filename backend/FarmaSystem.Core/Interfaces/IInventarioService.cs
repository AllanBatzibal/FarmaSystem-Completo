using FarmaSystem.Core.DTOs;

namespace FarmaSystem.Core.Interfaces;

public interface IInventarioService
{
    Task<List<InventarioItemDTO>> ObtenerEstadoStockAsync();
    Task<List<MovimientoInventarioDTO>> ObtenerMovimientosAsync();
    Task<List<MovimientoInventarioDTO>> ObtenerMovimientosPorMedicamentoAsync(int idMedicamento);
    string ClasificarStock(int stockActual, int stockMinimo);
}
