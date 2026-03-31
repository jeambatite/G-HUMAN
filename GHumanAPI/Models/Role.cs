namespace GHumanAPI.Models
{
    public class Rol
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;

        // Navegacion
        public ICollection<RolPermiso> RolesPermisos { get; set; } = new List<RolPermiso>();
        public ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
    }
}