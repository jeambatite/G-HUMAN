using GHumanAPI.DTOs;

namespace GHumanAPI.Services
{
    public interface IRolService
    {
        Task<List<RolDTO>> GetAll();
        Task<RolDTO?> GetById(int id);
        Task<RolDTO> Crear(CrearRolDTO dto);
        Task<RolDTO?> Editar(int id, EditarRolDTO dto);
        Task<bool> Eliminar(int id);
    }
}