using AutoMapper;
using AutoMapper.QueryableExtensions;
using FarmaSystem.Core.DTOs;
using FarmaSystem.Core.Interfaces;
using FarmaSystem.Core.Models;
using FarmaSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FarmaSystem.Infrastructure.Services;

public class CategoriaService : ICategoriaService
{
    private readonly FarmaSystemContext _context;
    private readonly IMapper _mapper;

    public CategoriaService(FarmaSystemContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<CategoriaDTO>> ObtenerTodosAsync()
    {
        return await _context.Categorias
            .AsNoTracking()
            .ProjectTo<CategoriaDTO>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<CategoriaDTO?> ObtenerPorIdAsync(int id)
    {
        var item = await _context.Categorias.AsNoTracking().FirstOrDefaultAsync(c => c.IdCategoria == id);
        return item == null ? null : _mapper.Map<CategoriaDTO>(item);
    }

    public async Task<CategoriaDTO> CrearAsync(CategoriaDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            throw new InvalidOperationException("El nombre de la categoría es obligatorio.");

        var categoria = new Categoria { Nombre = dto.Nombre, Descripcion = dto.Descripcion };
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();
        return _mapper.Map<CategoriaDTO>(categoria);
    }
}
