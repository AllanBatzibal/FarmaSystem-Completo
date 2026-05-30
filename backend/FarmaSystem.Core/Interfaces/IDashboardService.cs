using FarmaSystem.Core.DTOs;

namespace FarmaSystem.Core.Interfaces;

public interface IDashboardService
{
    Task<DashboardResumenDTO> ObtenerResumenAsync();
    Task<List<VentasPorMesDTO>> ObtenerVentasPorMesAsync();
    Task<List<TopMedicamentoDTO>> ObtenerTopMedicamentosAsync();
}
