using ManejoAlquileres.Models;
using ManejoAlquileres.Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace ManejoAlquileres.Service
{
    public class PropiedadUsuarioService : IPropiedadUsuarioService
    {
        private readonly ApplicationDbContext _context;

        public PropiedadUsuarioService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(PropiedadUsuario propiedadUsuario)
        {
            _context.PropiedadesUsuarios.Add(propiedadUsuario);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string usuarioId, string propiedadId)
        {
            var entity = await _context.PropiedadesUsuarios
                .FirstOrDefaultAsync(pu => pu.UsuarioId == usuarioId && pu.PropiedadId == propiedadId);

            if (entity != null)
            {
                _context.PropiedadesUsuarios.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<PropiedadUsuario>> GetAllAsync()
        {
            return await _context.PropiedadesUsuarios
                .Include(pu => pu.Usuario)
                .Include(pu => pu.Propiedad)
                .ToListAsync();
        }

        public async Task<List<PropiedadUsuario>> GetByUsuarioIdAsync(string usuarioId)
        {
            return await _context.PropiedadesUsuarios
                .Where(pu => pu.UsuarioId == usuarioId)
                .Include(pu => pu.Propiedad)
                .ToListAsync();
        }

        public async Task<List<PropiedadUsuario>> GetByPropiedadIdAsync(string propiedadId)
        {
            return await _context.PropiedadesUsuarios
                .Where(pu => pu.PropiedadId == propiedadId)
                .Include(pu => pu.Usuario)
                .ToListAsync();
        }
    }
}