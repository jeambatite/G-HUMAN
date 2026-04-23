namespace GHumanAPI.Models
{
    public class FiltroAts
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string PalabrasClave { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    }
}