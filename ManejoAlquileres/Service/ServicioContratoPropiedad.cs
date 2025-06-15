using ManejoAlquileres.Models;
using ManejoAlquileres.Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace ManejoAlquileres.Service
{
    public class ServicioContratoPropiedad : IContratoPropietario
    {
        private readonly ApplicationDbContext _context;

        public ServicioContratoPropiedad(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ContratoPropietario propietario)
        {
            _context.ContratosPropietarios.Add(propietario);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string contratoId, string usuarioId)
        {
            var entity = await _context.ContratosPropietarios
                .FirstOrDefaultAsync(ci => ci.ContratoId == contratoId && ci.UsuarioId == usuarioId);

            if (entity != null)
            {
                _context.ContratosPropietarios.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
        public async Task DeleteByContratoIdAsync(string contratoId)
        {
            var entities = await _context.ContratosPropietarios
                .Where(ci => ci.ContratoId == contratoId)
                .ToListAsync();

            if (entities.Any())
            {
                _context.ContratosPropietarios.RemoveRange(entities);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<List<ContratoPropietario>> GetAllAsync()
        {
            var entities = await _context.ContratosPropietarios
                .ToListAsync();
            return entities;
        }

        public async Task<List<ContratoPropietario>> GetByContratoIdAsync(string contratoId)
        {
            var entities = await _context.ContratosPropietarios
                .Where(ci => ci.ContratoId == contratoId)
                .Include(ci => ci.Usuario)
                .ToListAsync();
            return entities;
        }
    }
}
