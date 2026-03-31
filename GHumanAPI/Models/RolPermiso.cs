namespace GHumanAPI.Models
{
    public class RolPermiso
    {
        public int IdRol { get; set; }
        public int IdPermiso { get; set; }

        // Navegacion
        public Rol Rol { get; set; } = null!;
        public Permiso Permiso { get; set; } = null!;
    }
}