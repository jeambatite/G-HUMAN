using Microsoft.AspNetCore.Mvc;
using GHumanAPI.DTOs;
using GHumanAPI.Services;

namespace GHumanAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            var result = await _authService.Login(dto);
            if (result == null)
                return Unauthorized(new { message = "Usuario o contraseña incorrectos." });

            return Ok(result);
        }

        [HttpPost("verificar-pin/{idEmpleado}")]
        public async Task<IActionResult> VerificarPin(int idEmpleado, [FromBody] VerificarPinDTO dto)
        {
            var result = await _authService.VerificarPin(idEmpleado, dto.Pin);
            if (!result)
                return Unauthorized(new { message = "PIN incorrecto." });

            return Ok(new { valido = true });
        }

        [HttpPost("crear-pin/{idUsuario}")]
        public async Task<IActionResult> CrearPin(int idUsuario, [FromBody] CrearPinDTO dto)
        {
            var result = await _authService.CrearPin(idUsuario, dto.Pin);
            if (!result)
                return BadRequest(new { message = "No se pudo crear el PIN. Verifica que el usuario sea Admin y el PIN tenga 4 dígitos." });

            return Ok(new { message = "PIN creado correctamente." });
        }

    }
}