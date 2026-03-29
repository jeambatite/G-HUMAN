using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GHumanAPI.Data;
using GHumanAPI.DTOs;

namespace GHumanAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<LoginResponseDTO?> Login(LoginDTO dto)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Empleado)
                    .ThenInclude(e => e.Rol)
                        .ThenInclude(r => r.RolesPermisos)
                            .ThenInclude(rp => rp.Permiso)
                .FirstOrDefaultAsync(u => u.Username == dto.Username && u.Activo);

            if (usuario == null) return null;
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash)) return null;

            var permisos = usuario.Empleado.Rol.RolesPermisos
                .Select(rp => rp.Permiso.Nombre)
                .ToList();

            var token = GenerarToken(
                usuario.Username,
                usuario.Empleado.Rol.Nombre,
                usuario.Empleado.Id,
                permisos
            );

            return new LoginResponseDTO
            {
                Token = token,
                Username = usuario.Username,
                NombreEmpleado = usuario.Empleado.Nombre,
                Rol = usuario.Empleado.Rol.Nombre,
                IdEmpleado = usuario.Empleado.Id,
                Permisos = permisos
            };
        }

        public async Task<bool> VerificarPin(int idEmpleado, string pin)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdEmpleado == idEmpleado);

            if (usuario == null || usuario.PinHash == null) return false;
            return BCrypt.Net.BCrypt.Verify(pin, usuario.PinHash);
        }

        public async Task<bool> CrearPin(int idUsuario, string pin)
        {
            if (pin.Length != 4 || !pin.All(char.IsDigit)) return false;

            var usuario = await _context.Usuarios
                .Include(u => u.Empleado)
                    .ThenInclude(e => e.Rol)
                .FirstOrDefaultAsync(u => u.Id == idUsuario);

            if (usuario == null) return false;
            if (usuario.Empleado.Rol.Nombre != "Admin") return false;

            usuario.PinHash = BCrypt.Net.BCrypt.HashPassword(pin);
            await _context.SaveChangesAsync();
            return true;
        }

        private string GenerarToken(string username, string rol, int idEmpleado, List<string> permisos)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, username),
        new Claim(ClaimTypes.Role, rol),
        new Claim("idEmpleado", idEmpleado.ToString())
    };

            // Agregar cada permiso como claim
            foreach (var permiso in permisos)
            {
                claims.Add(new Claim("permiso", permiso));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expira = DateTime.UtcNow.AddMinutes(
                double.Parse(_config["Jwt:ExpiresInMinutes"]!));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expira,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}


//Nota que este servicio usa BCrypt para hashear contraseñas. Necesitas instalar el paquete. Ejecuta en la terminal:

//dotnet add package BCrypt.Net-Next --version 4.0.3 // esta es la version que use. recordatorio