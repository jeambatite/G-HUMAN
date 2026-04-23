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
    [Authorize(Roles = "Admin")]
    public class NominaController : ControllerBase
    {
        private readonly INominaService _nominaService;
        private readonly AppDbContext _context;

        public NominaController(INominaService nominaService, AppDbContext context)
        {
            _nominaService = nominaService;
            _context = context;
        }

        [HttpGet("empleados")]
        public async Task<IActionResult> GetEmpleados()
        {
            var result = await _nominaService.GetEmpleadosNomina();
            return Ok(result);
        }

        [HttpGet("config")]
        public async Task<IActionResult> GetConfig()
        {
            var result = await _nominaService.GetConfig();
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPut("bono")]
        public async Task<IActionResult> ActualizarBono([FromBody] ActualizarBonoDTO dto)
        {
            if (dto.Bono < 0 || dto.Bono > 100)
                return BadRequest(new { message = "El bono debe ser un entero entre 0 y 100." });
            var result = await _nominaService.ActualizarBono(dto);
            if (!result) return NotFound();
            return Ok(new { message = "Bono actualizado." });
        }

        [HttpPut("bono-global")]
        public async Task<IActionResult> BonoGlobal([FromBody] BonoGlobalDTO dto)
        {
            if (dto.Bono < 0 || dto.Bono > 100)
                return BadRequest(new { message = "El bono debe ser un entero entre 0 y 100." });
            var result = await _nominaService.BonoGlobal(dto);
            if (!result) return BadRequest();
            return Ok(new { message = $"Bono de {dto.Bono}% aplicado a todos los empleados." });
        }

        [HttpGet("historial")]
        public async Task<IActionResult> GetHistorial()
        {
            var result = await _nominaService.GetHistorial();
            return Ok(result);
        }

        [HttpPost("test-run")]
        [AllowAnonymous]
        public async Task<IActionResult> TestRun([FromBody] TestRunDTO dto)
        {
            var config = await _context.EmpresaConfig.FirstOrDefaultAsync();
            if (config == null) return NotFound(new { message = "Configuración no encontrada." });

            if (!BCrypt.Net.BCrypt.Verify(dto.SecretKey, config.TestRunKeyHash))
                return Unauthorized(new { message = "Clave incorrecta." });

            await _nominaService.ProcesarNomina();
            return Ok(new { message = "Nómina procesada correctamente." });
        }
        [HttpPut("config/limite-ausencias")]
        public async Task<IActionResult> ActualizarLimiteAusencias([FromBody] int limite)
        {
            if (limite < 1 || limite > 365)
                return BadRequest(new { message = "El límite debe estar entre 1 y 365." });

            var result = await _nominaService.ActualizarLimiteAusencias(limite);
            if (!result) return NotFound(new { message = "Configuración no encontrada." });

            return Ok(new { message = $"Límite de ausencias actualizado a {limite}." });
        }
    }
}