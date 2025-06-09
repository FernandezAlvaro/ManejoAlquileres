using ManejoAlquileres.Models.Helpers;
using ManejoAlquileres.Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace ManejoAlquileres.Service
{
    public class GeneradorIdsService : IGeneradorIdsService
    {
        private readonly GeneradorIds _generadorIds;

        public GeneradorIdsService(ApplicationDbContext context)
        {
            _generadorIds = new GeneradorIds(async id =>
                await context.Habitaciones.AnyAsync(h => h.Id_habitacion == id) ||
                await context.GastosInmueble.AnyAsync(g => g.Id_gasto == id));
        }

        public async Task<string> GenerarIdUnicoAsync()
        {
            return await _generadorIds.GenerarIdUnicoAsync();
        }
    }
}