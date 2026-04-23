using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GHumanAPI.DTOs;
using GHumanAPI.Services;
using GHumanAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace GHumanAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmpleadosController : ControllerBase
    {
        private readonly IEmpleadoService _empleadoService;
        private readonly AppDbContext _context;

        public EmpleadosController(IEmpleadoService empleadoService, AppDbContext context)
        {
            _empleadoService = empleadoService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var empleados = await _empleadoService.GetAll();
            return Ok(empleados);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var empleado = await _empleadoService.GetById(id);
            if (empleado == null) return NotFound();
            return Ok(empleado);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Crear([FromBody] CrearEmpleadoDTO dto)
        {
            try
            {
                var result = await _empleadoService.Crear(dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                var mensaje = ex.Message switch
                {
                    "EMAIL_DUPLICADO" => "El email ya está registrado.",
                    "DOCUMENTO_DUPLICADO" => "El número de documento ya está registrado.",
                    _ => "Error al crear el empleado."
                };
                return Conflict(new { message = mensaje });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Nivel 3,Nivel 2")]
        public async Task<IActionResult> Editar(int id, [FromBody] EditarEmpleadoDTO dto)
        {
            var empleado = await _empleadoService.Editar(id, dto);
            if (empleado == null) return NotFound();
            return Ok(empleado);
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var result = await _empleadoService.Eliminar(id);
            if (!result) return NotFound();
            return Ok(new { message = "Empleado eliminado correctamente." });
        }

        [HttpGet("{id}/datos-sensibles")]
        [Authorize(Roles = "Admin,Nivel 3")]
        public async Task<IActionResult> GetDatosSensibles(int id)
        {
            var datos = await _empleadoService.GetDatosSensibles(id);
            if (datos == null) return NotFound();
            return Ok(datos);
        }
        [HttpGet("paginado")]
        public async Task<IActionResult> GetPaginado(
    [FromQuery] int pagina = 1,
    [FromQuery] int tamanoPagina = 10,
    [FromQuery] string? nombre = null,
    [FromQuery] string? rol = null,
    [FromQuery] string? estado = null,
    [FromQuery] string? departamento = null)
        {
            var result = await _empleadoService.GetAllPaginado(pagina, tamanoPagina, nombre, rol, estado, departamento);
            return Ok(result);
        }


        [HttpPut("{id}/ausencias")]
        [Authorize(Roles = "Admin,Nivel 2,Nivel 3")]
        public async Task<IActionResult> ActualizarAusencias(int id)
        {
            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null) return NotFound();

            var config = await _context.EmpresaConfig.FirstOrDefaultAsync();
            int limite = config?.LimiteAusencias ?? 3;

            empleado.Ausencias += 1;

            if (empleado.Ausencias >= limite)
                empleado.Estado = "suspendido";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                ausencias = empleado.Ausencias,
                estado = empleado.Estado,
                suspendido = empleado.Ausencias >= limite
            });
        }
        [HttpPut("{id}/ausencias/quitar")]
        [Authorize(Roles = "Admin,Nivel 2,Nivel 3")]
        public async Task<IActionResult> QuitarAusencia(int id)
        {
            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null) return NotFound();

            if (empleado.Ausencias > 0)
                empleado.Ausencias -= 1;

            if (empleado.Estado == "suspendido" && empleado.Ausencias < (await _context.EmpresaConfig.FirstOrDefaultAsync())?.LimiteAusencias)
                empleado.Estado = "activo";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                ausencias = empleado.Ausencias,
                estado = empleado.Estado
            });
        }


    }
}