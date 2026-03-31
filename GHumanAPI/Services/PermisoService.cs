using Microsoft.EntityFrameworkCore;
using GHumanAPI.Data;
using GHumanAPI.DTOs;

namespace GHumanAPI.Services
{
    public class PermisoService : IPermisoService
    {
        private readonly AppDbContext _context;

        public PermisoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<PermisoDTO>> GetAll()
        {
            return await _context.Permisos
                .Select(p => new PermisoDTO
                {
                    Id     = p.Id,
                    Nombre = p.Nombre
                })
                .ToListAsync();
        }
    }
}