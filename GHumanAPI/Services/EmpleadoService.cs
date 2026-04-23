using Microsoft.EntityFrameworkCore;
using GHumanAPI.Data;
using GHumanAPI.DTOs;
using GHumanAPI.Models;

namespace GHumanAPI.Services
{
    public class EmpleadoService : IEmpleadoService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public EmpleadoService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<List<EmpleadoDTO>> GetAll()
        {
            return await _context.Empleados
                .Include(e => e.Rol)
                .Include(e => e.Jefe)
                .Include(e => e.DatosSensibles)
                .Select(e => new EmpleadoDTO
                {
                    Id = e.Id,
                    Genero = e.Genero,
                    Nombre = e.Nombre,
                    Email = e.Email,
                    Sueldo = e.Sueldo,
                    FechaI = e.FechaI.ToString("yyyy-MM-dd"),
                    Departamento = e.Departamento,
                    IdJefe = e.IdJefe,
                    NombreJefe = e.Jefe != null ? e.Jefe.Nombre : string.Empty,
                    IdRol = e.IdRol,
                    NombreRol = e.Rol.Nombre,
                    Estado = e.Estado,
                    TieneUsuario = _context.Usuarios.Any(u => u.IdEmpleado == e.Id),
                    FechaNacimiento = e.DatosSensibles != null ? e.DatosSensibles.FechaNacimiento.ToString("yyyy-MM-dd") : null,
                    //nomina 
                    Banco = e.Banco,
                    NumeroCuenta = e.NumeroCuenta,
                    TipoCuenta = e.TipoCuenta,
                    Ausencias = e.Ausencias,
                })
                .ToListAsync();
        }

        public async Task<EmpleadoDTO?> GetById(int id)
        {
            var e = await _context.Empleados
                .Include(e => e.Rol)
                .Include(e => e.Jefe)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (e == null) return null;

            return new EmpleadoDTO
            {
                Id = e.Id,
                Genero = e.Genero,
                Nombre = e.Nombre,
                Email = e.Email,
                Sueldo = e.Sueldo,
                FechaI = e.FechaI.ToString("yyyy-MM-dd"),
                Departamento = e.Departamento,
                IdJefe = e.IdJefe,
                NombreJefe = e.Jefe != null ? e.Jefe.Nombre : string.Empty,
                IdRol = e.IdRol,
                NombreRol = e.Rol.Nombre,
                Estado = e.Estado
            };
        }

        public async Task<EmpleadoDTO> Crear(CrearEmpleadoDTO dto)
        {

            // Verificar email duplicado
            if (await _context.Empleados.AnyAsync(e => e.Email == dto.Email))
                throw new InvalidOperationException("EMAIL_DUPLICADO");

            // Verificar documento duplicado
            if (await _context.Datos_sensibles.AnyAsync(d => d.NumDocumento == dto.NumDocumento))
                throw new InvalidOperationException("DOCUMENTO_DUPLICADO");

            var empleado = new Empleado
            {
                Genero = dto.Genero,
                Nombre = dto.Nombre,
                Email = dto.Email,
                Sueldo = dto.Sueldo,
                FechaI = dto.FechaI,
                Departamento = dto.Departamento,
                IdJefe = dto.IdJefe,
                IdRol = dto.IdRol,
                Estado = dto.Estado,
                Banco = dto.Banco,
                NumeroCuenta = dto.NumeroCuenta,
                TipoCuenta = dto.TipoCuenta,
            };

            _context.Empleados.Add(empleado);
            await _context.SaveChangesAsync();
            await AgregarContactoResend(empleado.Email, empleado.Nombre);

            // Crear datos sensibles
            var datosSensibles = new DatosSensible
            {
                IdEmpleado = empleado.Id,
                TipoDocumento = dto.TipoDocumento,
                NumDocumento = dto.NumDocumento,
                TipoSangre = dto.TipoSangre,
                FechaNacimiento = dto.FechaNacimiento,
                Telefono = dto.Telefono,
                ContactoEmergencia = dto.ContactoEmergencia,
                TelEmergencia = dto.TelEmergencia
            };

            _context.Datos_sensibles.Add(datosSensibles);
            await _context.SaveChangesAsync();

            return (await GetById(empleado.Id))!;
        }

