using FarmaSystem.API.DTOs;

namespace FarmaSystem.API.Services;

public interface IVentaService
{
    Task<List<VentaDTO>> ObtenerTodasAsync(int? limit = null, DateTime? fecha = null);
    Task<VentaDTO?> ObtenerPorIdAsync(int id);
    Task<List<VentaDTO>> ObtenerPorFechaAsync(DateTime inicio, DateTime fin);
    Task<List<VentaDTO>> ObtenerVentasHoyAsync();
    Task<VentaResumenDTO> ObtenerResumenDiaAsync();
    Task<VentaDTO> RegistrarVentaAsync(VentaCreateDTO dto);
}
