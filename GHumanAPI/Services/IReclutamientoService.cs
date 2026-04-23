using GHumanAPI.DTOs;

namespace GHumanAPI.Services
{
    public interface IReclutamientoService
    {
        Task<List<PostulanteDTO>> GetPostulantes(string? estado, string? puesto, string? busquedaCv);
        Task<PostulanteDTO?> GetById(int id);
        Task<PostulanteDTO> Crear(CrearPostulanteDTO dto);
        Task<bool> ActualizarEstado(int id, ActualizarEstadoPostulanteDTO dto);
        Task<bool> Eliminar(int id);
        Task<List<FiltroAtsDTO>> GetFiltros();
        Task<FiltroAtsDTO> CrearFiltro(CrearFiltroAtsDTO dto);
        Task<bool> EliminarFiltro(int id);
        Task<bool> ToggleFiltro(int id);
        Task<List<PostulanteDTO>> AplicarFiltrosAts();
    }
}