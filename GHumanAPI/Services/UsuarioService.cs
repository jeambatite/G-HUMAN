using Microsoft.EntityFrameworkCore;
using GHumanAPI.Data;
using GHumanAPI.DTOs;
using GHumanAPI.Models;

namespace GHumanAPI.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly AppDbContext _context;

        public UsuarioService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> TieneUsuario(int idEmpleado)
        {
            return await _context.Usuarios.AnyAsync(u => u.IdEmpleado == idEmpleado);
        }

        public async Task<UsuarioDTO?> Crear(CrearUsuarioDTO dto)
        {
            // Verificar que el empleado existe
            var empleado = await _context.Empleados
                .Include(e => e.Rol)
                .FirstOrDefaultAsync(e => e.Id == dto.IdEmpleado);
            if (empleado == null) return null;

            // Verificar que no tenga usuario ya
            if (await TieneUsuario(dto.IdEmpleado)) return null;

            var usuario = new Usuario
            {
                IdEmpleado = dto.IdEmpleado,
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                PinHash = dto.Pin != null ? BCrypt.Net.BCrypt.HashPassword(dto.Pin) : null,
                Activo = true
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return new UsuarioDTO
            {
                Id = usuario.Id,
                IdEmpleado = usuario.IdEmpleado,
                Username = usuario.Username,
                Activo = usuario.Activo
            };
        }
    }
}