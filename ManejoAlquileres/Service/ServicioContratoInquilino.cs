using ManejoAlquileres.Models;
using ManejoAlquileres.Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace ManejoAlquileres.Service
{
    public class ServicioContratoInquilino : IContratoInquilino
    {
        private readonly ApplicationDbContext _context;

        public ServicioContratoInquilino(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ContratoInquilino>> GetAllAsync()
        {
            return await _context.ContratosInquilinos
                .Include(ci => ci.Contrato)
                .Include(ci => ci.Usuario)
                .ToListAsync();
        }

        public async Task<List<ContratoInquilino>> GetByContratoIdAsync(string contratoId)
        {
            return await _context.ContratosInquilinos
                .Where(ci => ci.ContratoId == contratoId)
                .Include(ci => ci.Usuario)
                .ToListAsync();
        }
        public async Task<List<ContratoInquilino>> GetByUsuarioIdAsync(string usuarioId)
        {
            return await _context.ContratosInquilinos
                .Where(ci => ci.Usuario.Id_usuario == usuarioId)
                .Include(ci => ci.Contrato)
                .ToListAsync();
        }

        public async Task AddAsync(ContratoInquilino inquilino)
        {
            _context.ContratosInquilinos.Add(inquilino);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string contratoId, string usuarioId)
        {
            var entity = await _context.ContratosInquilinos
                .FirstOrDefaultAsync(ci => ci.ContratoId == contratoId && ci.UsuarioId == usuarioId);

            if (entity != null)
            {
                _context.ContratosInquilinos.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
        public async Task DeleteByContratoIdAsync(string contratoId)
        {
            var entities = await _context.ContratosInquilinos
                .Where(ci => ci.ContratoId == contratoId)
                .ToListAsync();

            if (entities.Any())
            {
                _context.ContratosInquilinos.RemoveRange(entities);
                await _context.SaveChangesAsync();
            }
        }
        public async Task AddRangeAsync(List<ContratoInquilino> inquilinos)
        {
            _context.ContratosInquilinos.AddRange(inquilinos);
            await _context.SaveChangesAsync();
        }

    }
}
