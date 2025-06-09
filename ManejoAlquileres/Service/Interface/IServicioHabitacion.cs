using ManejoAlquileres.Models;

namespace ManejoAlquileres.Service.Interface
{
    public interface IServicioHabitacion
    {
        Task Crear(Habitacion habitacion);
        Task<Habitacion> ObtenerPorId(string id);
        Task<IEnumerable<Habitacion>> ObtenerTodas();
        Task Actualizar(Habitacion habitacion);
        Task Borrar(string id);
        Task<bool> Existe(string id);
    }
}
