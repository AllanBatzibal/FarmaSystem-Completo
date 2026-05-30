using AutoMapper;
using AutoMapper.QueryableExtensions;
using FarmaSystem.Core.DTOs;
using FarmaSystem.Core.Interfaces;
using FarmaSystem.Core.Models;
using FarmaSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FarmaSystem.Infrastructure.Services;

public class ClienteService : IClienteService
{
    private readonly FarmaSystemContext _context;
    private readonly IMapper _mapper;

    public ClienteService(FarmaSystemContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<object> TestConexionAsync()
    {
        var totalClientes = await _context.Clientes.CountAsync();
        var totalMedicamentos = await _context.Medicamentos.CountAsync();
        return new
        {
            mensaje = "Conexión exitosa a FarmaSystemDB",
            api = "http://localhost:5280",
            clientes = totalClientes,
            medicamentos = totalMedicamentos
        };
    }

    public async Task<List<ClienteDTO>> ObtenerTodosAsync()
    {
        return await _context.Clientes
            .AsNoTracking()
            .ProjectTo<ClienteDTO>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<ClienteDTO?> ObtenerPorIdAsync(int id)
    {
        var cliente = await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.IdCliente == id);
        return cliente == null ? null : _mapper.Map<ClienteDTO>(cliente);
    }

    public async Task<ClienteDTO> CrearAsync(ClienteCreateDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Apellido))
            throw new InvalidOperationException("Nombre y apellido son obligatorios.");

        var cliente = _mapper.Map<Cliente>(dto);
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return _mapper.Map<ClienteDTO>(cliente);
    }

    public async Task<ClienteDTO?> ActualizarAsync(int id, ClienteCreateDTO dto)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
            return null;

        if (string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Apellido))
            throw new InvalidOperationException("Nombre y apellido son obligatorios.");

        _mapper.Map(dto, cliente);
        await _context.SaveChangesAsync();
        return _mapper.Map<ClienteDTO>(cliente);
    }

    public async Task EliminarAsync(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
            throw new InvalidOperationException($"Cliente con ID {id} no encontrado.");

        var tieneVentas = await _context.Ventas.AnyAsync(v => v.IdCliente == id);
        if (tieneVentas)
            throw new InvalidOperationException("No se puede eliminar el cliente porque tiene ventas asociadas.");

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();
    }
}
