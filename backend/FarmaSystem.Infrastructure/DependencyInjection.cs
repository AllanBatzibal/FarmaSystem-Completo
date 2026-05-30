using FarmaSystem.Core.Interfaces;
using FarmaSystem.Infrastructure.Data;
using FarmaSystem.Infrastructure.Mapping;
using FarmaSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FarmaSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FarmaSystemContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("FarmaSystemDB")));

        services.AddAutoMapper(typeof(MappingProfile));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IProveedorService, ProveedorService>();
        services.AddScoped<ICategoriaService, CategoriaService>();
        services.AddScoped<IEmpleadoService, EmpleadoService>();
        services.AddScoped<IMedicamentoService, MedicamentoService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IVentaService, VentaService>();
        services.AddScoped<ICompraService, CompraService>();
        services.AddScoped<IInventarioService, InventarioService>();

        return services;
    }
}
