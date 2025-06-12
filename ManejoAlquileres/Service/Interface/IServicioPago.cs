using ManejoAlquileres.Models;
using ManejoAlquileres.Models.DTO;

namespace ManejoAlquileres.Service.Interface
{
    public interface IServicioPago
    {
        Task Crear(Pago pago);
        Task<Pago> ObtenerPorId(string id);
        Task<IEnumerable<Pago>> ObtenerTodos();
        Task Actualizar(Pago pago);
        Task Borrar(string id);
        Task<bool> Existe(string id);
        Task<List<Pago>> ObtenerPagosARealizarPorUsuarioAsync(string idUsuario);
        Task<List<Pago>> ObtenerPagosARecibirPorUsuarioAsync(string idUsuario);
        Task<List<PagoConContratoDTO>> ObtenerPagosConDatosContrato();
    }
}
