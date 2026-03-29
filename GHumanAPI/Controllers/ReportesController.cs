using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GHumanAPI.Data;

namespace GHumanAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("por-departamento")]
        public async Task<IActionResult> PorDepartamento()
        {
            var data = await _context.Empleados
                .GroupBy(e => e.Departamento)
                .Select(g => new { departamento = g.Key, total = g.Count() })
                .ToListAsync();
            return Ok(data);
        }

        [HttpGet("por-rol")]
        public async Task<IActionResult> PorRol()
        {
            var data = await _context.Empleados
                .Include(e => e.Rol)
                .GroupBy(e => e.Rol.Nombre)
                .Select(g => new { rol = g.Key, total = g.Count() })
                .ToListAsync();
            return Ok(data);
        }

        [HttpGet("por-genero")]
        public async Task<IActionResult> PorGenero()
        {
            var data = await _context.Empleados
                .GroupBy(e => e.Genero)
                .Select(g => new { genero = g.Key == "M" ? "Masculino" : "Femenino", total = g.Count() })
                .ToListAsync();
            return Ok(data);
        }

        [HttpGet("por-estado")]
        public async Task<IActionResult> PorEstado()
        {
            var data = await _context.Empleados
                .GroupBy(e => e.Estado)
                .Select(g => new { estado = g.Key, total = g.Count() })
                .ToListAsync();
            return Ok(data);
        }

        [HttpGet("sueldo-por-departamento")]
        public async Task<IActionResult> SueldoPorDepartamento()
        {
            var data = await _context.Empleados
                .GroupBy(e => e.Departamento)
                .Select(g => new { departamento = g.Key, promedio = g.Average(e => e.Sueldo) })
                .ToListAsync();
            return Ok(data);
        }

        [HttpGet("ingresos-por-anio")]
        public async Task<IActionResult> IngresosPorAnio()
        {
            var data = await _context.Empleados
                .GroupBy(e => e.FechaI.Year)
                .Select(g => new { anio = g.Key, total = g.Count() })
                .OrderBy(g => g.anio)
                .ToListAsync();
            return Ok(data);
        }
    }
}