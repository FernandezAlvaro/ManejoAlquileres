using ManejoAlquileres.Models;
using ManejoAlquileres.Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace ManejoAlquileres.Service
{
    public class ServicioHabitacion : IServicioHabitacion
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ServicioHabitacion(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Crear(Habitacion habitacion)
        {
            _context.Habitaciones.Add(habitacion);
            await _context.SaveChangesAsync();
        }

        public async Task<Habitacion> ObtenerPorId(string id)
        {
            return await _context.Habitaciones
                .FirstOrDefaultAsync(h => h.Id_habitacion == id);
        }

        public async Task<List<Habitacion>> ObtenerTodas()
        {
            return await _context.Habitaciones
                .Include(h => h.Propiedad)
                .ToListAsync();
        }

        public async Task Actualizar(Habitacion habitacion)
        {
            _context.Habitaciones.Update(habitacion);
            await _context.SaveChangesAsync();
        }

        public async Task Borrar(string id)
        {
            var habitacion = await ObtenerPorId(id);
            if (habitacion != null)
            {
                _context.Habitaciones.Remove(habitacion);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> Existe(string id)
        {
            return await _context.Habitaciones.AnyAsync(h => h.Id_habitacion == id);
        }

        public async Task<List<Habitacion>> ObtenerPorPropiedad(string propiedadId)
        {
            return await _context.Habitaciones
                .Where(h => h.Id_propiedad == propiedadId)
                .ToListAsync();
        }
        public async Task<Habitacion> ObtenerConDetalles(string id)
        {
            return await _context.Habitaciones
                .Include(h => h.Propiedad)
                .Include(h => h.Contratos)
                .FirstOrDefaultAsync(h => h.Id_habitacion == id);
        }
        public async Task<Habitacion> ObtenerHabitacionConSuIdyPropiedadId(string idHabitación, string idPropiedad)
        {
            return await _context.Habitaciones
                .Where(h => idHabitación.Equals(h.Id_habitacion) && idPropiedad.Equals(h.Id_propiedad))
                .FirstOrDefaultAsync();
        }
        public async Task<List<Habitacion>> ObtenerPorListaIds(List<string> ids)
        {
            return await _context.Habitaciones
                .Where(h => ids.Contains(h.Id_habitacion))
                .ToListAsync();
        }
    }
}