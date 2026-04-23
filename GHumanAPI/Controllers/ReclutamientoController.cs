using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GHumanAPI.DTOs;
using GHumanAPI.Services;

namespace GHumanAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReclutamientoController : ControllerBase
    {
        private readonly IReclutamientoService _service;

        public ReclutamientoController(IReclutamientoService service)
        {
            _service = service;
        }

        [HttpGet("postulantes")]
        public async Task<IActionResult> GetPostulantes(
            [FromQuery] string? estado,
            [FromQuery] string? puesto,
            [FromQuery] string? busquedaCv)
        {
            var result = await _service.GetPostulantes(estado, puesto, busquedaCv);
            return Ok(result);
        }

        [HttpGet("postulantes/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetById(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("postulantes")]
        [AllowAnonymous]
        public async Task<IActionResult> Crear([FromBody] CrearPostulanteDTO dto)
        {
            var result = await _service.Crear(dto);
            return Ok(result);
        }

        [HttpPut("postulantes/{id}/estado")]
        public async Task<IActionResult> ActualizarEstado(int id, [FromBody] ActualizarEstadoPostulanteDTO dto)
        {
            var result = await _service.ActualizarEstado(id, dto);
            if (!result) return NotFound();
            return Ok(new { message = "Estado actualizado." });
        }

        [HttpDelete("postulantes/{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var result = await _service.Eliminar(id);
            if (!result) return NotFound();
            return Ok(new { message = "Postulante eliminado." });
        }

        [HttpGet("filtros")]
        public async Task<IActionResult> GetFiltros()
        {
            var result = await _service.GetFiltros();
            return Ok(result);
        }

        [HttpPost("filtros")]
        public async Task<IActionResult> CrearFiltro([FromBody] CrearFiltroAtsDTO dto)
        {
            var result = await _service.CrearFiltro(dto);
            return Ok(result);
        }

        [HttpDelete("filtros/{id}")]
        public async Task<IActionResult> EliminarFiltro(int id)
        {
            var result = await _service.EliminarFiltro(id);
            if (!result) return NotFound();
            return Ok(new { message = "Filtro eliminado." });
        }

        [HttpPut("filtros/{id}/toggle")]
        public async Task<IActionResult> ToggleFiltro(int id)
        {
            var result = await _service.ToggleFiltro(id);
            if (!result) return NotFound();
            return Ok(new { message = "Filtro actualizado." });
        }

        [HttpGet("aplicar-filtros")]
        public async Task<IActionResult> AplicarFiltros()
        {
            var result = await _service.AplicarFiltrosAts();
            return Ok(result);
        }
    }
}