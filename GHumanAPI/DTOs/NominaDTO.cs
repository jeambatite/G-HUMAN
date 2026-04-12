namespace GHumanAPI.DTOs
{
    public class NominaEmpleadoDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Departamento { get; set; } = string.Empty;
        public string NombreRol { get; set; } = string.Empty;
        public decimal Sueldo { get; set; }
        public int BonoProximoPago { get; set; }
        public string Banco { get; set; } = string.Empty;
        public string NumeroCuenta { get; set; } = string.Empty;
        public string TipoCuenta { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }

    public class ActualizarBonoDTO
    {
        public int EmpleadoId { get; set; }
        public int Bono { get; set; }
    }

    public class BonoGlobalDTO
    {
        public int Bono { get; set; }
    }

    public class EmpresaConfigDTO
    {
        public decimal BalanceActual { get; set; }
        public int UltimaNominaMes { get; set; }
        public int DiaPago { get; set; }
        public string EmailAdmin { get; set; } = string.Empty;
    }

    public class TestRunDTO
    {
        public string SecretKey { get; set; } = string.Empty;
    }

    public class NominaPagoDTO
    {
        public int Id { get; set; }
        public int EmpleadoId { get; set; }
        public string NombreEmpleado { get; set; } = string.Empty;
        public DateTime FechaPago { get; set; }
        public decimal MontoBase { get; set; }
        public decimal MontoBono { get; set; }
        public decimal MontoTotal { get; set; }
    }
}