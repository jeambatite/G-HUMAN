using GHumanAPI.DTOs;

namespace GHumanAPI.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDTO?> Login(LoginDTO dto);
        Task<bool> VerificarPin(int idUsuario, string pin);
        Task<bool> CrearPin(int idUsuario, string pin);
    }
}