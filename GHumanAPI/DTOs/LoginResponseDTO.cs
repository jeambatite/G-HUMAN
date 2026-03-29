namespace GHumanAPI.DTOs
{
    public class LoginResponseDTO
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string NombreEmpleado { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public int IdEmpleado { get; set; }
        public List<string> Permisos { get; set; } = new List<string>();
    }
}