        public async Task<EmpleadoDTO?> Editar(int id, EditarEmpleadoDTO dto)
        {
            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null) return null;

            empleado.Genero = dto.Genero;
            empleado.Nombre = dto.Nombre;
            empleado.Email = dto.Email;
            empleado.Departamento = dto.Departamento;
            empleado.IdJefe = dto.IdJefe;
            empleado.Estado = dto.Estado;

            if (dto.Sueldo.HasValue) empleado.Sueldo = dto.Sueldo.Value;
            if (dto.IdRol.HasValue) empleado.IdRol = dto.IdRol.Value;
            if (dto.FechaI.HasValue) empleado.FechaI = dto.FechaI.Value;
            if (dto.Banco != null) empleado.Banco = dto.Banco;
            if (dto.NumeroCuenta != null) empleado.NumeroCuenta = dto.NumeroCuenta;
            if (dto.TipoCuenta != null) empleado.TipoCuenta = dto.TipoCuenta;

            await _context.SaveChangesAsync();
            if (dto.Email != null && dto.Email != empleado.Email)
            {
                await EliminarContactoResend(empleado.Email);
                await AgregarContactoResend(dto.Email, empleado.Nombre);
            }
            return await GetById(id);
        }


        public async Task<bool> Eliminar(int id)
        {

            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null) return false;

            // Desasignar jefe de subordinados
            var subordinados = await _context.Empleados
                .Where(e => e.IdJefe == id)
                .ToListAsync();

            foreach (var sub in subordinados)
                sub.IdJefe = null;

            // Eliminar usuario manualmente (sin cascade)
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdEmpleado == id);
            if (usuario != null) _context.Usuarios.Remove(usuario);
            await EliminarContactoResend(empleado.Email);

            _context.Empleados.Remove(empleado);
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<DatosSensiblesDTO?> GetDatosSensibles(int id)
        {
            var ds = await _context.Datos_sensibles
                .FirstOrDefaultAsync(d => d.IdEmpleado == id);

            if (ds == null) return null;

            return new DatosSensiblesDTO
            {
                TipoDocumento = ds.TipoDocumento,
                NumDocumento = ds.NumDocumento,
                TipoSangre = ds.TipoSangre,
                FechaNacimiento = ds.FechaNacimiento,
                Telefono = ds.Telefono,
                ContactoEmergencia = ds.ContactoEmergencia,
                TelEmergencia = ds.TelEmergencia
            };
        }
        public async Task<ResultadoPaginadoDTO<EmpleadoDTO>> GetAllPaginado(int pagina, int tamanoPagina, string? nombre = null, string? rol = null, string? estado = null, string? departamento = null)
        {
            var query = _context.Empleados
                .Include(e => e.Rol)
                .Include(e => e.Jefe)
                .Include(e => e.DatosSensibles)
                .AsQueryable();

            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(e => e.Nombre.Contains(nombre));

            if (!string.IsNullOrEmpty(rol))
                query = query.Where(e => e.Rol.Nombre == rol);

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(e => e.Estado == estado);

            if (!string.IsNullOrEmpty(departamento))
                query = query.Where(e => e.Departamento == departamento);

            var total = await query.CountAsync();

            var data = await query
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .Select(e => new EmpleadoDTO
                {
                    Id = e.Id,
                    Genero = e.Genero,
                    Nombre = e.Nombre,
                    Email = e.Email,
                    Sueldo = e.Sueldo,
                    FechaI = e.FechaI.ToString("yyyy-MM-dd"),
                    Departamento = e.Departamento,
                    IdJefe = e.IdJefe,
                    NombreJefe = e.Jefe != null ? e.Jefe.Nombre : string.Empty,
                    IdRol = e.IdRol,
                    NombreRol = e.Rol.Nombre,
                    Estado = e.Estado,
                    TieneUsuario = _context.Usuarios.Any(u => u.IdEmpleado == e.Id),
                    FechaNacimiento = e.DatosSensibles != null ? e.DatosSensibles.FechaNacimiento.ToString("yyyy-MM-dd") : null,
                    Banco = e.Banco,
                    NumeroCuenta = e.NumeroCuenta,
                    TipoCuenta = e.TipoCuenta,
                    Ausencias = e.Ausencias,
                })
                .ToListAsync();

            return new ResultadoPaginadoDTO<EmpleadoDTO>
            {
                Data = data,
                Total = total,
                Pagina = pagina,
                TamanoPagina = tamanoPagina,
                TotalPaginas = (int)Math.Ceiling((double)total / tamanoPagina)
            };
        }
        private async Task AgregarContactoResend(string email, string nombre)
        {
            try
            {
                var apiKey = Environment.GetEnvironmentVariable("Resend__ApiKey")
                    ?? _config["Resend:ApiKey"];
                var audienceId = Environment.GetEnvironmentVariable("Resend__AudienceId")
                    ?? _config["Resend:AudienceId"];

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var partes = nombre.Split(' ');
                var body = new
                {
                    email = email,
                    first_name = partes[0],
                    last_name = partes.Length > 1 ? partes[1] : "",
                    unsubscribed = false
                };

                var json = System.Text.Json.JsonSerializer.Serialize(body);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                await client.PostAsync($"https://api.resend.com/audiences/{audienceId}/contacts", content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error agregando contacto a Resend: {ex.Message}");
            }
        }

        private async Task EliminarContactoResend(string email)
        {
            try
            {
                var apiKey = Environment.GetEnvironmentVariable("Resend__ApiKey")
                    ?? _config["Resend:ApiKey"];
                var audienceId = Environment.GetEnvironmentVariable("Resend__AudienceId")
                    ?? _config["Resend:AudienceId"];

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                await client.DeleteAsync($"https://api.resend.com/audiences/{audienceId}/contacts/{email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error eliminando contacto de Resend: {ex.Message}");
            }
        }
        public async Task<EmpleadoDTO?> ActualizarAusencias(int id, int cantidad)
        {
            var empleado = await _context.Empleados
                .Include(e => e.Rol)
                .Include(e => e.Jefe)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (empleado == null) return null;

            var config = await _context.EmpresaConfig.FirstOrDefaultAsync();
            var limite = config?.LimiteAusencias ?? 3;

            empleado.Ausencias += cantidad;

            if (empleado.Ausencias < 0)
                empleado.Ausencias = 0;

            if (empleado.Ausencias >= limite)
            {
                empleado.Estado = "suspendido";
            }
            else if (empleado.Estado == "suspendido")
            {
                empleado.Estado = "activo";
            }

            await _context.SaveChangesAsync();

            return new EmpleadoDTO
            {
                Id = empleado.Id,
                Genero = empleado.Genero,
                Nombre = empleado.Nombre,
                Email = empleado.Email,
                Sueldo = empleado.Sueldo,
                FechaI = empleado.FechaI.ToString("yyyy-MM-dd"),
                Departamento = empleado.Departamento,
                IdJefe = empleado.IdJefe,
                NombreJefe = empleado.Jefe?.Nombre ?? string.Empty,
                IdRol = empleado.IdRol,
                NombreRol = empleado.Rol?.Nombre ?? string.Empty,
                Estado = empleado.Estado,
                TieneUsuario = empleado.Usuario != null,
                Banco = empleado.Banco,
                NumeroCuenta = empleado.NumeroCuenta,
                TipoCuenta = empleado.TipoCuenta,
                Ausencias = empleado.Ausencias
            };
        }
    }
}