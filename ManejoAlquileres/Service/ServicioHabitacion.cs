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

        public async Task<Habitacion?> ObtenerPorId(string id)
        {
            return await _context.Habitaciones
                .FirstOrDefaultAsync(h => h.Id_habitacion == id);
        }

        public async Task<IEnumerable<Habitacion>> ObtenerTodas()
        {
            return await _context.Habitaciones.ToListAsync();
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
    }
}