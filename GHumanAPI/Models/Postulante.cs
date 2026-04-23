namespace GHumanAPI.Models
{
    public class Postulante
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? PuestoAplicado { get; set; }
        public int ExperienciaAnios { get; set; } = 0;
        public string? NivelEducacion { get; set; }
        public string? CvUrl { get; set; }
        public string? CvTexto { get; set; }
        public DateTime FechaPostulacion { get; set; } = DateTime.UtcNow;
        public string Estado { get; set; } = "pendiente";
        public string? Notas { get; set; }
    }
}