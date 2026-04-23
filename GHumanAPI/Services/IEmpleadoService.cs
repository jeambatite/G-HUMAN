using GHumanAPI.DTOs;

namespace GHumanAPI.Services
{
    public interface IEmpleadoService
    {
        Task<List<EmpleadoDTO>> GetAll();
        Task<EmpleadoDTO?> GetById(int id);
        Task<EmpleadoDTO> Crear(CrearEmpleadoDTO dto);
        Task<EmpleadoDTO?> Editar(int id, EditarEmpleadoDTO dto);
        Task<bool> Eliminar(int id);
        Task<DatosSensiblesDTO?> GetDatosSensibles(int id);
        Task<ResultadoPaginadoDTO<EmpleadoDTO>> GetAllPaginado(int pagina, int tamanoPagina, string? nombre, string? rol, string? estado, string? departamento);
        Task<EmpleadoDTO?> ActualizarAusencias(int id, int cantidad);
    }
}