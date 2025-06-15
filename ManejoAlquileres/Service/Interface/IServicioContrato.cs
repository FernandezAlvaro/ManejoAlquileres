using ManejoAlquileres.Models;

namespace ManejoAlquileres.Service.Interface
{
    public interface IServicioContrato
    {
        Task Crear(Contrato contrato);
        Task<Contrato> ObtenerPorId(string id);
        Task<List<Contrato>> ObtenerTodos();
        Task Actualizar(Contrato contrato);
        Task Borrar(string id);
        Task<bool> Existe(string id);
        Task<List<Contrato>> ObtenerContratosDelUsuario(string userId);
        Task<Contrato> Eliminar(string id);
    }
}
