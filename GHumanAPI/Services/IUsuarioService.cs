using GHumanAPI.DTOs;

namespace GHumanAPI.Services
{
    public interface IUsuarioService
    {
        Task<UsuarioDTO?> Crear(CrearUsuarioDTO dto);
        Task<bool> TieneUsuario(int idEmpleado);
    }
}