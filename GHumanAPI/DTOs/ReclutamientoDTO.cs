namespace GHumanAPI.DTOs
{
    public class PostulanteDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? PuestoAplicado { get; set; }
        public int ExperienciaAnios { get; set; }
        public string? NivelEducacion { get; set; }
        public string? CvUrl { get; set; }
        public bool TieneCv { get; set; }
        public DateTime FechaPostulacion { get; set; }
        public string Estado { get; set; } = "pendiente";
        public string? Notas { get; set; }
        public List<string> PalabrasEncontradas { get; set; } = new();
    }

    public class CrearPostulanteDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? PuestoAplicado { get; set; }
        public int ExperienciaAnios { get; set; }
        public string? NivelEducacion { get; set; }
        public string? CvUrl { get; set; }
        public string? CvTexto { get; set; }
    }

    public class ActualizarEstadoPostulanteDTO
    {
        public string Estado { get; set; } = string.Empty;
        public string? Notas { get; set; }
    }

    public class FiltroAtsDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string PalabrasClave { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime CreadoEn { get; set; }
    }

    public class CrearFiltroAtsDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public string PalabrasClave { get; set; } = string.Empty;
    }

    public class BuscarCvDTO
    {
        public string Termino { get; set; } = string.Empty;
    }
}