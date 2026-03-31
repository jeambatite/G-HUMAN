namespace GHumanAPI.DTOs
{
    public class DatosSensiblesDTO
    {
        public string TipoDocumento { get; set; } = string.Empty;
        public string NumDocumento { get; set; } = string.Empty;
        public string TipoSangre { get; set; } = string.Empty;
        public DateOnly FechaNacimiento { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public string ContactoEmergencia { get; set; } = string.Empty;
        public string TelEmergencia { get; set; } = string.Empty;
    }
}