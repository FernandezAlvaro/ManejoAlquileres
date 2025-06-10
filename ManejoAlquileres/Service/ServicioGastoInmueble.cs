using ManejoAlquileres.Models;
using ManejoAlquileres.Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace ManejoAlquileres.Service
{
    public class ServicioGastoInmueble:IServicioGastoInmueble
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ServicioGastoInmueble(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Crear(GastoInmueble gasto)
        {
            _context.GastosInmueble.Add(gasto);
            await _context.SaveChangesAsync();
        }

        public async Task<GastoInmueble?> ObtenerPorId(string id)
        {
            return await _context.GastosInmueble
                .FirstOrDefaultAsync(g => g.Id_gasto == id);
        }

        public async Task<List<GastoInmueble>> ObtenerTodos()
        {
            return await _context.GastosInmueble
                .Include(g => g.Propiedad)
                .ToListAsync();
        }

        public async Task Actualizar(GastoInmueble gasto)
        {
            _context.GastosInmueble.Update(gasto);
            await _context.SaveChangesAsync();
        }

        public async Task Borrar(string id)
        {
            var gasto = await ObtenerPorId(id);
            if (gasto != null)
            {
                _context.GastosInmueble.Remove(gasto);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> Existe(string id)
        {
            return await _context.GastosInmueble.AnyAsync(g => g.Id_gasto == id);
        }

        public async Task<GastoInmueble> ObtenerConPropiedad(string id)
        {
            return await _context.GastosInmueble
                .Include(g => g.Propiedad)
                .FirstOrDefaultAsync(g => g.Id_gasto == id);
        }
        public async Task<List<GastoInmueble>> ObtenerPorPropiedades(List<string> propiedadIds)
        {
            return await _context.GastosInmueble
                .Where(g => propiedadIds.Contains(g.Id_propiedad))
                .ToListAsync();
        }
    }
}