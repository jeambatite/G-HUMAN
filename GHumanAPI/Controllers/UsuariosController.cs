using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GHumanAPI.DTOs;
using GHumanAPI.Services;

namespace GHumanAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpGet("tiene-usuario/{idEmpleado}")]
        public async Task<IActionResult> TieneUsuario(int idEmpleado)
        {
            var result = await _usuarioService.TieneUsuario(idEmpleado);
            return Ok(new { tieneUsuario = result });
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearUsuarioDTO dto)
        {
            var result = await _usuarioService.Crear(dto);
            if (result == null)
                return BadRequest(new { message = "No se pudo crear el usuario. Verifica que el empleado exista y no tenga usuario." });
            return Ok(result);
        }
    }
}