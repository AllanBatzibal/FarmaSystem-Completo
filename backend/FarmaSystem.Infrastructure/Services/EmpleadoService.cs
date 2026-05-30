using AutoMapper;
using AutoMapper.QueryableExtensions;
using FarmaSystem.Core.DTOs;
using FarmaSystem.Core.Interfaces;
using FarmaSystem.Core.Models;
using FarmaSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FarmaSystem.Infrastructure.Services;

public class EmpleadoService : IEmpleadoService
{
    private readonly FarmaSystemContext _context;
    private readonly IMapper _mapper;

    public EmpleadoService(FarmaSystemContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<EmpleadoDTO>> ObtenerTodosAsync()
    {
        return await _context.Empleados
            .AsNoTracking()
            .ProjectTo<EmpleadoDTO>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<EmpleadoDTO?> ObtenerPorIdAsync(int id)
    {
        var item = await _context.Empleados.AsNoTracking().FirstOrDefaultAsync(e => e.IdEmpleado == id);
        return item == null ? null : _mapper.Map<EmpleadoDTO>(item);
    }

    public async Task<EmpleadoDTO> CrearAsync(EmpleadoCreateDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Apellido))
            throw new InvalidOperationException("Nombre y apellido son obligatorios.");

        var empleado = _mapper.Map<Empleado>(dto);
        _context.Empleados.Add(empleado);
        await _context.SaveChangesAsync();
        return _mapper.Map<EmpleadoDTO>(empleado);
    }

    public async Task<EmpleadoDTO?> ActualizarAsync(int id, EmpleadoCreateDTO dto)
    {
        var empleado = await _context.Empleados.FindAsync(id);
        if (empleado == null)
            return null;

        _mapper.Map(dto, empleado);
        await _context.SaveChangesAsync();
        return _mapper.Map<EmpleadoDTO>(empleado);
    }

    public async Task EliminarAsync(int id)
    {
        var empleado = await _context.Empleados.FindAsync(id);
        if (empleado == null)
            throw new InvalidOperationException($"Empleado con ID {id} no encontrado.");

        var tieneRegistros = await _context.Ventas.AnyAsync(v => v.IdEmpleado == id)
            || await _context.Compras.AnyAsync(c => c.IdEmpleado == id);
        if (tieneRegistros)
            throw new InvalidOperationException("No se puede eliminar el empleado porque tiene ventas o compras asociadas.");

        _context.Empleados.Remove(empleado);
        await _context.SaveChangesAsync();
    }
}
