using ManejoAlquileres.Models;
using ManejoAlquileres.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ManejoAlquileres.Service
{
    public class ServicioPropiedades : IServicioPropiedades
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ServicioPropiedades(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private string ObtenerUsuarioId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new Exception("No se pudo obtener el ID del usuario.");
        }

        public async Task Crear(Propiedad propiedad)
        {
            _context.Propiedades.Add(propiedad);
            await _context.SaveChangesAsync();
        }

        public async Task<Propiedad> ObtenerPorId(string id)
        {
            return await _context.Propiedades
                .Include(p => p.Habitaciones)
                .FirstOrDefaultAsync(p => p.Id_propiedad == id);
        }

        public async Task<List<Propiedad>> ObtenerTodas()
        {
            return await _context.Propiedades.ToListAsync();
        }

        public async Task Actualizar(Propiedad propiedad)
        {
            _context.Propiedades.Update(propiedad);
            await _context.SaveChangesAsync();
        }

        public async Task Borrar(string id)
        {
            var propiedad = await ObtenerPorId(id);
            if (propiedad is not null)
            {
                _context.Propiedades.Remove(propiedad);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExisteReferenciaCatastral(string referenciaCatastral, string id = null)
        {
            return await _context.Propiedades.AnyAsync(p =>
                p.Referencia_catastral == referenciaCatastral &&
                (id == null || p.Id_propiedad != id));
        }

        public async Task<List<Propiedad>> ObtenerPropiedadesComoPropietarioAsync()
        {
            var id = ObtenerUsuarioId();

            return await _context.Propiedades
                .Where(p => _context.PropiedadesUsuarios
                    .Where(pu => pu.UsuarioId == id)
                    .Select(pu => pu.PropiedadId)
                    .Contains(p.Id_propiedad))
                .Include(p => p.Habitaciones)
                .Distinct()
                .ToListAsync();

        }

        public async Task<List<Propiedad>> ObtenerPropiedadesComoInquilinoAsync()
        {
            var id = ObtenerUsuarioId();

            return await _context.Propiedades
                .Where(p => _context.ContratosInquilinos
                    .Where(ci => ci.UsuarioId == id)
                    .Select(ci => ci.Contrato.Id_propiedad)
                    .Contains(p.Id_propiedad))
                .Include(p => p.Habitaciones)
                .Distinct()
                .ToListAsync();
        }
    }
}
