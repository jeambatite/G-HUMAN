using GHumanAPI.DTOs;

namespace GHumanAPI.Services
{
    public interface INominaService
    {
        Task<List<NominaEmpleadoDTO>> GetEmpleadosNomina();
        Task<EmpresaConfigDTO?> GetConfig();
        Task<bool> ActualizarBono(ActualizarBonoDTO dto);
        Task<bool> BonoGlobal(BonoGlobalDTO dto);
        Task<List<NominaPagoDTO>> GetHistorial();
        Task ProcesarNomina();
    }
}