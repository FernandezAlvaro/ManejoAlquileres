using ManejoAlquileres.Models;
using Microsoft.EntityFrameworkCore;

namespace ManejoAlquileres.Service.Interface
{
    public interface IServicioHabitacion
    {
        Task Crear(Habitacion habitacion);
        Task<Habitacion> ObtenerPorId(string id);
        Task<List<Habitacion>> ObtenerTodas();
        Task Actualizar(Habitacion habitacion);
        Task Borrar(string id);
        Task<Habitacion> ObtenerConDetalles(string id);
        Task<bool> Existe(string id);
        Task<List<Habitacion>> ObtenerPorPropiedad(string propiedadId);
        Task<Habitacion> ObtenerHabitacionConSuIdyPropiedadId(string idHabitación, string idPropiedad);
        Task<List<Habitacion>> ObtenerPorListaIds(List<string> ids);
    }
}
