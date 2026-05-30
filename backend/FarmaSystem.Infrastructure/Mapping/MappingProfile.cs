using AutoMapper;
using FarmaSystem.Core.DTOs;
using FarmaSystem.Core.Models;

namespace FarmaSystem.Infrastructure.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Cliente, ClienteDTO>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.IdCliente))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Correo))
            .ForMember(d => d.Nit, o => o.Ignore());

        CreateMap<ClienteCreateDTO, Cliente>()
            .ForMember(d => d.Correo, o => o.MapFrom(s => s.Email))
            .ForMember(d => d.FechaRegistro, o => o.MapFrom(_ => DateTime.Now));

        CreateMap<Proveedor, ProveedorDTO>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.IdProveedor))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Correo));

        CreateMap<ProveedorCreateDTO, Proveedor>()
            .ForMember(d => d.Correo, o => o.MapFrom(s => s.Email));

        CreateMap<Empleado, EmpleadoDTO>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.IdEmpleado))
            .ForMember(d => d.Puesto, o => o.MapFrom(s => s.Cargo))
            .ForMember(d => d.Email, o => o.Ignore());

        CreateMap<EmpleadoCreateDTO, Empleado>()
            .ForMember(d => d.Cargo, o => o.MapFrom(s => s.Puesto))
            .ForMember(d => d.FechaIngreso, o => o.MapFrom(_ => DateTime.Today));

        CreateMap<Categoria, CategoriaDTO>().ForMember(d => d.Id, o => o.MapFrom(s => s.IdCategoria));

        CreateMap<Medicamento, MedicamentoDTO>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.IdMedicamento))
            .ForMember(d => d.Categoria, o => o.MapFrom(s => s.Categoria.Nombre));

        CreateMap<Venta, VentaDTO>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.IdVenta))
            .ForMember(d => d.Fecha, o => o.MapFrom(s => s.Fecha))
            .ForMember(d => d.ClienteNombre, o => o.MapFrom(s => $"{s.Cliente.Nombre} {s.Cliente.Apellido}".Trim()))
            .ForMember(d => d.EmpleadoNombre, o => o.MapFrom(s => $"{s.Empleado.Nombre} {s.Empleado.Apellido}".Trim()));

        CreateMap<DetalleVenta, DetalleVentaDTO>()
            .ForMember(d => d.Nombre, o => o.MapFrom(s => s.Medicamento.Nombre));

        CreateMap<Compra, CompraDTO>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.IdCompra))
            .ForMember(d => d.Fecha, o => o.MapFrom(s => s.Fecha))
            .ForMember(d => d.NumeroFactura, o => o.MapFrom(s => s.NFactura))
            .ForMember(d => d.ProveedorNombre, o => o.MapFrom(s => s.Proveedor.Nombre))
            .ForMember(d => d.EmpleadoNombre, o => o.MapFrom(s => $"{s.Empleado.Nombre} {s.Empleado.Apellido}".Trim()));

        CreateMap<DetalleCompra, DetalleCompraDTO>()
            .ForMember(d => d.Nombre, o => o.MapFrom(s => s.Medicamento.Nombre));

        CreateMap<MovimientoInventario, MovimientoInventarioDTO>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.IdMovimiento))
            .ForMember(d => d.MedicamentoNombre, o => o.MapFrom(s => s.Medicamento.Nombre))
            .ForMember(d => d.Motivo, o => o.MapFrom(s => s.Motivo))
            .ForMember(d => d.Referencia, o => o.MapFrom(s => s.Motivo))
            .ForMember(d => d.Observacion, o => o.MapFrom(s => s.Motivo));
    }
}
