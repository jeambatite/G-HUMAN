namespace GHumanAPI.Models
{
    public class DatosSensible
    {
        public int Id { get; set; }
        public int IdEmpleado { get; set; }
        public string TipoDocumento { get; set; } = string.Empty;
        public string NumDocumento { get; set; } = string.Empty;
        public string TipoSangre { get; set; } = string.Empty;
        public DateOnly FechaNacimiento { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public string ContactoEmergencia { get; set; } = string.Empty;
        public string TelEmergencia { get; set; } = string.Empty;

        // Navegacion
        public Empleado Empleado { get; set; } = null!;
    }
}