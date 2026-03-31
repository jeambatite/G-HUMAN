using Microsoft.EntityFrameworkCore;
using GHumanAPI.Data;
using GHumanAPI.DTOs;
using GHumanAPI.Models;

namespace GHumanAPI.Services
{
    public class RolService : IRolService
    {
        private readonly AppDbContext _context;

        public RolService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<RolDTO>> GetAll()
        {
            return await _context.Roles
                .Include(r => r.RolesPermisos)
                    .ThenInclude(rp => rp.Permiso)
                .Select(r => new RolDTO
                {
                    Id = r.Id,
                    Nombre = r.Nombre,
                    Permisos = r.RolesPermisos.Select(rp => rp.Permiso.Nombre).ToList(),
                    IdPermisos = r.RolesPermisos.Select(rp => rp.IdPermiso).ToList()
                })
                .ToListAsync();
        }

        public async Task<RolDTO?> GetById(int id)
        {
            var rol = await _context.Roles
                .Include(r => r.RolesPermisos)
                    .ThenInclude(rp => rp.Permiso)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rol == null) return null;

            return new RolDTO
            {
                Id = rol.Id,
                Nombre = rol.Nombre,
                Permisos = rol.RolesPermisos.Select(rp => rp.Permiso.Nombre).ToList(),
                IdPermisos = rol.RolesPermisos.Select(rp => rp.IdPermiso).ToList()
            };
        }

        public async Task<RolDTO> Crear(CrearRolDTO dto)
        {
            var rol = new Rol { Nombre = dto.Nombre };
            _context.Roles.Add(rol);
            await _context.SaveChangesAsync();

            foreach (var idPermiso in dto.IdPermisos)
            {
                _context.RolesPermisos.Add(new RolPermiso
                {
                    IdRol = rol.Id,
                    IdPermiso = idPermiso
                });
            }

            await _context.SaveChangesAsync();
            return (await GetById(rol.Id))!;
        }

        public async Task<RolDTO?> Editar(int id, EditarRolDTO dto)
        {
            var rol = await _context.Roles
                .Include(r => r.RolesPermisos)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rol == null) return null;

            // Eliminar permisos actuales y reasignar
            _context.RolesPermisos.RemoveRange(rol.RolesPermisos);

            foreach (var idPermiso in dto.IdPermisos)
            {
                _context.RolesPermisos.Add(new RolPermiso
                {
                    IdRol = id,
                    IdPermiso = idPermiso
                });
            }

            await _context.SaveChangesAsync();
            return await GetById(id);
        }

        public async Task<bool> Eliminar(int id)
        {
            var rol = await _context.Roles
                .Include(r => r.Empleados)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rol == null) return false;

            // No permitir eliminar si tiene empleados asignados
            if (rol.Empleados.Any()) return false;

            _context.Roles.Remove(rol);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}