namespace GHumanAPI.DTOs
{
    public class EmpleadoDTO
    {
        public int Id { get; set; }
        public string Genero { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal Sueldo { get; set; }
        public string FechaI { get; set; } = string.Empty;
        public string Departamento { get; set; } = string.Empty;
        public int? IdJefe { get; set; }
        public string NombreJefe { get; set; } = string.Empty;
        public int IdRol { get; set; }
        public string NombreRol { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public bool TieneUsuario { get; set; }
        public string? FechaNacimiento { get; set; }
        public string Banco { get; set; } = string.Empty;
        public string NumeroCuenta { get; set; } = string.Empty;
        public string TipoCuenta { get; set; } = string.Empty;
        public int Ausencias { get; set; }
    }

    public class CrearEmpleadoDTO
    {
        public string Genero { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal Sueldo { get; set; }
        public DateOnly FechaI { get; set; }
        public string Departamento { get; set; } = string.Empty;
        public int? IdJefe { get; set; }
        public int IdRol { get; set; }
        public string Estado { get; set; } = "activo";
        public string TipoDocumento { get; set; } = string.Empty;
        public string NumDocumento { get; set; } = string.Empty;
        public string TipoSangre { get; set; } = string.Empty;
        public DateOnly FechaNacimiento { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public string ContactoEmergencia { get; set; } = string.Empty;
        public string TelEmergencia { get; set; } = string.Empty;
        public string Banco { get; set; } = string.Empty;
        public string NumeroCuenta { get; set; } = string.Empty;
        public string TipoCuenta { get; set; } = string.Empty;
    }

    public class EditarEmpleadoDTO
    {
        public string Genero { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal? Sueldo { get; set; }
        public string Departamento { get; set; } = string.Empty;
        public int? IdJefe { get; set; }
        public int? IdRol { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateOnly? FechaI { get; set; }
        public string? Banco { get; set; }
        public string? NumeroCuenta { get; set; }
        public string? TipoCuenta { get; set; }
        public int? Ausencias { get; set; }
    }
}