namespace GHumanAPI.DTOs
{
    public class CrearUsuarioDTO
    {
        public int IdEmpleado { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
         public string? Pin { get; set; }
    }

    public class UsuarioDTO
    {
        public int Id { get; set; }
        public int IdEmpleado { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}