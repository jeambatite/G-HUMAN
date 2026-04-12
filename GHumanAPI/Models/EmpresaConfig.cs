namespace GHumanAPI.Models
{
    public class EmpresaConfig
    {
        public int Id { get; set; }
        public decimal BalanceActual { get; set; }
        public int UltimaNominaMes { get; set; }
        public int DiaPago { get; set; } = 30;
        public string EmailAdmin { get; set; } = string.Empty;
        public string SmtpPasswordHash { get; set; } = string.Empty;
        public string TestRunKeyHash { get; set; } = string.Empty;
    }
}