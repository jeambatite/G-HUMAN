namespace GHumanAPI.DTOs
{
    public class PaginacionDTO
    {
        public int Pagina { get; set; } = 1;
        public int TamanoPagina { get; set; } = 10;
    }

    public class ResultadoPaginadoDTO<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public int Total { get; set; }
        public int Pagina { get; set; }
        public int TamanoPagina { get; set; }
        public int TotalPaginas { get; set; }
    }
}