namespace GHumanAPI.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public int IdEmpleado { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? PinHash { get; set; }
        public bool Activo { get; set; } = true;

        // Navegacion
        public Empleado Empleado { get; set; } = null!;
    }
}