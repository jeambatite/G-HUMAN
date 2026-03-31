namespace GHumanAPI.Models
{
    public class Permiso
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;

        // Navegacion
        public ICollection<RolPermiso> RolesPermisos { get; set; } = new List<RolPermiso>();
    }
}