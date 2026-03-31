using GHumanAPI.DTOs;

namespace GHumanAPI.Services
{
    public interface IPermisoService
    {
        Task<List<PermisoDTO>> GetAll();
    }
}