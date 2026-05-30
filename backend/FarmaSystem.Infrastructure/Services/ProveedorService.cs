using AutoMapper;
using AutoMapper.QueryableExtensions;
using FarmaSystem.Core.DTOs;
using FarmaSystem.Core.Interfaces;
using FarmaSystem.Core.Models;
using FarmaSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FarmaSystem.Infrastructure.Services;

public class ProveedorService : IProveedorService
{
    private readonly FarmaSystemContext _context;
    private readonly IMapper _mapper;

    public ProveedorService(FarmaSystemContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ProveedorDTO>> ObtenerTodosAsync()
    {
        return await _context.Proveedores
            .AsNoTracking()
            .ProjectTo<ProveedorDTO>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<ProveedorDTO?> ObtenerPorIdAsync(int id)
    {
        var item = await _context.Proveedores.AsNoTracking().FirstOrDefaultAsync(p => p.IdProveedor == id);
        return item == null ? null : _mapper.Map<ProveedorDTO>(item);
    }

    public async Task<ProveedorDTO> CrearAsync(ProveedorCreateDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            throw new InvalidOperationException("El nombre del proveedor es obligatorio.");

        var proveedor = _mapper.Map<Proveedor>(dto);
        _context.Proveedores.Add(proveedor);
        await _context.SaveChangesAsync();
        return _mapper.Map<ProveedorDTO>(proveedor);
    }

    public async Task<ProveedorDTO?> ActualizarAsync(int id, ProveedorCreateDTO dto)
    {
        var proveedor = await _context.Proveedores.FindAsync(id);
        if (proveedor == null)
            return null;

        _mapper.Map(dto, proveedor);
        await _context.SaveChangesAsync();
        return _mapper.Map<ProveedorDTO>(proveedor);
    }

    public async Task EliminarAsync(int id)
    {
        var proveedor = await _context.Proveedores.FindAsync(id);
        if (proveedor == null)
            throw new InvalidOperationException($"Proveedor con ID {id} no encontrado.");

        var tieneCompras = await _context.Compras.AnyAsync(c => c.IdProveedor == id);
        if (tieneCompras)
            throw new InvalidOperationException("No se puede eliminar el proveedor porque tiene compras asociadas.");

        _context.Proveedores.Remove(proveedor);
        await _context.SaveChangesAsync();
    }
}
