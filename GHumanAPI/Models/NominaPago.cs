namespace GHumanAPI.Models
{
    public class NominaPago
    {
        public int Id { get; set; }
        public int EmpleadoId { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal MontoBase { get; set; }
        public decimal MontoBono { get; set; }
        public decimal MontoTotal { get; set; }
        public Empleado Empleado { get; set; } = null!;
    }
}