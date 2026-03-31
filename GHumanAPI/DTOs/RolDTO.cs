namespace GHumanAPI.DTOs
{
   public class RolDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public List<string> Permisos { get; set; } = new List<string>();
    public List<int> IdPermisos { get; set; } = new List<int>();
}

    public class CrearRolDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public List<int> IdPermisos { get; set; } = new List<int>();
    }

    public class EditarRolDTO
    {
        public List<int> IdPermisos { get; set; } = new List<int>();
    }
}