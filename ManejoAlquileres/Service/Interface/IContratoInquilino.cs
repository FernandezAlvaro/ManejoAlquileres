using ManejoAlquileres.Models;

namespace ManejoAlquileres.Service.Interface
{
    public interface IContratoInquilino
    {
        Task<List<ContratoInquilino>> GetAllAsync();
        Task<List<ContratoInquilino>> GetByContratoIdAsync(string contratoId);
        Task<List<ContratoInquilino>> GetByUsuarioIdAsync(string usuarioId);
        Task AddAsync(ContratoInquilino inquilino);
        Task DeleteAsync(string contratoId, string usuarioId);
        Task DeleteByContratoIdAsync(string contratoId);
        Task AddRangeAsync(List<ContratoInquilino> inquilinos);
    }
}
