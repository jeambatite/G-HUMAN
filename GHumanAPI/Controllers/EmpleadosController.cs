using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GHumanAPI.DTOs;
using GHumanAPI.Services;

namespace GHumanAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmpleadosController : ControllerBase
    {
        private readonly IEmpleadoService _empleadoService;

        public EmpleadosController(IEmpleadoService empleadoService)
        {
            _empleadoService = empleadoService;
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
    }
}