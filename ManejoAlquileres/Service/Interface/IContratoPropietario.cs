using ManejoAlquileres.Models;

namespace ManejoAlquileres.Service.Interface
{
    public interface IContratoPropietario
    {
        Task<List<ContratoPropietario>> GetAllAsync();
        Task<List<ContratoPropietario>> GetByContratoIdAsync(string contratoId);
        Task AddAsync(ContratoPropietario propietario);
        Task DeleteAsync(string contratoId, string usuarioId);
    }
}
