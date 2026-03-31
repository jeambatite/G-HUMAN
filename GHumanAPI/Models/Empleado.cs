namespace GHumanAPI.Models
{
    public class Empleado
    {
        public int Id { get; set; }
        public string Genero { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal Sueldo { get; set; }
        public DateOnly FechaI { get; set; }
        public string Departamento { get; set; } = string.Empty;
        public int? IdJefe { get; set; }
        public int IdRol { get; set; }
        public string Estado { get; set; } = "activo";

        // Navegacion
        public Rol Rol { get; set; } = null!;
        public Empleado? Jefe { get; set; }
        public ICollection<Empleado> Subordinados { get; set; } = new List<Empleado>();
        public DatosSensible? DatosSensibles { get; set; }
        public Usuario? Usuario { get; set; }
    }
}