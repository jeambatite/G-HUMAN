using Microsoft.EntityFrameworkCore;
using GHumanAPI.Data;
using GHumanAPI.DTOs;
using GHumanAPI.Models;

namespace GHumanAPI.Services
{
    public class ReclutamientoService : IReclutamientoService
    {
        private readonly AppDbContext _context;

        public ReclutamientoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<PostulanteDTO>> GetPostulantes(string? estado, string? puesto, string? busquedaCv)
        {
            var query = _context.Postulantes.AsQueryable();
            if (!string.IsNullOrEmpty(puesto))
                query = query.Where(p => p.PuestoAplicado != null &&
                    p.PuestoAplicado.ToLower().Contains(puesto.ToLower()));

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(p => p.Estado == estado);

            if (!string.IsNullOrEmpty(puesto))
                query = query.Where(p => p.PuestoAplicado != null && p.PuestoAplicado.Contains(puesto));

            var postulantes = await query.OrderByDescending(p => p.FechaPostulacion).ToListAsync();

            var filtros = await _context.FiltrosAts.Where(f => f.Activo).ToListAsync();

            return postulantes.Select(p => MapToDTO(p, filtros, busquedaCv)).ToList();
        }

        public async Task<PostulanteDTO?> GetById(int id)
        {
            var p = await _context.Postulantes.FindAsync(id);
            if (p == null) return null;
            return MapToDTO(p, new List<FiltroAts>(), null);
        }

        public async Task<PostulanteDTO> Crear(CrearPostulanteDTO dto)
        {
            var postulante = new Postulante
            {
                Nombre = dto.Nombre,
                Email = dto.Email,
                Telefono = dto.Telefono,
                PuestoAplicado = dto.PuestoAplicado,
                ExperienciaAnios = dto.ExperienciaAnios,
                NivelEducacion = dto.NivelEducacion,
                CvUrl = dto.CvUrl,
                CvTexto = dto.CvTexto,
                FechaPostulacion = DateTime.UtcNow,
                Estado = "pendiente"
            };

            _context.Postulantes.Add(postulante);
            await _context.SaveChangesAsync();
            return MapToDTO(postulante, new List<FiltroAts>(), null);
        }

        public async Task<bool> ActualizarEstado(int id, ActualizarEstadoPostulanteDTO dto)
        {
            var postulante = await _context.Postulantes.FindAsync(id);
            if (postulante == null) return false;
            postulante.Estado = dto.Estado;
            if (dto.Notas != null) postulante.Notas = dto.Notas;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Eliminar(int id)
        {
            var postulante = await _context.Postulantes.FindAsync(id);
            if (postulante == null) return false;
            _context.Postulantes.Remove(postulante);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<FiltroAtsDTO>> GetFiltros()
        {
            return await _context.FiltrosAts
                .OrderByDescending(f => f.CreadoEn)
                .Select(f => new FiltroAtsDTO
                {
                    Id = f.Id,
                    Nombre = f.Nombre,
                    PalabrasClave = f.PalabrasClave,
                    Activo = f.Activo,
                    CreadoEn = f.CreadoEn
                })
                .ToListAsync();
        }

        public async Task<FiltroAtsDTO> CrearFiltro(CrearFiltroAtsDTO dto)
        {
            var filtro = new FiltroAts
            {
                Nombre = dto.Nombre,
                PalabrasClave = dto.PalabrasClave,
                Activo = true,
                CreadoEn = DateTime.UtcNow
            };
            _context.FiltrosAts.Add(filtro);
            await _context.SaveChangesAsync();
            return new FiltroAtsDTO
            {
                Id = filtro.Id,
                Nombre = filtro.Nombre,
                PalabrasClave = filtro.PalabrasClave,
                Activo = filtro.Activo,
                CreadoEn = filtro.CreadoEn
            };
        }

        public async Task<bool> EliminarFiltro(int id)
        {
            var filtro = await _context.FiltrosAts.FindAsync(id);
            if (filtro == null) return false;
            _context.FiltrosAts.Remove(filtro);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleFiltro(int id)
        {
            var filtro = await _context.FiltrosAts.FindAsync(id);
            if (filtro == null) return false;
            filtro.Activo = !filtro.Activo;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<PostulanteDTO>> AplicarFiltrosAts()
        {
            var postulantes = await _context.Postulantes.ToListAsync();
            var filtros = await _context.FiltrosAts.Where(f => f.Activo).ToListAsync();
            return postulantes
                .Select(p => MapToDTO(p, filtros, null))
                .Where(p => p.PalabrasEncontradas.Count > 0)
                .OrderByDescending(p => p.PalabrasEncontradas.Count)
                .ToList();
        }

        private PostulanteDTO MapToDTO(Postulante p, List<FiltroAts> filtros, string? busquedaCv)
        {
            var palabrasEncontradas = new List<string>();

            var textoCompleto = $"{p.CvTexto} {p.PuestoAplicado} {p.NivelEducacion}".ToLower();

            foreach (var filtro in filtros)
            {
                var palabras = filtro.PalabrasClave.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var palabra in palabras)
                {
                    var p2 = palabra.Trim().ToLower();
                    if (!string.IsNullOrEmpty(p2) && textoCompleto.Contains(p2))
                        palabrasEncontradas.Add(p2);
                }
            }

            if (!string.IsNullOrEmpty(busquedaCv) && !string.IsNullOrEmpty(p.CvTexto))
            {
                var terminos = busquedaCv.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var t in terminos)
                {
                    if (p.CvTexto.ToLower().Contains(t.ToLower()))
                        palabrasEncontradas.Add(t.ToLower());
                }
            }

            return new PostulanteDTO
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Email = p.Email,
                Telefono = p.Telefono,
                PuestoAplicado = p.PuestoAplicado,
                ExperienciaAnios = p.ExperienciaAnios,
                NivelEducacion = p.NivelEducacion,
                CvUrl = p.CvUrl,
                TieneCv = !string.IsNullOrEmpty(p.CvUrl),
                FechaPostulacion = p.FechaPostulacion,
                Estado = p.Estado,
                Notas = p.Notas,
                PalabrasEncontradas = palabrasEncontradas.Distinct().ToList()
            };
        }
    }
}