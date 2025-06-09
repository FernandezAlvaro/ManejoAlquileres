using ManejoAlquileres.Models;

namespace ManejoAlquileres.Service.Interface
{
    public interface IPropiedadUsuario
    {
        Task<List<PropiedadUsuario>> GetAllAsync();
        Task<List<PropiedadUsuario>> GetByUsuarioIdAsync(string usuarioId);
        Task<List<PropiedadUsuario>> GetByPropiedadIdAsync(string propiedadId);
        Task AddAsync(PropiedadUsuario propiedadUsuario);
        Task DeleteAsync(string usuarioId, string propiedadId);
    }
}
